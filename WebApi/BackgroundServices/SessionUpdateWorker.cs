using Services.Interfaces;
using Shared.Responses;

namespace WebApi.BackgroundServices
{
    public class SessionUpdateWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<SessionUpdateWorker> _logger;

        public SessionUpdateWorker(IServiceScopeFactory serviceScopeFactory, ILogger<SessionUpdateWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var incrementDays = GetDaysUntilNextMonday();
                try
                {
                    if (DateTime.Now.DayOfWeek != DayOfWeek.Monday)
                        throw new Exception("This method can only be called on Mondays.");

                    using var scope = _serviceScopeFactory.CreateScope();
                    
                    var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
                    
                    LogResponse(await sessionService.UpdateRecentSession());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while updating the calendar.");
                }
                await Task.Delay(TimeSpan.FromDays(incrementDays), stoppingToken);
            }
        }

        private void LogResponse(Response response)
        {
            if(response.HasSuccess)
            {
                _logger.LogInformation(response.Message);
            }
            else if (response.Message == "This method can only be called on Mondays.")
            {
                _logger.LogWarning("This method can only be called on Mondays.");
            }
            else if (response.Message == "No recent meeting key found.")
            {
                _logger.LogWarning("No recent meeting key found.");
            }
            else
            {
                _logger.LogError("An error occurred while updating the sessions: {Message}", response.Message);
            }
        }

        private static int GetDaysUntilNextMonday()
        {
            int daysUntilNextMonday = ((int)DayOfWeek.Monday - (int)DateTime.Now.DayOfWeek + 7) % 7;
            return daysUntilNextMonday == 0 ? 7 : daysUntilNextMonday; 
        }
    }
}
