using Newtonsoft.Json;
using System.Net;

namespace ElastiCache.Client.Caching
{
    public interface ICachingService
    {
        Task<CachingResponse<T>> GetAsync<T>(string key);
        Task<CachingResponse<bool>> SetAsync<T>(string key, T value);
        Task<CachingResponse<bool>> DeleteAsync(string key);

        Task<CachingResponse<T>> HashGetAsync<T>(string cacheHashKey, string cacheHashField);
        Task<CachingResponse<Dictionary<string, string>>> HashGetAllAsync(string cacheHashKey);
        Task<CachingResponse<bool>> HashSetAsync<T>(string cacheHashKey, string cacheHashField, T cacheValue);
        Task<CachingResponse<bool>> HashDeleteAsync(string cacheHashKey, string cacheHashField);
    }
}