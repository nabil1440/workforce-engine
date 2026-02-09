import { Collection, Db } from 'mongodb';
import { DashboardSummary } from '../../domain/DashboardSummary.js';
import { MongoDashboardWriter } from '../../application/ports.js';

export class MongoDashboardRepository implements MongoDashboardWriter {
  private readonly collection: Collection<DashboardSummary>;

  constructor(database: Db) {
    this.collection = database.collection<DashboardSummary>(
      'dashboard_summaries'
    );
  }

  async saveSummary(summary: DashboardSummary): Promise<void> {
    await this.collection.updateOne(
      { summaryKey: summary.summaryKey },
      { $set: summary },
      { upsert: true }
    );
  }
}
