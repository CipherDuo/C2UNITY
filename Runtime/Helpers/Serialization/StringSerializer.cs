using System.Text;
using UnityEngine;

namespace CipherDuo.Utility.Serialization
{
    public static class StringSerializer
    {
        public static StringPacket.StringFile Serialize(string msg)
        {
            var serString = new StringPacket.StringFile();

            serString.content = Encoding.UTF7.GetBytes(msg);

            return serString;
        }

        public static string Deserialize(byte[] data)
        {
            string str = Encoding.UTF7.GetString(data);

            return str;
        }
    }
}