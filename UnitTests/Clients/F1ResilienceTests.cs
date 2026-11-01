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
    private sealed class FixedClock : TimeProvider
    {
        public DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }

    [Test]
    public async Task RetryAfterDate_UsesServerDelay()
    {
        var clock = new FixedClock();
        var transport = new Transport { Handler = (n, _) => Task.FromResult(n == 1
            ? new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Headers = { RetryAfter = new RetryConditionHeaderValue(clock.Now.AddSeconds(9)) } }
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") }) };
        var waits = new List<TimeSpan>();
        var client = new F1ApiClient(new HttpClient(transport) { BaseAddress = new Uri("https://fixtures.invalid/") }, clock,
            (d, _) => { waits.Add(d); return Task.CompletedTask; }, _ => Task.CompletedTask);
        Assert.That((await client.Get("laps", "")).HasSuccess, Is.True);
        Assert.That(waits, Is.EqualTo(new[] { TimeSpan.FromSeconds(9) }));
    }

    [Test]
    public async Task Limiter_EnforcesBothSlidingWindows()
    {
        var clock = new FixedClock();
        var waits = new List<TimeSpan>();
        var limiter = new OpenF1RateLimiter(() => clock.Now, (d, _) => { waits.Add(d); clock.Now += d; return Task.CompletedTask; });
        for (var i = 0; i < 31; i++) await limiter.WaitAsync(default);
        Assert.That(waits, Has.Count.EqualTo(10));
        Assert.That(waits.Take(9), Is.All.EqualTo(TimeSpan.FromSeconds(1)));
        Assert.That(waits.Last(), Is.EqualTo(TimeSpan.FromSeconds(51)));
        Assert.That(clock.Now, Is.EqualTo(new DateTimeOffset(2026, 1, 1, 0, 1, 0, TimeSpan.Zero)));
    }
    private sealed class ManualTimeoutClock : TimeProvider
    {
        private TimerCallback? callback;
        private object? state;
        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            this.callback = callback;
            this.state = state;
            return new ManualTimer();
        }
        public void Expire() => callback!(state);
        private sealed class ManualTimer : ITimer
        {
            public bool Change(TimeSpan dueTime, TimeSpan period) => true;
            public void Dispose() { }
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }

    [Test]
    public async Task AttemptTimeout_RetriesAndStopsAtLimit()
    {
        var clock = new ManualTimeoutClock();
        var transport = new Transport { Handler = (_, token) => { clock.Expire(); return Task.FromCanceled<HttpResponseMessage>(token); } };
        var waits = new List<TimeSpan>();
        var client = new F1ApiClient(new HttpClient(transport) { BaseAddress = new Uri("https://fixtures.invalid/") }, clock,
            (d, _) => { waits.Add(d); return Task.CompletedTask; }, _ => Task.CompletedTask);
        var result = await client.Get("laps", "");
        Assert.That(result.HasSuccess, Is.False);
        Assert.That(transport.Calls, Is.EqualTo(3));
        Assert.That(waits, Is.EqualTo(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) }));
    }
}
