namespace Caching.Api.Model
{
    public class CacheHashSetRequestModel
    {
        public CacheHashSetRequestModel(string cacheHashKey, string cacheHashField, string cacheValue) { this.CacheHashKey = cacheHashKey; this.CacheHashField = cacheHashField; this.CacheValue = cacheValue; }

        public string CacheHashKey { get; set; }
        public string CacheHashField { get; set; }
        public string CacheValue { get; set; }
    }
}