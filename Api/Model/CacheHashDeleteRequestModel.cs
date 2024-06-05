namespace Huro.Caching.Api.Model
{
    public class CacheHashDeleteRequestModel
    {
        public CacheHashDeleteRequestModel(string cacheHashKey, string cacheHashField) { this.CacheHashKey = cacheHashKey; this.CacheHashField = cacheHashField;}

        public string CacheHashKey { get; set; }
        public string CacheHashField { get; set; }
    }
}