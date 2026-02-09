import { PrismaClient } from '@prisma/client';

export function createSqlClient(databaseUrl: string): PrismaClient {
  return new PrismaClient({
    datasources: {
      db: { url: databaseUrl }
    }
  });
}
