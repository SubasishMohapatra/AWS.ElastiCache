using Caching.Core;
using Huron.AWS.SecretsManager.Core;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using Serilog;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Caching.AWS.ElastiCache
{
    public static class JSonSerializerDeserializationExtension
    {
        public static string Serialize(this Dictionary<string, string> dictionary)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            foreach (var pair in dictionary)
            {
                sb.AppendFormat("\"{0}\":{1},", pair.Key, pair.Value);
            }
            if (dictionary.Count > 0)
            {
                sb.Length--; // Remove the last comma
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}
