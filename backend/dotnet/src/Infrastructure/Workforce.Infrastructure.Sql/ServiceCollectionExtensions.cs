using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.Infrastructure.Sql.Options;
using Workforce.Infrastructure.Sql.Repositories;
using Workforce.Infrastructure.Sql.Seeding;

namespace Workforce.Infrastructure.Sql;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("ConnectionStrings");
        var connectionString = section["Sql"] ?? string.Empty;

        services.AddDbContext<WorkforceDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IDesignationRepository, DesignationRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ISqlSeedData, SqlSeedData>();

        services.Configure<SqlOptions>(options => options.ConnectionString = connectionString);
        return services;
    }
}
