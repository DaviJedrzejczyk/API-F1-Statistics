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
                var tryCount = 0;

                while (tryCount < 3 && !stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        if (DateTime.Now.DayOfWeek != DayOfWeek.Monday)
                        {
                            LogResponse(new Response() { HasSuccess = false, Message = "This method can only be called on Mondays." });
                        }
                        else
                        {
                            using var scope = _serviceScopeFactory.CreateScope();

                            var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();

                            var response = await sessionService.UpdateRecentSession();
                            if(response.HasSuccess)
                               LogResponse(response);
                            else
                            {
                                _logger.LogError("An error occurred while updating the sessions: {Message}", response.Message);
                                tryCount++;
                                continue;
                            }
                        }

                        break;
                    }
                    catch (Exception ex)
                    {
                        tryCount++;
                        if (tryCount < 3)
                        {
                            _logger.LogInformation("Retrying in 1 minute...");
                            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                        }
                        else
                        {
                            _logger.LogError(ex, "An error occurred while updating the calendar. Tried 3 times.");
                        }
                    }
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
                _logger.LogWarning(response.Message);
            }
            else if (response.Message == "No recent meeting key found.")
            {
                _logger.LogWarning(response.Message);
            }
            else if (response.Message == "Recent session already exists, no update needed.")
            {
                _logger.LogWarning(response.Message);
            }
            else
            {
                _logger.LogWarning("Something when try to update session happend and not mapped: " + response.Message);
            }
        }

        private static int GetDaysUntilNextMonday()
        {
            int daysUntilNextMonday = ((int)DayOfWeek.Monday - (int)DateTime.Now.DayOfWeek + 7) % 7;
            return daysUntilNextMonday == 0 ? 7 : daysUntilNextMonday; 
        }
    }
}
