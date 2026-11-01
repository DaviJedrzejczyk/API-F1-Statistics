using System.Net;
using ExternalApi.Interfaces;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace ExternalApi.Impls
{
    [IncludeDependencyInjection]
    public class F1ApiClient : IF1ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly Func<TimeSpan, CancellationToken, Task> _delay;
        private readonly Func<CancellationToken, Task> _rateLimit;
        private readonly TimeProvider _clock;
        private static readonly SemaphoreSlim Gate = new(1, 1);
        private static readonly Queue<DateTimeOffset> Requests = new();
        private const int MaxAttempts = 3;
        private static readonly TimeSpan AttemptTimeout = TimeSpan.FromSeconds(30);

        public F1ApiClient(HttpClient httpClient) : this(httpClient, TimeProvider.System, Task.Delay, WaitForRateLimit) { }

        // The clock and waiting functions also allow deterministic transport tests.
        public F1ApiClient(HttpClient httpClient, TimeProvider clock,
            Func<TimeSpan, CancellationToken, Task> delay, Func<CancellationToken, Task> rateLimit)
        {
            _httpClient = httpClient;
            _clock = clock;
            _delay = delay;
            _rateLimit = rateLimit;
        }

        public async Task<SingleResponse<string>> Get(string endPoint, string parametersUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                for (int attempt = 1; ; attempt++)
                {
                    await _rateLimit(cancellationToken);
                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeout.CancelAfter(AttemptTimeout);
                    try
                    {
                        using var response = await _httpClient.GetAsync(endPoint + parametersUrl, timeout.Token);
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync(timeout.Token);
                            return ResponseFactory.CreateInstance().CreateSuccessSingleResponse(content);
                        }
                        if (attempt == MaxAttempts || !IsTransient(response.StatusCode))
                            return ResponseFactory.CreateInstance().CreateFailureSingleResponse<string>("Error to fetch data: " + response.ReasonPhrase);

                        var wait = RetryDelay(response, attempt);
                        await _delay(wait, cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
                    catch (Exception ex) when ((ex is HttpRequestException || ex is OperationCanceledException) && attempt < MaxAttempts)
                    {
                        await _delay(Backoff(attempt), cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<string>("Error to fetch data: " + ex.Message, ex);
            }
        }

        private static bool IsTransient(HttpStatusCode status) => status == HttpStatusCode.TooManyRequests || status == HttpStatusCode.RequestTimeout || (int)status >= 500 && (int)status <= 599;
        private static TimeSpan Backoff(int attempt) => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
        private TimeSpan RetryDelay(HttpResponseMessage response, int attempt)
        {
            var retry = response.Headers.RetryAfter;
            var requested = retry?.Delta ?? (retry?.Date - _clock.GetUtcNow());
            return requested is { } value && value > Backoff(attempt) ? value : Backoff(attempt);
        }

        // Sliding windows enforce both published free-tier limits, shared by clients in this process.
        private static async Task WaitForRateLimit(CancellationToken cancellationToken)
        {
            await Gate.WaitAsync(cancellationToken);
            try
            {
                while (true)
                {
                    var now = DateTimeOffset.UtcNow;
                    while (Requests.Count > 0 && now - Requests.Peek() >= TimeSpan.FromMinutes(1)) Requests.Dequeue();
                    var recent = Requests.Count(t => now - t < TimeSpan.FromSeconds(1));
                    var minuteWait = Requests.Count >= 30 ? Requests.Peek().AddMinutes(1) - now : TimeSpan.Zero;
                    var secondWait = recent >= 3 ? Requests.Where(t => now - t < TimeSpan.FromSeconds(1)).First().AddSeconds(1) - now : TimeSpan.Zero;
                    var wait = minuteWait > secondWait ? minuteWait : secondWait;
                    if (wait <= TimeSpan.Zero) { Requests.Enqueue(now); return; }
                    await Task.Delay(wait, cancellationToken);
                }
            }
            finally { Gate.Release(); }
        }
    }
}
