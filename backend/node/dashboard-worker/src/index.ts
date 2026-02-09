import { loadConfig } from "./infrastructure/config.js";
import { createSqlClient } from "./infrastructure/sql/SqlClient.js";
import { createMongoClient } from "./infrastructure/mongo/MongoClientFactory.js";
import { SqlDashboardRepository } from "./infrastructure/sql/SqlDashboardRepository.js";
import { MongoLeaveRepository } from "./infrastructure/mongo/MongoLeaveRepository.js";
import { MongoDashboardRepository } from "./infrastructure/mongo/MongoDashboardRepository.js";
import { DashboardSummaryService } from "./application/DashboardSummaryService.js";
import { runScheduler } from "./presentation/Scheduler.js";

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

  await runScheduler(service, config.cronSchedule);
}

main().catch((error) => {
  console.error("Dashboard worker failed to start.", error);
  process.exitCode = 1;
});

process.on("SIGINT", async () => {
  await sqlClient.$disconnect();
  await mongoClient.close();
  process.exit(0);
});

process.on("SIGTERM", async () => {
  await sqlClient.$disconnect();
  await mongoClient.close();
  process.exit(0);
});
