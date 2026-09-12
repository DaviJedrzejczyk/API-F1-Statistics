using Dao.Impl;
using Dao.Interface;
using ExternalApi.Impls;
using ExternalApi.Interfaces;
using Services.Impl;
using Services.Interfaces;

namespace WebApi.Config
{
    /// <summary>
    /// Class of extension methods for configuring dependency injection in the application. 
    /// This class provides a method to register application services with transient lifetimes, 
    /// allowing for the creation of new instances of these services each time they are requested.
    /// </summary>
    public static class TransientDIConfig
    {
        /// <summary>
        /// Registers application services with transient lifetimes in the provided IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <returns>The IServiceCollection with the added services.</returns>
        public static IServiceCollection AddApplicationServicesTransient(this IServiceCollection services)
        {
            services.AddTransient<ISessionDao, SessionDao>();
            services.AddTransient<ISessionService, SessionService>();
            services.AddTransient<IMeetingService, MeetingService>();
            services.AddTransient<IMeetingDao, MeetingDao>();
            services.AddTransient<IUnityOfWork, UnityOfWork>();
            services.AddTransient<ISessionClient, SessionClient>();
            services.AddTransient<IF1ApiClient, F1ApiClient>();
            services.AddTransient<IDriverDao, DriverDao>();
            services.AddTransient<IMeetingClient, MeetingClient>();
            services.AddTransient<IDriverService, DriverService>();
            services.AddTransient<IDriverClient, DriverClient>();
            services.AddTransient<ICarDataClient, CarDataClient>();
            services.AddTransient<ICarDataDao, CarDataDao>();
            services.AddTransient<ICarDataService, CarDataService>();
            services.AddTransient<ISessionResultDao, SessionResultDao>();
            services.AddTransient<ISessionResultService, SessionResultService>();
            services.AddTransient<ISessionResultClient, SessionResultClient>();
            services.AddTransient<IOvertakeDao, OvertakeDao>();
            services.AddTransient<IPitDao, PitDao>();
            services.AddTransient<IPitService, PitService>();
            services.AddTransient<IOvertakeService, OvertakeService>();
            services.AddTransient<IPitClient, PitClient>();
            services.AddTransient<IOvertakeClient, OvertakeClient>();
            services.AddTransient<IRaceControlDao, RaceControlDao>();
            services.AddTransient<IRaceControlClient, RaceControlClient>();
            services.AddTransient<IRaceControlService, RaceControlService>();
            services.AddTransient<IStintClient, StintClient>();
            services.AddTransient<IStintDao, StintDao>();
            services.AddTransient<IStintService, StintService>();
            services.AddTransient<ISessionResultQualifyDao, SessionResultQualifyDao>();
            services.AddTransient<ISessionResultQualifyingsService, SessionResultQualifyingsService>();
            services.AddTransient<ILapDao, LapDao>();
            services.AddTransient<ILapSegmentDao, LapSegmentDao>();
            services.AddTransient<ILapService, LapService>();
            services.AddTransient<ILapSegmentService, LapSegmentService>(); 

            return services;
        }
    }
}
