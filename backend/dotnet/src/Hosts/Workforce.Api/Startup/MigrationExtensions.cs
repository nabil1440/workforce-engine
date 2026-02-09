using Microsoft.EntityFrameworkCore;
using Workforce.Infrastructure.Sql;

namespace Workforce.Api.Startup;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WorkforceDbContext>();
        dbContext.Database.Migrate();
    }
}
