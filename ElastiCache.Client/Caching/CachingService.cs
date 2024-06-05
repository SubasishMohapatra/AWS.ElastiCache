using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace ElastiCache.Client.Caching
{
    public class CachingService : ICachingService
    {
        private readonly ICacheHttpClient cacheHttpClient;
        private readonly ILogger<CachingService> logger;
        private readonly IConfiguration configuration;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public CachingService(ICacheHttpClient cacheHttpClient, ILogger<CachingService> logger, IConfiguration configuration)
        {
            this.cacheHttpClient = cacheHttpClient;
            this.logger = logger;
            this.configuration = configuration;
            if (HealthCheckAsync().GetAwaiter().GetResult() == false)
                cancellationTokenSource.Cancel();
        }

        public async Task<CachingResponse<T>> GetAsync<T>(string cacheKey)
        {
            try
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    return CacheOperationCancelledResponse<T>();
                }
                var response = await this.cacheHttpClient.GetAsync(cacheKey);
                var content = await response.Content.ReadAsStringAsync();

                // Deserialize the outer CachingResponse<string> object
                var jsonData = JsonConvert.DeserializeObject<CachingResponse<string>>(content);

                T result = default!;
                if (jsonData!.HttpStatusCode == HttpStatusCode.OK)
                {
                    if (typeof(T) == typeof(string) || typeof(T).IsPrimitive)
                    {
                        result = (T)Convert.ChangeType(jsonData.Result, typeof(T));
                    }
                    else
                    {
                        // Deserialize the nested JSON string into the desired type T (Employee)
                        result = JsonConvert.DeserializeObject<T>(jsonData.Result)!;
                    }
                }
                // Create a new CachingResponse<Employee> object
                return new CachingResponse<T>
                {
                    Result = result,
                    HttpStatusCode = jsonData.HttpStatusCode,
                    Message = jsonData.Message
                };
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error calling Caching API GetAsync() for key - {cacheKey}\n{ex.Message}";
                logger.LogError(ex, errorMessage);

                return new CachingResponse<T>
                {
                    Result = default!,
                    HttpStatusCode = HttpStatusCode.BadRequest,
                    Message = errorMessage
                };
            }
        }

        public async Task<CachingResponse<bool>> SetAsync<T>(string cacheKey, T cacheValue)
        {
            try
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    return CacheOperationCancelledResponse<bool>();
                }
                var jsonValue = JsonConvert.SerializeObject(cacheValue);
                var response = await cacheHttpClient.SetAsync(cacheKey, jsonValue);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<CachingResponse<bool>>(content);
                return result!;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error calling Caching API SetAsync() for key - {cacheKey}\n{ex.Message}";
                logger.LogError(ex, errorMessage);
                return new CachingResponse<bool>()
                {
                    Message = errorMessage,
                    Result = false,
                    HttpStatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
        }

        public async Task<CachingResponse<bool>> DeleteAsync(string cacheKey)
        {
            try
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    return CacheOperationCancelledResponse<bool>();
                }
                var response = await cacheHttpClient.DeleteAsync(cacheKey);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CachingResponse<bool>>(content)!;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error calling Caching API DeleteAsync() for key - {cacheKey}\n{ex.Message}";
                logger.LogError(ex, errorMessage);
                return new CachingResponse<bool>()
                {
                    Message = errorMessage,
                    Result = false,
                    HttpStatusCode = HttpStatusCode.BadRequest
                };
            }
        }

        private async Task<bool> HealthCheckAsync()
        {
            try
            {                
                if (bool.TryParse(this.configuration.GetSection("CacheHttpSettings:EnableCache").Value, out bool isCacheEnabled) && isCacheEnabled==false)
                {
                    return isCacheEnabled;
                }
                var response = await cacheHttpClient.HealthCheckAsync();
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error calling Caching API HealthCheckAsync()";
                logger.LogError(ex, errorMessage);
                return false;
            }
        }

        private static CachingResponse<T> CacheOperationCancelledResponse<T>()
        {
            return new CachingResponse<T>
            {
                Result = default(T)!,
                HttpStatusCode = HttpStatusCode.BadRequest,
                Message = "Operation cancelled"
            };
        }

        public async Task<CachingResponse<T>> HashGetAsync<T>(string cacheHashKey, string cacheHashField)
        {
            try
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    return CacheOperationCancelledResponse<T>();
                }
                var response = await this.cacheHttpClient.HashGetAsync(cacheHashKey, cacheHashField);
                var content = await response.Content.ReadAsStringAsync();

                // Deserialize the outer CachingResponse<string> object
                var jsonData = JsonConvert.DeserializeObject<CachingResponse<string>>(content);

                T result = default!;
                if (jsonData!.HttpStatusCode == HttpStatusCode.OK)
                {
                    if (typeof(T) == typeof(string) || typeof(T).IsPrimitive)
                    {
                        result = (T)Convert.ChangeType(jsonData.Result, typeof(T));
                    }
                    else
                    {
                        // Deserialize the nested JSON string into the desired type T (Employee)
                        result = JsonConvert.DeserializeObject<T>(jsonData.Result)!;
                    }
                }
                // Create a new CachingResponse<Employee> object
                return new CachingResponse<T>
                {
                    Result = result,
                    HttpStatusCode = jsonData.HttpStatusCode,
                    Message = jsonData.Message
                };
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error calling Caching API HashGetAsync() for hashKey - {cacheHashKey} & cacheHashField - {cacheHashField}\n{ex.Message}";
                logger.LogError(ex, errorMessage);
                return new CachingResponse<T>
                {
                    Result = default!,
                    HttpStatusCode = HttpStatusCode.BadRequest,
                    Message = errorMessage
                };
            }
        }

        public async Task<CachingResponse<Dictionary<string, string>>> HashGetAllAsync(string cacheHashKey)
        {
            try
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    return CacheOperationCancelledResponse<Dictionary<string, string>>();
                }
                var response = await this.cacheHttpClient.HashGetAllAsync(cacheHashKey);
                var content = await response.Content.ReadAsStringAsync();

                // Deserialize the outer CachingResponse<string> object
                var jsonData = JsonConvert.DeserializeObject<CachingResponse<string>>(content);

                var result = new Dictionary<string, string>();
                if (jsonData!.HttpStatusCode == HttpStatusCode.OK)
                {
                    result = jsonData.Result.DeserializeJsonData();
                }
                // Create a new CachingResponse<Employee> object
                return new CachingResponse<Dictionary<string, string>>
                {
                    Result = result,
                    HttpStatusCode = jsonData.HttpStatusCode,
                    Message = jsonData.Message
                };
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error calling Caching API HashGetAllAsync() for hashKey - {cacheHashKey}\n{ex.Message}";
                logger.LogError(ex, errorMessage);
                return new CachingResponse<Dictionary<string, string>>
                {
                    Result = default!,
                    HttpStatusCode = HttpStatusCode.BadRequest,
                    Message = errorMessage
                };
            }
        }


        public async Task<CachingResponse<bool>> HashSetAsync<T>(string cacheHashKey, string cacheHashField, T cacheValue)
        {
            try
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    return CacheOperationCancelledResponse<bool>();
                }
                // Create the request model
                var jsonValue = JsonConvert.SerializeObject(cacheValue);
                var response = await cacheHttpClient.HashSetAsync(cacheHashKey, cacheHashField, jsonValue);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<CachingResponse<bool>>(content);
                return result!;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error calling Caching API HashSetAsync() for hashKey - {cacheHashKey} & cacheHashField - {cacheHashField}\n{ex.Message}";
                logger.LogError(ex, errorMessage);
                return new CachingResponse<bool>()
                {
                    Message = errorMessage,
                    Result = false,
                    HttpStatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
        }

        public async Task<CachingResponse<bool>> HashDeleteAsync(string cacheHashKey, string cacheHashField)
        {
            try
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    return CacheOperationCancelledResponse<bool>();
                }
                var response = await cacheHttpClient.HashDeleteAsync(cacheHashKey, cacheHashField);
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CachingResponse<bool>>(content)!;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error calling Caching API DeleteAsync() for hashKey - {cacheHashKey} & cacheHashField - {cacheHashField}\n{ex.Message}";
                logger.LogError(ex, errorMessage);
                return new CachingResponse<bool>()
                {
                    Message = errorMessage,
                    Result = false,
                    HttpStatusCode = HttpStatusCode.BadRequest
                };
            }
        }
                
    }
}
