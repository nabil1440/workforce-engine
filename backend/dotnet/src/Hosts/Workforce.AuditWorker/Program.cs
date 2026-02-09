using Workforce.AuditWorker;
using Workforce.Infrastructure.Messaging;
using Workforce.Infrastructure.Mongo;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddMongoInfrastructure(builder.Configuration);
builder.Services.AddMessagingInfrastructure(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
