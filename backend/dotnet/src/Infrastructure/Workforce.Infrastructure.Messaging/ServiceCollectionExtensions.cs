using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Workforce.AppCore.Abstractions;
using Workforce.Infrastructure.Messaging.Options;

namespace Workforce.Infrastructure.Messaging;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessagingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitOptions>(configuration.GetSection("Rabbit"));

        services.AddSingleton(sp => sp.GetRequiredService<IOptions<RabbitOptions>>().Value);
        services.AddSingleton<IConnection>(sp =>
        {
            var options = sp.GetRequiredService<RabbitOptions>();
            if (string.IsNullOrWhiteSpace(options.UserName) || string.IsNullOrWhiteSpace(options.Password))
            {
                throw new InvalidOperationException("RabbitMQ credentials must be configured via the Rabbit section.");
            }

            var factory = new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password,
                VirtualHost = options.VirtualHost,
                DispatchConsumersAsync = true
            };

            return factory.CreateConnection(options.ClientProvidedName ?? "workforce");
        });

        services.AddSingleton<IEventPublisher, RabbitEventPublisher>();
        return services;
    }
}
