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
        private static readonly OpenF1RateLimiter Limiter = new(() => DateTimeOffset.UtcNow, Task.Delay);
        private const int MaxAttempts = 3;
        private static readonly TimeSpan AttemptTimeout = TimeSpan.FromSeconds(30);

        public F1ApiClient(HttpClient httpClient) : this(httpClient, TimeProvider.System, Task.Delay, Limiter.WaitAsync) { }

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
                    TimeSpan wait;
                    try
                    {
                        using var deadline = new CancellationTokenSource(AttemptTimeout, _clock);
                        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, deadline.Token);
                        using var response = await _httpClient.GetAsync(endPoint + parametersUrl, timeout.Token);
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync(timeout.Token);
                            return ResponseFactory.CreateInstance().CreateSuccessSingleResponse(content);
                        }
                        if (attempt == MaxAttempts || !IsTransient(response.StatusCode))
                            return ResponseFactory.CreateInstance().CreateFailureSingleResponse<string>("Error to fetch data: " + response.ReasonPhrase);

                        wait = RetryDelay(response, attempt);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
                    catch (Exception ex) when ((ex is HttpRequestException || ex is OperationCanceledException) && attempt < MaxAttempts)
                    {
                        wait = Backoff(attempt);
                    }
                    await _delay(wait, cancellationToken);
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

    }
}
