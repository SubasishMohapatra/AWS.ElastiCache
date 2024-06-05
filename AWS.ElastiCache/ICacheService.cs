using Huron.AWS.Common.Core;

namespace Caching.AWS.ElastiCache
{
    public interface ICacheService
    {
        Task<CachingResponse<string?>> GetAsync(string key);
        Task<CachingResponse<bool>> SetAsync(string key, string value);
        Task<CachingResponse<bool>> DeleteAsync(string key);

        Task<CachingResponse<string?>> GetHashAsync(string hashKey, string field);
        Task<CachingResponse<string>> GetHashAllAsync(string hashKey);

        Task<CachingResponse<bool>> SetHashAsync(string hashKey, string field, string value);
        Task<CachingResponse<bool>> DeleteHashAsync(string hashKey, string field);
        // Add other cache operations as needed
    }

}