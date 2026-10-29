using Dao;
using ExternalApi.Impls;
using ExternalApi.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using WebApi.BackgroundServices;

namespace IntegrationTests.Infrastructure;

public sealed class ApiFactory(string connectionString, FixtureHttpHandler handler) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:F1DBLocal"] = connectionString
        }));
        builder.ConfigureTestServices(services =>
        {
            foreach (var descriptor in services.Where(d => d.ServiceType == typeof(IHostedService) &&
                         d.ImplementationType == typeof(SessionUpdateWorker)).ToArray())
                services.Remove(descriptor);
            services.RemoveAll<ApiF1DB>();
            services.RemoveAll<DbContextOptions<ApiF1DB>>();
            services.AddDbContext<ApiF1DB>(o => o.UseSqlServer(connectionString));
            services.AddSingleton<IHttpMessageHandlerBuilderFilter>(new FixtureHttpFilter(handler));
            services.RemoveAll<IF1ApiClient>();
            services.AddHttpClient<IF1ApiClient, F1ApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://fixtures.invalid/v1/");
                client.Timeout = TimeSpan.FromSeconds(5);
            });
        });
    }
}
