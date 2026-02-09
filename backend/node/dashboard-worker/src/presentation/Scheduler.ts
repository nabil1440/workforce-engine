import cron from 'node-cron';
import { DashboardSummaryService } from '../application/DashboardSummaryService.js';
import { logger } from '../infrastructure/logger.js';

export async function runScheduler(
  service: DashboardSummaryService,
  schedule: string,
  retryMaxAttempts: number,
  retryBaseDelayMs: number
): Promise<void> {
  await runOnce(service, retryMaxAttempts, retryBaseDelayMs);

  cron.schedule(schedule, async () => {
    await runOnce(service, retryMaxAttempts, retryBaseDelayMs);
  });
}

async function runOnce(
  service: DashboardSummaryService,
  retryMaxAttempts: number,
  retryBaseDelayMs: number
): Promise<void> {
  try {
    const summary = await runWithRetry(
      () => service.buildAndStore(),
      retryMaxAttempts,
      retryBaseDelayMs
    );
    logger.info('Dashboard summary generated.', {
      generatedAt: summary.generatedAt.toISOString(),
      activeProjectsCount: summary.activeProjectsCount,
      headcount: summary.headcountByDepartment.length
    });
  } catch (error) {
    logger.error('Failed to generate dashboard summary after retries.', error);
  }
}

async function runWithRetry<T>(
  action: () => Promise<T>,
  maxAttempts: number,
  baseDelayMs: number
): Promise<T> {
  let attempt = 0;
  let lastError: unknown;

  while (attempt < maxAttempts) {
    attempt += 1;
    try {
      return await action();
    } catch (error) {
      lastError = error;
      if (attempt >= maxAttempts) {
        break;
      }

      const delay = baseDelayMs * Math.pow(2, attempt - 1);
      logger.warn('Retrying dashboard summary generation.', {
        attempt,
        maxAttempts,
        delayMs: delay
      });
      await new Promise((resolve) => setTimeout(resolve, delay));
    }
  }

  throw lastError;
}
