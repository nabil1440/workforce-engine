import cron from 'node-cron';
import { DashboardSummaryService } from '../application/DashboardSummaryService.js';
import { logger } from '../infrastructure/logger.js';

export async function runScheduler(
  service: DashboardSummaryService,
  schedule: string
): Promise<void> {
  await runOnce(service);

  cron.schedule(schedule, async () => {
    await runOnce(service);
  });
}

async function runOnce(service: DashboardSummaryService): Promise<void> {
  try {
    const summary = await service.buildAndStore();
    logger.info('Dashboard summary generated.', {
      generatedAt: summary.generatedAt.toISOString(),
      activeProjectsCount: summary.activeProjectsCount,
      headcount: summary.headcountByDepartment.length
    });
  } catch (error) {
    logger.error('Failed to generate dashboard summary.', error);
  }
}
