using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.Infrastructure.Mongo.Options;
using Workforce.Infrastructure.Mongo.Repositories;
using Workforce.Infrastructure.Mongo.Serialization;

namespace Workforce.Infrastructure.Mongo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoOptions>(configuration.GetSection("Mongo"));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MongoOptions>>().Value;
            return new MongoClient(options.ConnectionString);
        });

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MongoOptions>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            MongoSerialization.Register();
            return client.GetDatabase(options.Database);
        });

        services.AddSingleton<MongoContext>();

        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IDashboardSummaryRepository, DashboardSummaryRepository>();
        services.AddScoped<IAuditLogWriter, AuditLogWriter>();

        return services;
    }
}
