namespace ExternalApi.Impls;

// One limiter is shared by all OpenF1 clients in this process.
public sealed class OpenF1RateLimiter
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly Queue<DateTimeOffset> requests = new();
    private readonly Func<DateTimeOffset> now;
    private readonly Func<TimeSpan, CancellationToken, Task> delay;

    public OpenF1RateLimiter(Func<DateTimeOffset> now, Func<TimeSpan, CancellationToken, Task> delay)
    {
        this.now = now;
        this.delay = delay;
    }

    public async Task WaitAsync(CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            while (true)
            {
                var current = now();
                while (requests.Count > 0 && current - requests.Peek() >= TimeSpan.FromMinutes(1)) requests.Dequeue();
                var recent = requests.Where(t => current - t < TimeSpan.FromSeconds(1)).ToArray();
                var minuteWait = requests.Count >= 30 ? requests.Peek().AddMinutes(1) - current : TimeSpan.Zero;
                var secondWait = recent.Length >= 3 ? recent[0].AddSeconds(1) - current : TimeSpan.Zero;
                var wait = minuteWait > secondWait ? minuteWait : secondWait;
                if (wait <= TimeSpan.Zero) { requests.Enqueue(current); return; }
                await delay(wait, cancellationToken);
            }
        }
        finally { gate.Release(); }
    }
}
