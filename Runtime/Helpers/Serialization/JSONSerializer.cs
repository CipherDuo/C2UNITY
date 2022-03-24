using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace CipherDuo.Utility.Serialization
{
    public static class JSONSerializer
    {
        public static string SerializeJson<T>(T packet)
        {
            return JsonConvert.SerializeObject(packet);
        }

        public static T DeserializeJson<T>(Stream serPacket)
        {
            var serializer = new JsonSerializer();

            using (var sr = new StreamReader(serPacket))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }
    }
}
