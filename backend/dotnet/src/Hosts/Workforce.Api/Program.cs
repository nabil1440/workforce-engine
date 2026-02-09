using System.Reflection;
using System.Text;
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
builder.Services.AddHealthChecks();
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

// Swagger UI bundled with Swashbuckle 6.x does not support OpenAPI 3.0.4
// emitted by Microsoft.OpenApi >= 1.6.  Rewrite the spec version to 3.0.1.
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    if (path != null && path.EndsWith("swagger.json", StringComparison.OrdinalIgnoreCase))
    {
        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        await next();

        buffer.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(buffer).ReadToEndAsync();
        json = json.Replace("\"openapi\": \"3.0.4\"", "\"openapi\": \"3.0.1\"")
                   .Replace("\"openapi\":\"3.0.4\"", "\"openapi\":\"3.0.1\"");

        context.Response.Body = originalBody;
        context.Response.ContentLength = Encoding.UTF8.GetByteCount(json);
        await context.Response.WriteAsync(json);
    }
    else
    {
        await next();
    }
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Workforce API v1");
    options.RoutePrefix = "swagger";
});

if (app.Configuration.GetValue("UseHttpsRedirection", false))
{
    app.UseHttpsRedirection();
}
app.MapHealthChecks("/health");
app.MapControllers();
if (!app.Configuration.GetValue("SkipMigrationsOnStartup", false))
{
    var seedData = app.Configuration.GetValue("SeedDataOnStartup", true);
    await app.ApplyMigrationsAsync(seedData);
}

app.Run();

public partial class Program;
