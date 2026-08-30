using ExternalApi.Interfaces;

namespace ExternalApi.Impls
{
    public class OvertakeClient : IOvertakeClient
    {
        private readonly IF1ApiClient _client;
        public OvertakeClient(IF1ApiClient f1ApiClient)
        {
            _client = f1ApiClient;
        }

    }
}
