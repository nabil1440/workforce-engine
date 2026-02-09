import 'dotenv/config';

export type WorkerConfig = {
  sqlUrl: string;
  mongoConnectionString: string;
  mongoDatabase: string;
  healthPort: number;
  retryMaxAttempts: number;
  retryBaseDelayMs: number;
  rabbitUrl: string;
  rabbitExchangePrefix: string;
  rabbitQueueName: string;
  rabbitPrefetch: number;
};

function requireValue(value: string | undefined, name: string): string {
  if (!value || value.trim().length === 0) {
    throw new Error(`Missing required environment variable: ${name}`);
  }

  return value;
}

function buildSqlUrl(): string {
  const existing = process.env.DATABASE_URL;
  if (existing && existing.trim().length > 0) {
    return existing;
  }

  const host = process.env.POSTGRES_HOST ?? 'localhost';
  const port = process.env.POSTGRES_PORT ?? '5432';
  const user = process.env.POSTGRES_USER ?? 'workforce';
  const password = process.env.POSTGRES_PASSWORD ?? 'workforce';
  const database = process.env.POSTGRES_DB ?? 'workforce';

  return `postgresql://${encodeURIComponent(user)}:${encodeURIComponent(password)}@${host}:${port}/${database}`;
}

function buildRabbitUrl(): string {
  const existing = process.env.RABBITMQ_URL;
  if (existing && existing.trim().length > 0) {
    return existing;
  }

  const host = process.env.RABBITMQ_HOSTNAME ?? 'localhost';
  const port = process.env.RABBITMQ_PORT ?? '5672';
  const user = process.env.RABBITMQ_USERNAME ?? 'guest';
  const password = process.env.RABBITMQ_PASSWORD ?? 'guest';
  const vhost = process.env.RABBITMQ_VHOST ?? '/';
  const normalizedVhost = vhost.startsWith('/') ? vhost.slice(1) : vhost;
  const encodedVhost = encodeURIComponent(
    normalizedVhost.length > 0 ? normalizedVhost : '/'
  );

  return `amqp://${encodeURIComponent(user)}:${encodeURIComponent(password)}@${host}:${port}/${encodedVhost}`;
}

export function loadConfig(): WorkerConfig {
  const healthPort = Number.parseInt(
    process.env.DASHBOARD_HEALTH_PORT ?? '8090',
    10
  );
  const retryMaxAttempts = Number.parseInt(
    process.env.DASHBOARD_RETRY_MAX ?? '3',
    10
  );
  const retryBaseDelayMs = Number.parseInt(
    process.env.DASHBOARD_RETRY_BASE_MS ?? '1000',
    10
  );
  const rabbitPrefetch = Number.parseInt(
    process.env.DASHBOARD_RABBIT_PREFETCH ?? '10',
    10
  );

  return {
    sqlUrl: buildSqlUrl(),
    mongoConnectionString: requireValue(
      process.env.MONGO_CONNECTION_STRING,
      'MONGO_CONNECTION_STRING'
    ),
    mongoDatabase: process.env.MONGO_DATABASE ?? 'workforce',
    healthPort: Number.isNaN(healthPort) ? 8090 : healthPort,
    retryMaxAttempts: Number.isNaN(retryMaxAttempts) ? 3 : retryMaxAttempts,
    retryBaseDelayMs: Number.isNaN(retryBaseDelayMs) ? 1000 : retryBaseDelayMs,
    rabbitUrl: buildRabbitUrl(),
    rabbitExchangePrefix:
      process.env.RABBITMQ_EXCHANGE_PREFIX ?? 'workforce.events',
    rabbitQueueName:
      process.env.DASHBOARD_RABBIT_QUEUE ?? 'workforce.dashboard',
    rabbitPrefetch: Number.isNaN(rabbitPrefetch) ? 10 : rabbitPrefetch
  };
}
