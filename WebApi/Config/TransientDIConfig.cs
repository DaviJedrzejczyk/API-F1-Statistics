using Dao.Impl;
using ExternalApi.Impls;
using Services.Impl;
using Shared.Common.Atrributes;
using System.Reflection;

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
        /// Registers application services with transient lifetimes in the dependency injection container.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <returns>The IServiceCollection with the added services.</returns>
        public static IServiceCollection AddApplicationServicesTransient(this IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssemblyDependencies(Assembly.GetExecutingAssembly())
                .AddClasses(classes => classes.Where(type => 
                                                     (type.Module.Name.Equals("DAO.DLL", StringComparison.CurrentCultureIgnoreCase) || 
                                                      type.Module.Name.Equals("SERVICES.DLL", StringComparison.CurrentCultureIgnoreCase) ||
                                                      type.Module.Name.Equals("EXTERNALAPI.DLL", StringComparison.CurrentCultureIgnoreCase)) &&
                                                     type.IsDefined(typeof(IncludeDependencyInjectionAttribute), false)))
                .AsMatchingInterface()
                .WithTransientLifetime());

            return services;
        }
    }
}
