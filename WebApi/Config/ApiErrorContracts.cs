using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Middleware;
using WebApi.ViewModels;

namespace WebApi.Config
{
    /// <summary>
    /// Registers the shared ErrorViewModel contract for automatic validation and API route errors.
    /// </summary>
    public static class ApiErrorContracts
    {
        /// <summary>
        /// Replaces the default ProblemDetails body of automatic 400 responses (binding, missing body, invalid JSON).
        /// </summary>
        /// <param name="services">Service collection to configure.</param>
        public static IServiceCollection AddApiErrorContracts(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = _ => new BadRequestObjectResult(
                    new ErrorViewModel(StatusCodes.Status400BadRequest, ApiErrorMessages.InvalidRequest));
            });

            return services;
        }

        /// <summary>
        /// Adds the middleware that normalizes error responses under /api.
        /// </summary>
        /// <param name="app">Application builder to configure.</param>
        public static IApplicationBuilder UseApiErrorContracts(this IApplicationBuilder app) =>
            app.UseMiddleware<ApiErrorResponseMiddleware>();
    }
}
