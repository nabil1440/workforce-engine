import { Collection, Db } from 'mongodb';
import { MongoLeaveStatsReader } from '../../application/ports.js';
import { LeaveStat } from '../../domain/DashboardSummary.js';

type LeaveAggregateResult = {
  type: string;
  status: string;
  count: number;
};

export class MongoLeaveRepository implements MongoLeaveStatsReader {
  private readonly collection: Collection;

  constructor(database: Db) {
    this.collection = database.collection('leave_requests');
  }

  async getLeaveStats(): Promise<LeaveStat[]> {
    const pipeline = [
      {
        $group: {
          _id: { type: '$leaveType', status: '$status' },
          count: { $sum: 1 }
        }
      },
      {
        $project: {
          _id: 0,
          type: '$_id.type',
          status: '$_id.status',
          count: 1
        }
      }
    ];

    const results = await this.collection
      .aggregate<LeaveAggregateResult>(pipeline)
      .toArray();
    return results.map(entry => ({
      type: entry.type,
      status: entry.status,
      count: entry.count
    }));
  }
}
