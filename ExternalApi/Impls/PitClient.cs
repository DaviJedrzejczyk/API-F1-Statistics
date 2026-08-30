using ExternalApi.Interfaces;

namespace ExternalApi.Impls
{
    public class PitClient : IPitClient
    {
        private readonly IF1ApiClient _client;
        public PitClient(IF1ApiClient f1ApiClient)
        {
            _client = f1ApiClient;
        }

    }
}
