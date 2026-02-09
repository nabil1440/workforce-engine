using MongoDB.Driver;
using Workforce.AppCore.Domain.Audit;
using Workforce.AppCore.Domain.Dashboard;
using Workforce.AppCore.Domain.Leaves;

namespace Workforce.Infrastructure.Mongo;

public sealed class MongoContext
{
    public MongoContext(IMongoDatabase database)
    {
        Database = database;
    }

    public IMongoDatabase Database { get; }

    public IMongoCollection<LeaveRequest> LeaveRequests => Database.GetCollection<LeaveRequest>("leave_requests");
    public IMongoCollection<AuditLog> AuditLogs => Database.GetCollection<AuditLog>("audit_logs");
    public IMongoCollection<DashboardSummary> DashboardSummaries => Database.GetCollection<DashboardSummary>("dashboard_summaries");
}
