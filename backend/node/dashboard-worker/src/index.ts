import http from "node:http";
import { loadConfig } from "./infrastructure/config.js";
import { createSqlClient } from "./infrastructure/sql/SqlClient.js";
import { createMongoClient } from "./infrastructure/mongo/MongoClientFactory.js";
import { SqlDashboardRepository } from "./infrastructure/sql/SqlDashboardRepository.js";
import { MongoLeaveRepository } from "./infrastructure/mongo/MongoLeaveRepository.js";
import { MongoDashboardRepository } from "./infrastructure/mongo/MongoDashboardRepository.js";
import { DashboardSummaryService } from "./application/DashboardSummaryService.js";
import { runScheduler } from "./presentation/Scheduler.js";
import { logger } from "./infrastructure/logger.js";

const config = loadConfig();
const sqlClient = createSqlClient(config.sqlUrl);
const mongoClient = createMongoClient(config.mongoConnectionString);

async function main(): Promise<void> {
  await mongoClient.connect();
  const mongoDatabase = mongoClient.db(config.mongoDatabase);

  const sqlReader = new SqlDashboardRepository(sqlClient);
  const leaveStatsReader = new MongoLeaveRepository(mongoDatabase);
  const dashboardWriter = new MongoDashboardRepository(mongoDatabase);
  const service = new DashboardSummaryService(sqlReader, leaveStatsReader, dashboardWriter);

  const server = startHealthServer(config.healthPort);
  await runScheduler(service, config.cronSchedule);

  logger.info("Dashboard worker started.", {
    schedule: config.cronSchedule,
    healthPort: config.healthPort
  });

  process.on("SIGINT", () => shutdown(server));
  process.on("SIGTERM", () => shutdown(server));
}

main().catch((error) => {
  logger.error("Dashboard worker failed to start.", error);
  process.exitCode = 1;
});

function startHealthServer(port: number): http.Server {
  const server = http.createServer((req, res) => {
    if (req.url?.startsWith("/health")) {
      res.writeHead(200, { "Content-Type": "application/json" });
      res.end(JSON.stringify({ status: "ok", time: new Date().toISOString() }));
      return;
    }

    res.writeHead(404, { "Content-Type": "application/json" });
    res.end(JSON.stringify({ error: "Not Found" }));
  });

  server.listen(port, "0.0.0.0", () => {
    logger.info("Health endpoint listening.", { port });
  });

  return server;
}

async function shutdown(server: http.Server): Promise<void> {
  logger.info("Dashboard worker shutting down.");

  await sqlClient.$disconnect();
  await mongoClient.close();

  server.close(() => {
    process.exit(0);
  });
}
