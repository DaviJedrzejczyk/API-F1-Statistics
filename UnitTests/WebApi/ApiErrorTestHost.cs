using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Config;
using WebApi.Controllers.Sessions;

namespace UnitTests.WebApi
{
    /// <summary>
    /// Runs the real controllers and error pipeline on a local Kestrel port with services supplied by the test.
    /// No hosted services are registered, so background workers and external APIs never start.
    /// </summary>
    public sealed class ApiErrorTestHost : IAsyncDisposable
    {
        private readonly WebApplication _app;

        private ApiErrorTestHost(WebApplication app)
        {
            _app = app;
            Client = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };
        }

        public HttpClient Client { get; }
        public IServiceProvider Services => _app.Services;

        public static async Task<ApiErrorTestHost> StartAsync(Action<IServiceCollection> registerServices)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ApplicationName = typeof(SessionController).Assembly.GetName().Name
            });
            builder.WebHost.UseUrls("http://127.0.0.1:0");

            registerServices(builder.Services);
            builder.Services.AddApiErrorContracts();
            builder.Services
                .AddControllers()
                .AddApplicationPart(typeof(SessionController).Assembly)
                .AddJsonConfiguration();

            var app = builder.Build();
            app.UseApiErrorContracts();
            app.MapControllers();
            await app.StartAsync();

            return new ApiErrorTestHost(app);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }
}
