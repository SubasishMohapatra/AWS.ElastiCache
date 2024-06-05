using Huron.AWS.Common.Core;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Caching.Api
{
    public static class CachingResponseExtensions
    {
        public static IActionResult ToActionResult<T>(this CachingResponse<T> response)
        {
            return response.HttpStatusCode switch
            {
                HttpStatusCode.OK => new OkObjectResult(response),
                HttpStatusCode.BadRequest => new BadRequestObjectResult(response),
                HttpStatusCode.NotFound => new NotFoundObjectResult(response),
                HttpStatusCode.Forbidden => new ObjectResult(response) { StatusCode = (int)response.HttpStatusCode },
                HttpStatusCode.InternalServerError => new ObjectResult(response) { StatusCode = (int)response.HttpStatusCode },
                _ => new ObjectResult(response) { StatusCode = (int)response.HttpStatusCode }
            };
        }
    }
}
