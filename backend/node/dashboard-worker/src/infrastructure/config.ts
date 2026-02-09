import 'dotenv/config';

export type WorkerConfig = {
  sqlUrl: string;
  mongoConnectionString: string;
  mongoDatabase: string;
  cronSchedule: string;
  healthPort: number;
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

export function loadConfig(): WorkerConfig {
  const healthPort = Number.parseInt(
    process.env.DASHBOARD_HEALTH_PORT ?? "8090",
    10
  );

  return {
    sqlUrl: buildSqlUrl(),
    mongoConnectionString: requireValue(
      process.env.MONGO_CONNECTION_STRING,
      'MONGO_CONNECTION_STRING'
    ),
    mongoDatabase: process.env.MONGO_DATABASE ?? 'workforce',
    cronSchedule: process.env.DASHBOARD_CRON ?? '0 * * * *',
    healthPort: Number.isNaN(healthPort) ? 8090 : healthPort
  };
}
