using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Workforce.AuditWorker;
using Workforce.Infrastructure.Messaging;
using Workforce.Infrastructure.Mongo;

var builder = WebApplication.CreateBuilder(args);
var healthUrl = builder.Configuration.GetValue("Health:Url", "http://0.0.0.0:8081");

builder.WebHost.UseUrls(healthUrl);

builder.Services.AddHealthChecks();
builder.Services.AddMongoInfrastructure(builder.Configuration);
builder.Services.AddMessagingInfrastructure(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var app = builder.Build();
app.MapHealthChecks("/health");

app.Run();
