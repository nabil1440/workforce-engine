import { DashboardSummary } from '../domain/DashboardSummary.js';
import {
  MongoDashboardWriter,
  MongoLeaveStatsReader,
  SqlDashboardReader
} from './ports.js';

export class DashboardSummaryService {
  constructor(
    private readonly sqlReader: SqlDashboardReader,
    private readonly leaveStatsReader: MongoLeaveStatsReader,
    private readonly dashboardWriter: MongoDashboardWriter
  ) {}

  async buildAndStore(): Promise<DashboardSummary> {
    const [
      headcountByDepartment,
      activeProjectsCount,
      tasksByStatus,
      leaveStats
    ] = await Promise.all([
      this.sqlReader.getHeadcountByDepartment(),
      this.sqlReader.getActiveProjectsCount(),
      this.sqlReader.getTasksByStatus(),
      this.leaveStatsReader.getLeaveStats()
    ]);

    const generatedAt = new Date();
    const summary: DashboardSummary = {
      summaryKey: buildSummaryKey(generatedAt),
      generatedAt,
      headcountByDepartment,
      activeProjectsCount,
      tasksByStatus,
      leaveStats
    };

    await this.dashboardWriter.saveSummary(summary);
    return summary;
  }
}

function buildSummaryKey(date: Date): string {
  return date.toISOString().slice(0, 13);
}
