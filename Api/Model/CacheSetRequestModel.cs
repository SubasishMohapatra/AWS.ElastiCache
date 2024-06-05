namespace Caching.Api.Model
{
    public class CacheSetRequestModel
    {
        public CacheSetRequestModel(string cacheKey, string cacheValue) { this.CacheKey = cacheKey; this.CacheValue = cacheValue; }

        public string CacheKey { get; set; }
        public string CacheValue { get; set; }
    }
}