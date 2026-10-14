using ExternalApi.Constants;
using ExternalApi.Interfaces;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace ExternalApi.Impls
{
    [IncludeDependencyInjection]
    public class F1ApiClient : IF1ApiClient
    {
        private readonly HttpClient _httpClient;

        private static int _requestCount = 0;
        private static readonly int _maxRequestsPerMinute = 30;

        private static DateTime _windowStart = DateTime.UtcNow;

        private static readonly SemaphoreSlim _rateLimitSemaphore = new(1, 1);

        public F1ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<SingleResponse<string>> Get(string endPoint, string parametersUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                await WaitForRateLimit(cancellationToken);

                HttpResponseMessage response = await _httpClient.GetAsync(endPoint + parametersUrl, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<string>("Error to fetch data: " + response.ReasonPhrase);
                }

                string content = await response.Content.ReadAsStringAsync(cancellationToken);

                return ResponseFactory.CreateInstance().CreateSuccessSingleResponse(content);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<string>("Error to fetch data: " + ex.Message, ex);
            }
        }

        private static async Task WaitForRateLimit(CancellationToken cancellationToken)
        {
            await _rateLimitSemaphore.WaitAsync(cancellationToken);

            try
            {
                TimeSpan elapsed = DateTime.UtcNow - _windowStart;

                if (elapsed >= TimeSpan.FromMinutes(1))
                {
                    _requestCount = 0;
                    _windowStart = DateTime.UtcNow;
                }

                if (_requestCount >= _maxRequestsPerMinute)
                {
                    TimeSpan waitTime =
                        TimeSpan.FromMinutes(1) - elapsed;

                    if (waitTime > TimeSpan.Zero)
                    {
                        await Task.Delay(waitTime, cancellationToken);
                    }

                    _requestCount = 0;
                    _windowStart = DateTime.UtcNow;
                }

                _requestCount++;
            }
            finally
            {
                _rateLimitSemaphore.Release();
            }
        }
    }
}