using Polly.CircuitBreaker;
using Polly;

namespace OnlineBankingApplication.Services
{
    public class ResilientHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;

        public ResilientHttpClient()
        {
            _httpClient = new HttpClient();
            _circuitBreakerPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                                          .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));
        }

        public async Task<HttpResponseMessage> GetAsync(string uri)
        {
            return await _circuitBreakerPolicy.ExecuteAsync(() => _httpClient.GetAsync(uri));
        }
        public async Task<HttpResponseMessage> PostAsync(string uri, HttpContent content)
        {
            return await _circuitBreakerPolicy.ExecuteAsync(() => _httpClient.PostAsync(uri, content));
        }
    }
}
