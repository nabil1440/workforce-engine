using Workforce.Api.Startup;
using Workforce.AppCore.Services;
using Workforce.AppCore.Services.Implementations;
using Workforce.Infrastructure.Messaging;
using Workforce.Infrastructure.Mongo;
using Workforce.Infrastructure.Sql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddSqlInfrastructure(builder.Configuration);
builder.Services.AddMongoInfrastructure(builder.Configuration);
builder.Services.AddMessagingInfrastructure(builder.Configuration);

builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectMemberService, ProjectMemberService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
if (!app.Configuration.GetValue("SkipMigrationsOnStartup", false))
{
    var seedData = app.Configuration.GetValue("SeedDataOnStartup", true);
    await app.ApplyMigrationsAsync(seedData);
}

app.Run();

public partial class Program;
