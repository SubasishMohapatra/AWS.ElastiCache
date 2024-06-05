using Caching.Api.Model;
using Caching.AWS.ElastiCache;
using Caching.Api;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using Caching.AWS.ElastiCache;

namespace Caching.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CacheController : ControllerBase
    {
        private readonly ICacheService cacheService;

        public CacheController(ICacheService cacheService)
        {
            this.cacheService = cacheService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(string cacheKey)
        {
            var response = await cacheService.GetAsync(cacheKey);
            return response.ToActionResult();
        }

        [HttpPost]
        public async Task<IActionResult> SetAsync([FromBody] CacheSetRequestModel model)
        {
            var response = await cacheService.SetAsync(model.CacheKey, model.CacheValue); 
            return response.ToActionResult();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(string cacheKey)
        {
            var response = await cacheService.DeleteAsync(cacheKey);
            return response.ToActionResult();
        }

        [HttpGet]
        [Route("gethash")]
        public async Task<IActionResult> HashGetAsync([FromQuery] string cacheHashKey, [FromQuery] string cacheHashField)
        {
            string decodedHashKey = Uri.UnescapeDataString(cacheHashKey);
            string decodedFieldValue = Uri.UnescapeDataString(cacheHashField);
            var response = await cacheService.GetHashAsync(decodedHashKey, decodedFieldValue);
            return response.ToActionResult();
        }

        [HttpGet]
        [Route("gethashAll")]
        public async Task<IActionResult> HashGetAllAsync([FromQuery] string cacheHashKey)
        {
            string decodedHashKey = Uri.UnescapeDataString(cacheHashKey);
            var response = await cacheService.GetHashAllAsync(decodedHashKey);
            return response.ToActionResult();
        }

        [HttpPost]
        [Route("sethash")]
        public async Task<IActionResult> HashSetAsync([FromBody] CacheHashSetRequestModel model)
        {
            var response = await cacheService.SetHashAsync(model.CacheHashKey,model.CacheHashField, model.CacheValue);
            return response.ToActionResult();
        }

        [HttpDelete]
        [Route("deletehash")]
        public async Task<IActionResult> HashDeleteAsync([FromQuery] string cacheHashKey, [FromQuery] string cacheHashField)
        {
            string decodedHashKey = Uri.UnescapeDataString(cacheHashKey);
            string decodedFieldValue = Uri.UnescapeDataString(cacheHashField);
            var response = await cacheService.DeleteHashAsync(decodedHashKey, decodedFieldValue);
            return response.ToActionResult();
        }
    }
}
