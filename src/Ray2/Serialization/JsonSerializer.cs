using Newtonsoft.Json;
using System;
using System.Text;

namespace Ray2.Serialization
{
    public class JsonSerializer : ISerializer
    {
        Encoding encoding = Encoding.UTF8;
        public T Deserialize<T>(byte[] bytes)
        {
            var json = encoding.GetString(bytes);
            if (string.IsNullOrEmpty(json))
                return default(T);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public object Deserialize(Type type, byte[] bytes)
        {
            var json = encoding.GetString(bytes);

            if (string.IsNullOrEmpty(json))
                return null;
            return JsonConvert.DeserializeObject(json, type);
        }
        public byte[] Serialize(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            return encoding.GetBytes(json);
          }

        public byte[] Serialize<T>(T obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            return encoding.GetBytes(json);
        }
    }
}
