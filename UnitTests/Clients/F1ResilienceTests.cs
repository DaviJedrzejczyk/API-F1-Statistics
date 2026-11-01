using System.Net;
using System.Net.Http.Headers;
using ExternalApi.Impls;

namespace UnitTests.Clients;

[TestFixture]
public class F1ResilienceTests
{
    private sealed class Transport : HttpMessageHandler
    {
        public int Calls;
        public Func<int, CancellationToken, Task<HttpResponseMessage>> Handler = null!;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => Handler(++Calls, token);
    }

    private static F1ApiClient Client(Transport transport, List<TimeSpan> waits) =>
        new(new HttpClient(transport) { BaseAddress = new Uri("https://fixtures.invalid/") }, TimeProvider.System,
            (delay, token) => { token.ThrowIfCancellationRequested(); waits.Add(delay); return Task.CompletedTask; },
            token => { token.ThrowIfCancellationRequested(); return Task.CompletedTask; });

    [Test]
    public async Task RateLimited_UsesRetryAfterAndRetries()
    {
        var transport = new Transport { Handler = (n, _) => Task.FromResult(n == 1
            ? new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Headers = { RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(7)) } }
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") }) };
        var waits = new List<TimeSpan>();
        var result = await Client(transport, waits).Get("laps", "");
        Assert.That(result.HasSuccess, Is.True);
        Assert.That(transport.Calls, Is.EqualTo(2));
        Assert.That(waits, Is.EqualTo(new[] { TimeSpan.FromSeconds(7) }));
    }

    [Test]
    public async Task TransientFailure_StopsAfterThreeAttempts()
    {
        var transport = new Transport { Handler = (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)) };
        var waits = new List<TimeSpan>();
        var result = await Client(transport, waits).Get("laps", "");
        Assert.That(result.HasSuccess, Is.False);
        Assert.That(transport.Calls, Is.EqualTo(3));
        Assert.That(waits, Is.EqualTo(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) }));
    }

    [Test]
    public void Cancellation_PropagatesWithoutRetry()
    {
        using var cancel = new CancellationTokenSource();
        var transport = new Transport { Handler = (_, _) => { cancel.Cancel(); throw new OperationCanceledException(cancel.Token); } };
        Assert.CatchAsync<OperationCanceledException>(async () => await Client(transport, new()).Get("laps", "", cancel.Token));
        Assert.That(transport.Calls, Is.EqualTo(1));
    }
}


