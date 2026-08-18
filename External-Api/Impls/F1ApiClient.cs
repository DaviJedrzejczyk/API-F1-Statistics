using ExternalApi.Constants;
using ExternalApi.Interfaces;
using Shared.Responses;

namespace ExternalApi.Impls
{
    public class F1ApiClient : IF1ApiClient
    {
        private readonly HttpClient _httpClient;
        public F1ApiClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri(F1ApiURL.URL_API_F1);
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
