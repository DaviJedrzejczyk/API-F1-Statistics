using Shared.Responses;

namespace ExternalApi.Interfaces
{
    public interface IF1ApiClient
    {
        Task<SingleResponse<String>> Get(string endPoint, string parametersUrl);
    }
}
