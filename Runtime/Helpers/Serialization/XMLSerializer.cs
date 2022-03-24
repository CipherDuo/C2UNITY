using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace CipherDuo.Utility.Serialization
{
    public static class XMLSerializer
    {
        public static byte[] SerializeXML<T>(T packet)
        {
            using (MemoryStream memoryStream = new MemoryStream())
                try
                {
                    new XmlSerializer(typeof(T)).Serialize(memoryStream, packet);
                    return memoryStream.ToArray();
                }
                catch (Exception error)
                {
                    Debug.Log(error);
                    return null;
                }
        }

        public static T DeserializeXML<T>(byte[] serPacket)
        {
            using (MemoryStream stream = new MemoryStream(serPacket))
                try
                {
                    var packet = (T) new XmlSerializer(typeof(T)).Deserialize(stream);
                    return packet;
                }
                catch (Exception error)
                {
                    Debug.Log(error);
                    return default;
                }
        }
    }
}