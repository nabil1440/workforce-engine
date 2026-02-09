using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Workforce.AppCore.Domain.Audit;
using Workforce.AppCore.Domain.Dashboard;
using Workforce.AppCore.Domain.Leaves;

namespace Workforce.Infrastructure.Mongo;

public sealed class MongoIndexInitializer : IHostedService
{
    private readonly MongoContext _context;
    private readonly ILogger<MongoIndexInitializer> _logger;

    public MongoIndexInitializer(MongoContext context, ILogger<MongoIndexInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await EnsureAuditIndexesAsync(cancellationToken);
            await EnsureLeaveIndexesAsync(cancellationToken);
            await EnsureDashboardIndexesAsync(cancellationToken);
            _logger.LogInformation("Mongo indexes ensured.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Mongo indexes.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private Task EnsureAuditIndexesAsync(CancellationToken cancellationToken)
    {
        var collection = _context.AuditLogs;
        var indexes = new List<CreateIndexModel<AuditLog>>
        {
            new(Builders<AuditLog>.IndexKeys
                .Ascending(a => a.EntityType)
                .Ascending(a => a.EntityId)
                .Descending(a => a.Timestamp)),
            new(Builders<AuditLog>.IndexKeys
                .Ascending(a => a.EventType)
                .Descending(a => a.Timestamp))
        };

        return collection.Indexes.CreateManyAsync(indexes, cancellationToken);
    }

    private Task EnsureLeaveIndexesAsync(CancellationToken cancellationToken)
    {
        var collection = _context.LeaveRequests;
        var indexes = new List<CreateIndexModel<LeaveRequest>>
        {
            new(Builders<LeaveRequest>.IndexKeys
                .Ascending(l => l.Status)
                .Ascending(l => l.LeaveType)
                .Descending(l => l.CreatedAt)),
            new(Builders<LeaveRequest>.IndexKeys
                .Descending(l => l.StartDate)),
            new(Builders<LeaveRequest>.IndexKeys
                .Descending(l => l.EndDate))
        };

        return collection.Indexes.CreateManyAsync(indexes, cancellationToken);
    }

    private Task EnsureDashboardIndexesAsync(CancellationToken cancellationToken)
    {
        var collection = _context.DashboardSummaries;
        var indexes = new List<CreateIndexModel<DashboardSummary>>
        {
            new(Builders<DashboardSummary>.IndexKeys
                .Descending(d => d.GeneratedAt))
        };

        return collection.Indexes.CreateManyAsync(indexes, cancellationToken);
    }
}
