import { PrismaClient } from '@prisma/client';
import {
  HeadcountByDepartment,
  TasksByStatus
} from '../../domain/DashboardSummary.js';
import { SqlDashboardReader } from '../../application/ports.js';

const TASK_STATUS_LABELS = new Map<number, string>([
  [0, 'Todo'],
  [1, 'InProgress'],
  [2, 'Review'],
  [3, 'Done']
]);

export class SqlDashboardRepository implements SqlDashboardReader {
  constructor(private readonly client: PrismaClient) {}

  async getHeadcountByDepartment(): Promise<HeadcountByDepartment[]> {
    const [departments, counts] = await Promise.all([
      this.client.department.findMany({ select: { id: true, name: true } }),
      this.client.employee.groupBy({
        by: ['departmentId'],
        _count: { _all: true }
      })
    ]);

    const nameById = new Map(departments.map(dept => [dept.id, dept.name]));
    return counts.map(entry => ({
      department: nameById.get(entry.departmentId) ?? 'Unknown',
      count: entry._count._all
    }));
  }

  async getActiveProjectsCount(): Promise<number> {
    return this.client.project.count({
      where: { status: 0 }
    });
  }

  async getTasksByStatus(): Promise<TasksByStatus[]> {
    const grouped = await this.client.workTask.groupBy({
      by: ['status'],
      _count: { _all: true }
    });

    const counts = new Map(
      grouped.map(entry => [entry.status, entry._count._all])
    );

    return Array.from(TASK_STATUS_LABELS.entries()).map(([status, label]) => ({
      status: label,
      count: counts.get(status) ?? 0
    }));
  }
}
