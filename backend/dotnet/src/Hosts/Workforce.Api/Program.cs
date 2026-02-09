using System.Reflection;
using Microsoft.OpenApi.Models;
using Workforce.Api.Startup;
using Workforce.AppCore.Services;
using Workforce.AppCore.Services.Implementations;
using Workforce.Infrastructure.Messaging;
using Workforce.Infrastructure.Mongo;
using Workforce.Infrastructure.Sql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Workforce API",
        Version = "v1"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

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

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Workforce API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();
if (!app.Configuration.GetValue("SkipMigrationsOnStartup", false))
{
    var seedData = app.Configuration.GetValue("SeedDataOnStartup", true);
    await app.ApplyMigrationsAsync(seedData);
}

app.Run();

public partial class Program;
