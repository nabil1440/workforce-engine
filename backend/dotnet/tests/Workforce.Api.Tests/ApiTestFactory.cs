using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Workforce.AppCore.Services;

namespace Workforce.Api.Tests;

public sealed class ApiTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            var values = new Dictionary<string, string?>
            {
                ["SkipMigrationsOnStartup"] = "true",
                ["SeedDataOnStartup"] = "false"
            };

            config.AddInMemoryCollection(values);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEmployeeService>();
            services.AddSingleton<IEmployeeService, FakeEmployeeService>();
        });
    }
}
