using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ElastiCache.Client.Caching
{
    public static class JSonSerializerDeserializationExtension
    {
        public static Dictionary<string, string> DeserializeJsonData(this string jsonData)
        {
            var deserializedData = new Dictionary<string, string>();

            // Deserialize the JSON string
            dynamic jsonObject = JsonConvert.DeserializeObject(jsonData);

            // Iterate over each property of the JSON object
            foreach (var property in jsonObject.Properties())
            {
                // Add the property name and its value (converted to string) to the dictionary
                deserializedData[property.Name] = (property.Value == null || property.Value.Type == JTokenType.Null) ? null : property.Value.ToString();
            }

            return deserializedData;
        }
    }
}
