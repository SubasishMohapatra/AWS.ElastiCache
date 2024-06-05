using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace ElastiCache.Client.Caching
{
    public class CacheHttpClient : ICacheHttpClient
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<CacheHttpClient> logger;

        public CacheHttpClient(HttpClient httpClient, ILogger<CacheHttpClient> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            // Additional configuration or customization can be done here
        }

        public async Task<HttpResponseMessage> GetAsync(string cacheKey)
        {
            var requestUri = $"?cacheKey={Uri.EscapeDataString(cacheKey)}";
            return await this.httpClient.GetAsync(requestUri);
        }

        public async Task<HttpResponseMessage> SetAsync(string cacheKey, string cacheValue)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(new { cacheKey = cacheKey, cacheValue = cacheValue }), Encoding.UTF8, "application/json");
            return await this.httpClient.PostAsync(string.Empty, stringContent);
        }


        public async Task<HttpResponseMessage> DeleteAsync(string cacheKey)
        {           
            var requestUri = $"?cacheKey={Uri.EscapeDataString(cacheKey)}";
            return await this.httpClient.DeleteAsync(requestUri);
        }

        public async Task<HttpResponseMessage> HashGetAsync(string cacheHashKey, string cacheHashField)
        {
            string requestUri = $"{httpClient.BaseAddress}/getHash?cacheHashKey={Uri.EscapeDataString(cacheHashKey)}&cacheHashField={Uri.EscapeDataString(cacheHashField)}";
            return await this.httpClient.GetAsync(requestUri);
        }

        public async Task<HttpResponseMessage> HashGetAllAsync(string cacheHashKey)
        {
            string requestUri = $"{httpClient.BaseAddress}/gethashAll?cacheHashKey={Uri.EscapeDataString(cacheHashKey)}";
            return await this.httpClient.GetAsync(requestUri);
        }


        public async Task<HttpResponseMessage> HashSetAsync(string cacheHashKey, string cacheHashField, string cacheValue)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(new { cacheHashKey = cacheHashKey, cacheHashField = cacheHashField, cacheValue = cacheValue }), Encoding.UTF8, "application/json");
            string requestUri = $"{httpClient.BaseAddress}/setHash";
            return await this.httpClient.PostAsync(requestUri, stringContent);
        }


        public async Task<HttpResponseMessage> HashDeleteAsync(string cacheHashKey, string cacheHashField)
        {
            string requestUri = $"{httpClient.BaseAddress}/deletehash?cacheHashKey={Uri.EscapeDataString(cacheHashKey)}&cacheHashField={Uri.EscapeDataString(cacheHashField)}";
            return await this.httpClient.DeleteAsync(requestUri);
        }

        public async Task<HttpResponseMessage> HealthCheckAsync()
        {
            //var requestUri = $"http://localhost:3100/api/HealthCheck/Diagnose";
            string hostUri = this.httpClient.BaseAddress!.GetLeftPart(UriPartial.Authority);
            return await this.httpClient.GetAsync($"{hostUri}/api/HealthCheck/Diagnose");
        }
    }
}
