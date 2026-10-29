using Microsoft.Extensions.Http;

namespace IntegrationTests.Infrastructure;

public sealed class FixtureHttpFilter(FixtureHttpHandler handler) : IHttpMessageHandlerBuilderFilter
{
    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next) => builder =>
    {
        next(builder);
        builder.PrimaryHandler = handler;
    };
}
