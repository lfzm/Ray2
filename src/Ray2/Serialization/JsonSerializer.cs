using Newtonsoft.Json;
using System;

namespace Ray2.Serialization
{
    public class JsonSerializer : IStringSerializer
    {
        public T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default(T);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public object Deserialize(Type type, string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;
            return JsonConvert.DeserializeObject(json, type);
        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
