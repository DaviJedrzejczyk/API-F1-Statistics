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
        public F1ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<SingleResponse<string>> Get(string endPoint, string parametersUrl)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(endPoint + parametersUrl);

                if (!response.IsSuccessStatusCode)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<String>("Error to fetch data: " + response.ReasonPhrase);

                string content = await response.Content.ReadAsStringAsync();
                
                return ResponseFactory.CreateInstance().CreateSuccessSingleResponse(content);

            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<string>("Error to fetch data: " + ex.Message, ex);
            }
            
        }
    }
}
