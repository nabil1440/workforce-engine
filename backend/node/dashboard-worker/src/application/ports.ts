import {
  HeadcountByDepartment,
  LeaveStat,
  TasksByStatus
} from '../domain/DashboardSummary.js';

export interface SqlDashboardReader {
  getHeadcountByDepartment(): Promise<HeadcountByDepartment[]>;
  getActiveProjectsCount(): Promise<number>;
  getTasksByStatus(): Promise<TasksByStatus[]>;
}

export interface MongoLeaveStatsReader {
  getLeaveStats(): Promise<LeaveStat[]>;
}

export interface MongoDashboardWriter {
  saveSummary(summary: {
    generatedAt: Date;
    headcountByDepartment: HeadcountByDepartment[];
    activeProjectsCount: number;
    tasksByStatus: TasksByStatus[];
    leaveStats: LeaveStat[];
  }): Promise<void>;
}
