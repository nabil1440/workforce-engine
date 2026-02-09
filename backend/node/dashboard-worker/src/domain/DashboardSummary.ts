export type HeadcountByDepartment = {
  department: string;
  count: number;
};

export type TasksByStatus = {
  status: string;
  count: number;
};

export type LeaveStat = {
  type: string;
  status: string;
  count: number;
};

export type DashboardSummary = {
  summaryKey: string;
  generatedAt: Date;
  headcountByDepartment: HeadcountByDepartment[];
  activeProjectsCount: number;
  tasksByStatus: TasksByStatus[];
  leaveStats: LeaveStat[];
};
