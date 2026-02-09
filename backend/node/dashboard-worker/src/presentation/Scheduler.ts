import { DashboardSummaryService } from '../application/DashboardSummaryService.js';
import { logger } from '../infrastructure/logger.js';

export async function runSummaryOnce(
  service: DashboardSummaryService,
  retryMaxAttempts: number,
  retryBaseDelayMs: number
): Promise<Awaited<ReturnType<DashboardSummaryService['buildAndStore']>>> {
  return runWithRetry(
    () => service.buildAndStore(),
    retryMaxAttempts,
    retryBaseDelayMs
  );
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
      await new Promise(resolve => setTimeout(resolve, delay));
    }
  }

  throw lastError;
}
