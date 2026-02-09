import http from 'node:http';
import { loadConfig } from './infrastructure/config.js';
import { createSqlClient } from './infrastructure/sql/SqlClient.js';
import { createMongoClient } from './infrastructure/mongo/MongoClientFactory.js';
import { SqlDashboardRepository } from './infrastructure/sql/SqlDashboardRepository.js';
import { MongoLeaveRepository } from './infrastructure/mongo/MongoLeaveRepository.js';
import { MongoDashboardRepository } from './infrastructure/mongo/MongoDashboardRepository.js';
import { DashboardSummaryService } from './application/DashboardSummaryService.js';
import { runSummaryOnce } from './presentation/Scheduler.js';
import { logger } from './infrastructure/logger.js';
import { startEventConsumer } from './infrastructure/rabbit/WorkerEventConsumer.js';

const config = loadConfig();
const sqlClient = createSqlClient(config.sqlUrl);
const mongoClient = createMongoClient(config.mongoConnectionString);

async function main(): Promise<void> {
  await connectWithRetry(
    () => mongoClient.connect(),
    config.retryMaxAttempts,
    config.retryBaseDelayMs,
    'MongoDB'
  );
  await connectWithRetry(
    () => sqlClient.$connect(),
    config.retryMaxAttempts,
    config.retryBaseDelayMs,
    'Postgres'
  );
  const mongoDatabase = mongoClient.db(config.mongoDatabase);

  const sqlReader = new SqlDashboardRepository(sqlClient);
  const leaveStatsReader = new MongoLeaveRepository(mongoDatabase);
  const dashboardWriter = new MongoDashboardRepository(mongoDatabase);
  const service = new DashboardSummaryService(
    sqlReader,
    leaveStatsReader,
    dashboardWriter
  );

  const server = startHealthServer(config.healthPort);
  const consumer = await connectWithRetry(
    () =>
      startEventConsumer(
        {
          url: config.rabbitUrl,
          exchangePrefix: config.rabbitExchangePrefix,
          queueName: config.rabbitQueueName,
          prefetch: config.rabbitPrefetch,
          eventTypes: [
            'EmployeeCreated',
            'EmployeeUpdated',
            'EmployeeDeactivated',
            'ProjectCreated',
            'ProjectUpdated',
            'ProjectStatusChanged',
            'TaskCreated',
            'TaskAssigned',
            'TaskStatusChanged',
            'LeaveRequested',
            'LeaveApproved',
            'LeaveRejected',
            'LeaveCancelled'
          ]
        },
        async envelope => {
          logger.info('Dashboard event received.', {
            eventType: envelope.eventType,
            occurredAt: envelope.occurredAt ?? null
          });

          const summary = await runSummaryOnce(
            service,
            config.retryMaxAttempts,
            config.retryBaseDelayMs
          );
          logger.info('Dashboard summary generated.', {
            generatedAt: summary.generatedAt.toISOString(),
            activeProjectsCount: summary.activeProjectsCount,
            headcount: summary.headcountByDepartment.length
          });
        }
      ),
    config.retryMaxAttempts,
    config.retryBaseDelayMs,
    'RabbitMQ'
  );

  logger.info('Dashboard worker started.', {
    healthPort: config.healthPort,
    rabbitQueue: config.rabbitQueueName
  });

  process.on('SIGINT', () => shutdown(server, consumer));
  process.on('SIGTERM', () => shutdown(server, consumer));
}

main().catch(error => {
  logger.error('Dashboard worker failed to start.', error);
  process.exitCode = 1;
});

function startHealthServer(port: number): http.Server {
  const server = http.createServer((req, res) => {
    if (req.url?.startsWith('/health')) {
      res.writeHead(200, { 'Content-Type': 'application/json' });
      res.end(JSON.stringify({ status: 'ok', time: new Date().toISOString() }));
      return;
    }

    res.writeHead(404, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ error: 'Not Found' }));
  });

  server.listen(port, '0.0.0.0', () => {
    logger.info('Health endpoint listening.', { port });
  });

  return server;
}

async function connectWithRetry<T>(
  action: () => Promise<T>,
  maxAttempts: number,
  baseDelayMs: number,
  target: string
): Promise<T> {
  let attempt = 0;
  let lastError: unknown;

  while (attempt < maxAttempts) {
    attempt += 1;
    try {
      const result = await action();
      logger.info('Connection established.', { target, attempt });
      return result;
    } catch (error) {
      lastError = error;
      if (attempt >= maxAttempts) {
        break;
      }

      const delay = baseDelayMs * Math.pow(2, attempt - 1);
      logger.warn('Connection attempt failed, retrying.', {
        target,
        attempt,
        maxAttempts,
        delayMs: delay
      });
      await new Promise(resolve => setTimeout(resolve, delay));
    }
  }

  throw lastError;
}

async function shutdown(
  server: http.Server,
  consumer: Awaited<ReturnType<typeof startEventConsumer>>
): Promise<void> {
  logger.info('Dashboard worker shutting down.');

  await consumer.close();
  await sqlClient.$disconnect();
  await mongoClient.close();

  server.close(() => {
    process.exit(0);
  });
}
