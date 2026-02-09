using Microsoft.EntityFrameworkCore;
using Workforce.Infrastructure.Sql;
using Workforce.Infrastructure.Sql.Seeding;
using Workforce.Infrastructure.Mongo.Seeding;

namespace Workforce.Api.Startup;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(
        this IApplicationBuilder app,
        bool seedData,
        CancellationToken cancellationToken = default)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WorkforceDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);

        if (!seedData)
        {
            return;
        }

        var seeder = scope.ServiceProvider.GetRequiredService<ISqlSeedData>();
        await seeder.SeedAsync(cancellationToken);

        var employees = await dbContext.Employees
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var mongoSeeder = scope.ServiceProvider.GetRequiredService<IMongoSeedData>();
        await mongoSeeder.SeedLeaveRequestsAsync(employees, cancellationToken);
    }
}
