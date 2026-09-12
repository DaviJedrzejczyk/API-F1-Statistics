using Shared.Converters;

namespace WebApi.Config
{
    /// <summary>
    /// Provides extension methods for configuring JSON serialization options in an ASP.NET Core MVC application.
    /// </summary>
    public static class JsonSerealizerConfig 
    {
        /// <summary>
        /// Adds custom JSON converters to the MVC builder for handling double values and lists of double values, converting nulls to zero.
        /// </summary>
        /// <param name="builder">The MVC builder to which the JSON converters will be added.</param>
        /// <returns>The updated MVC builder.</returns>
        public static IMvcBuilder AddJsonConfiguration(this IMvcBuilder builder)
        {
            builder.AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.Converters.Add(
                    new ListDoubleNullToZeroConverter());

                opts.JsonSerializerOptions.Converters.Add(
                    new DoubleNullToZeroConverter());
            });

            return builder;
        }
    }
}
