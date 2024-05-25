using Polly.CircuitBreaker;
using Polly;

namespace OnlineBankingApplication.Services
{
    public class ResilientHttpClient
    {
        private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;

        public ResilientHttpClient()
        {
            _circuitBreakerPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));
        }

        public async Task<HttpResponseMessage> GetAsync(string uri)
        {
            using var httpClient = new HttpClient();
            return await _circuitBreakerPolicy.ExecuteAsync(() => httpClient.GetAsync(uri));
        }
    }
}
