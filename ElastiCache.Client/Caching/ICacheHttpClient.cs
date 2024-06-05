namespace ElastiCache.Client.Caching
{
    public interface ICacheHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string cacheKey);
        Task<HttpResponseMessage> SetAsync(string cacheKey, string cacheValue);
        Task<HttpResponseMessage> DeleteAsync(string cacheKey);
        Task<HttpResponseMessage> HashGetAsync(string cacheHashKey, string cacheHashField);
        Task<HttpResponseMessage> HashGetAllAsync(string cacheHashKey);
        Task<HttpResponseMessage> HashSetAsync(string cacheHashKey, string cacheHashField, string cacheValue);
        Task<HttpResponseMessage> HashDeleteAsync(string cacheHashKey, string cacheHashField);
        Task<HttpResponseMessage> HealthCheckAsync();
    }
}