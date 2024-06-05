namespace Caching.AWS.ElastiCache
{
    using global::Caching.AWS.ElastiCache;
    using Google.Protobuf.WellKnownTypes;
    using Huron.AWS.Common.Core;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using StackExchange.Redis;
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Text.Json;

    public class ElastiCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer? connectionMultiplexer;
        private readonly ILogger<ElastiCacheService> logger;

        public ElastiCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger<ElastiCacheService> logger)
        {
            this.connectionMultiplexer = connectionMultiplexer;
            if (this.connectionMultiplexer is null)
            {
                // Handle connection failure here (e.g., log, throw, etc.)
                logger.LogError("Redis connection is not available.");
            }

            this.logger = logger;
        }

        private IDatabase? GetDatabase()
        {
            return connectionMultiplexer?.GetDatabase();
        }

        private CachingResponse<T> CreateConnectionErrorResponse<T>()
        {
            return new CachingResponse<T>
            {
                Message = "ElastiCache connection not available",
                Result = default!,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }


        public async Task<CachingResponse<bool>> SetAsync(string key, string value)
        {
            try
            {
                var db = GetDatabase();

                if (db == null)
                {
                    return CreateConnectionErrorResponse<bool>();
                }
                var result = await db.StringSetAsync(key, value);
                return new CachingResponse<bool>
                {
                    Result = result,
                    HttpStatusCode = result ? HttpStatusCode.OK : HttpStatusCode.Forbidden,
                    Message = result ? "Key successfully set in ElastiCache" : "Key not set in ElastiCache"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error setting value in ElastiCache for key - {key}");
                return new CachingResponse<bool>()
                {
                    Message = $"Error setting value in ElastiCache for key - {key}\n{ex.Message}",
                    Result = false,
                    HttpStatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
        }

        public async Task<CachingResponse<string?>> GetAsync(string key)
        {
            try
            {
                var db = GetDatabase();

                if (db == null)
                {
                    return CreateConnectionErrorResponse<string?>();
                }

                var keyExists = await db.KeyExistsAsync(key);
                var result = keyExists ? await db.StringGetAsync(key) : (RedisValue?)null;

                return new CachingResponse<string?>
                {
                    Result = result?.ToString(),
                    HttpStatusCode = keyExists ? HttpStatusCode.OK : HttpStatusCode.NotFound,
                    Message = keyExists ? "Key found in ElastiCache" : "Key not found in ElastiCache"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error getting value from ElastiCache for key - {key}");

                return new CachingResponse<string?>
                {
                    Message = $"Error getting value from ElastiCache for key - {key}\n{ex.Message}",
                    Result = string.Empty,
                    HttpStatusCode = HttpStatusCode.BadRequest
                };
            }
        }

        //Delete operation
        public async Task<CachingResponse<bool>> DeleteAsync(string key)
        {
            try
            {
                var db = GetDatabase();

                if (db == null)
                {
                    return CreateConnectionErrorResponse<bool>();
                }

                var keyExists = await db.KeyExistsAsync(key);

                var result = keyExists ? await db.KeyDeleteAsync(key) : false;

                return new CachingResponse<bool>
                {
                    Result = result,
                    HttpStatusCode = keyExists ? HttpStatusCode.OK : HttpStatusCode.NotFound,
                    Message = keyExists ? "Key successfully deleted from ElastiCache" : "Key not found in ElastiCache"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting value from ElastiCache for key - {key}");
                return new CachingResponse<bool>()
                {
                    Message = $"Error deleting value from ElastiCache for key - {key}\n{ex.Message}",
                    Result = false,
                    HttpStatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
        }

        public async Task<CachingResponse<bool>> SetHashAsync(string hashKey, string field, string value)
        {
            try
            {
                var db = GetDatabase();

                if (db == null)
                {
                    return CreateConnectionErrorResponse<bool>();
                }
                var result = await db.HashSetAsync(hashKey, field, value);
                return new CachingResponse<bool>
                {
                    Result = result,
                    HttpStatusCode = result ? HttpStatusCode.OK : HttpStatusCode.Forbidden,
                    Message = result ? "Key successfully set in ElastiCache" : "Key not set in ElastiCache"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error setting value in ElastiCache for key - {hashKey}");
                return new CachingResponse<bool>()
                {
                    Message = $"Error setting value in ElastiCache for key - {hashKey}\n{ex.Message}",
                    Result = false,
                    HttpStatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
        }

        public async Task<CachingResponse<string?>> GetHashAsync(string hashKey, string field)
        {
            try
            {
                var db = GetDatabase();

                if (db == null)
                {
                    return CreateConnectionErrorResponse<string?>();
                }

                var keyExists = await db.HashExistsAsync(hashKey, field);
                var result = keyExists ? await db.HashGetAsync(hashKey, field) : (RedisValue?)null;

                return new CachingResponse<string?>
                {
                    Result = result?.ToString(),
                    HttpStatusCode = keyExists ? HttpStatusCode.OK : HttpStatusCode.NotFound,
                    Message = keyExists ? $"Hashkey {hashKey} with field{field} found in ElastiCache" : $"Hashkey {hashKey} field {field} not found in ElastiCache"
                };
            }
            catch (Exception ex)
            {

                logger.LogError(ex, $"Error getting value from ElastiCache for hashkey - {hashKey} and field - {field}");

                return new CachingResponse<string?>
                {
                    Message = $"Error getting value from ElastiCache for hashkey - {hashKey} and field - {field} \n {ex.Message}",
                    Result = string.Empty,
                    HttpStatusCode = HttpStatusCode.BadRequest
                };
            }
        }

        public async Task<CachingResponse<string>> GetHashAllAsync(string hashKey)
        {
            try
            {
                var db = GetDatabase();

                if (db == null)
                {
                    return CreateConnectionErrorResponse<string>();
                }

                var keyExists = await db.KeyExistsAsync(hashKey);
                var result = string.Empty;
                if (keyExists)
                {
                    var hashEntries = await db.HashGetAllAsync(hashKey);
                    var keyValuePairs = hashEntries.ToDictionary(
    entry => entry.Name.ToString(), // Convert RedisValue key to string
    entry => entry.Value.ToString() // Convert RedisValue value to string
);                    result = keyValuePairs.Serialize();
                }
                return new CachingResponse<string>
                {
                    Result = result,
                    HttpStatusCode = keyExists ? HttpStatusCode.OK : HttpStatusCode.NotFound,
                    Message = keyExists ? $"Hashkey {hashKey} found in ElastiCache" : $"Hashkey {hashKey} not found in ElastiCache"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error getting value from ElastiCache for hashkey - {hashKey}");
                return new CachingResponse<string>
                {
                    Message = $"Error getting value from ElastiCache for hashkey - {hashKey}\n {ex.Message}",
                    Result = string.Empty,
                    HttpStatusCode = HttpStatusCode.BadRequest
                };
            }
        }

        //Delete operation
        public async Task<CachingResponse<bool>> DeleteHashAsync(string hashKey, string field)
        {
            try
            {
                var db = GetDatabase();

                if (db == null)
                {
                    return CreateConnectionErrorResponse<bool>();
                }

                var keyExists = await db.HashExistsAsync(hashKey, field);

                var result = keyExists ? await db.HashDeleteAsync(hashKey, field) : false;

                return new CachingResponse<bool>
                {
                    Result = result,
                    HttpStatusCode = keyExists ? HttpStatusCode.OK : HttpStatusCode.NotFound,
                    Message = keyExists ? $"Hashkey {hashKey} with field {field} successfully deleted from ElastiCache" : $"Hashkey {hashKey} field {field} not found in ElastiCache"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error getting value from ElastiCache for hashkey - {hashKey} and field - {field}");
                return new CachingResponse<bool>()
                {
                    Message = $"Error getting value from ElastiCache for hashkey - {hashKey} and field - {field} \n {ex.Message}",
                    Result = false,
                    HttpStatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
        }

        // ... other cache operations

    }
}