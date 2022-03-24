using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;

namespace CipherDuo.IPFS.Serialization
{
    public static class StringConvertion
    {
        private readonly static Dictionary<char, byte> hexmap = new Dictionary<char, byte>()
        {
        { 'a', 0xA },{ 'b', 0xB },{ 'c', 0xC },{ 'd', 0xD },
        { 'e', 0xE },{ 'f', 0xF },{ 'A', 0xA },{ 'B', 0xB },
        { 'C', 0xC },{ 'D', 0xD },{ 'E', 0xE },{ 'F', 0xF },
        { '0', 0x0 },{ '1', 0x1 },{ '2', 0x2 },{ '3', 0x3 },
        { '4', 0x4 },{ '5', 0x5 },{ '6', 0x6 },{ '7', 0x7 },
        { '8', 0x8 },{ '9', 0x9 }
        };
        
        public static string ByteArrayToString(this byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] ToBytes(this string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                throw new ArgumentException("Hex cannot be null/empty/whitespace");

            if (hex.Length % 2 != 0)
                throw new FormatException("Hex must have an even number of characters");

            bool startsWithHexStart = hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase);

            if (startsWithHexStart && hex.Length == 2)
                throw new ArgumentException("There are no characters in the hex string");


            int startIndex = startsWithHexStart ? 2 : 0;

            byte[] bytesArr = new byte[(hex.Length - startIndex) / 2];

            char left;
            char right;

            try
            {
                int x = 0;
                for (int i = startIndex; i < hex.Length; i += 2, x++)
                {
                    left = hex[i];
                    right = hex[i + 1];
                    bytesArr[x] = (byte)((hexmap[left] << 4) | hexmap[right]);
                }
                return bytesArr;
            }
            catch (KeyNotFoundException)
            {
                throw new FormatException("Hex string has non-hex character");
            }

        }

    }
    public class Conversion
    {

        private List<byte> byteStream = new List<byte>();
        private byte[] byteArray;
        private int index = 0;
        
        public static int[] byteArrayToIntArray(byte[] serialized)
        {
            int location = 0;
            //first we get the int array length that we serialized to the start of the byte array
            int length = BitConverter.ToInt32(serialized, location);
            location += 4;
 
            int[] intArray = new int[length];
            int index = 0;
            while (index < length)  //Now lets reconstruct the original integer array
            {
                intArray[index] = BitConverter.ToInt32(serialized, location);
                location += 4;
                index++;
            }
 
            return intArray;
        }
        
        public static byte[] IntArrayToByteArray(int[] intArray)
        {
            int totalBytes = (intArray.Length * 4) + 4;  //Integers are 4 bytes long, plus we're going to add another integer at the beginning with the total array length
            byte[] serialized = new byte[totalBytes];  //Byte array we are going to return
 
            List<byte[]> listOfBytes = new List<byte[]>();  //A temporary list of byte arrays of converted integers
            foreach (int i in intArray)
            {
                byte[] converted = BitConverter.GetBytes(i);  //convert an integer into a 4 length byte array
                listOfBytes.Add(converted);
            }
 
            //Now lets build the final byte array
            int location = 0;  //track our current location within the byte array we are going to return with this
 
            Array.Copy(BitConverter.GetBytes(intArray.Length), 0, serialized, location, 4);  //include the length of the integer array as a header in front of the actual data
            location += 4;
            foreach (byte[] ba in listOfBytes)  //now add the contents of the list to the byte array
            {
                Array.Copy(ba, 0, serialized, location, 4);
                location +=4;
            }
 
            return serialized;
        }
        /// <summary>  
        /// Returns the stream as a Byte Array  
        /// </summary>  
        public byte[] ByteArray
        {
            get
            {
                if (byteArray == null || byteStream.Count != byteArray.Length)
                    byteArray = byteStream.ToArray();

                return byteArray;
            }
        }


        /// <summary>  
        /// Initialiaze a stream from a byte array.  
        /// Used for deserilaizing a byte array  
        /// </summary>  
        /// <param name="ByteArray"></param>  
        public Conversion(byte[] ByteArray)
        {
            byteArray = ByteArray;
            byteStream = new List<byte>(ByteArray);
        }
        public Conversion()
        {
            byteArray = null;
            byteStream = new List<byte>();
        }



        // --- double ---  
        public void Serialize(double d)
        {
            byteStream.AddRange(BitConverter.GetBytes(d));

        }

        public double DeserializeDouble()
        {
            double d = BitConverter.ToDouble(ByteArray, index); index += 8;
            return d;
        }
        //  

        // --- bool ---  
        public void Serialize(bool b)
        {
            byteStream.AddRange(BitConverter.GetBytes(b));
        }

        public bool DeserializeBool()
        {
            bool b = BitConverter.ToBoolean(ByteArray, index); index += 1;
            return b;
        }
        //  

        // --- Vector2 ---  
        public void Serialize(Vector2 v)
        {
            byteStream.AddRange(GetBytes(v));
        }

        public Vector2 DeserializeVector2()
        {
            Vector2 vector2 = new Vector2();
            vector2.x = BitConverter.ToSingle(ByteArray, index); index += 4;
            vector2.y = BitConverter.ToSingle(ByteArray, index); index += 4;
            return vector2;
        }
        //  

        // --- Vector3 ---  
        public void Serialize(Vector3 v)
        {
            byteStream.AddRange(GetBytes(v));
        }

        public Vector3 DeserializeVector3()
        {
            Vector3 vector3 = new Vector3();
            vector3.x = BitConverter.ToSingle(ByteArray, index); index += 4;
            vector3.y = BitConverter.ToSingle(ByteArray, index); index += 4;
            vector3.z = BitConverter.ToSingle(ByteArray, index); index += 4;
            return vector3;
        }
        //  

        // --- Type ---  
        public void Serialize(System.Type t)
        {
            // serialize type as string  
            string typeStr = t.ToString();
            Serialize(typeStr);
        }

        public Type DeserializeType()
        {
            // type stored as string  
            string typeStr = DeserializeString();
            return Type.GetType(typeStr); ;
        }
        //  

        // --- String ---  
        public void Serialize(string s)
        {
            // add the length as a header  
            byteStream.AddRange(BitConverter.GetBytes(s.Length));
            foreach (char c in s)
                byteStream.Add((byte)c);
        }

        public string DeserializeString()
        {
            int length = BitConverter.ToInt32(ByteArray, index); index += 4;
            string s = "";
            for (int i = 0; i < length; i++)
            {
                s += (char)ByteArray[index];
                index++;
            }

            return s;
        }
        //  

        // --- byte[] ---  
        public void Serialize(byte[] b)
        {
            // add the length as a header  
            byteStream.AddRange(BitConverter.GetBytes(b.Length));
            byteStream.AddRange(b);
        }

        public byte[] DeserializeByteArray()
        {
            int length = BitConverter.ToInt32(ByteArray, index); index += 4;
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                bytes[i] = ByteArray[index];
                index++;
            }

            return bytes;
        }
        //  

        // --- Quaternion ---  
        public void Serialize(Quaternion q)
        {
            byteStream.AddRange(GetBytes(q));
        }

        public Quaternion DeserializeQuaternion()
        {
            Quaternion quat = new Quaternion();
            quat.x = BitConverter.ToSingle(ByteArray, index); index += 4;
            quat.y = BitConverter.ToSingle(ByteArray, index); index += 4;
            quat.z = BitConverter.ToSingle(ByteArray, index); index += 4;
            quat.w = BitConverter.ToSingle(ByteArray, index); index += 4;
            return quat;
        }
        //  
        
        // --- Color ---  
        public void Serialize(Color c)
        {
            byteStream.AddRange(GetBytes(c));
        }

        public Color DeserializeColor()
        {
            Color color = new Color();
            color.r = BitConverter.ToSingle(ByteArray, index); index += 4;
            color.g = BitConverter.ToSingle(ByteArray, index); index += 4;
            color.b = BitConverter.ToSingle(ByteArray, index); index += 4;
            color.a = BitConverter.ToSingle(ByteArray, index); index += 4;
            return color;
        }
        //

        // --- float ---  
        public void Serialize(float f)
        {
            byteStream.AddRange(BitConverter.GetBytes(f));
        }

        public float DeserializeFloat()
        {
            float f = BitConverter.ToSingle(ByteArray, index); index += 4;
            return f;
        }
        //  

        // --- int ---  
        public void Serialize(int i)
        {
            byteStream.AddRange(BitConverter.GetBytes(i));
        }

        public int DeserializeInt()
        {
            int i = BitConverter.ToInt32(ByteArray, index); index += 4;
            return i;
        }
        //  

        // --- internal ----  
        Vector3 DeserializeVector3(byte[] bytes, ref int index)
        {
            Vector3 vector3 = new Vector3();
            vector3.x = BitConverter.ToSingle(bytes, index); index += 4;
            vector3.y = BitConverter.ToSingle(bytes, index); index += 4;
            vector3.z = BitConverter.ToSingle(bytes, index); index += 4;

            return vector3;
        }

        Quaternion DeserializeQuaternion(byte[] bytes, ref int index)
        {
            Quaternion quat = new Quaternion();
            quat.x = BitConverter.ToSingle(bytes, index); index += 4;
            quat.y = BitConverter.ToSingle(bytes, index); index += 4;
            quat.z = BitConverter.ToSingle(bytes, index); index += 4;
            quat.w = BitConverter.ToSingle(bytes, index); index += 4;
            return quat;
        }
        

        byte[] GetBytes(Vector2 v)
        {
            List<byte> bytes = new List<byte>(8);
            bytes.AddRange(BitConverter.GetBytes(v.x));
            bytes.AddRange(BitConverter.GetBytes(v.y));
            return bytes.ToArray();
        }

        byte[] GetBytes(Vector3 v)
        {
            List<byte> bytes = new List<byte>(12);
            bytes.AddRange(BitConverter.GetBytes(v.x));
            bytes.AddRange(BitConverter.GetBytes(v.y));
            bytes.AddRange(BitConverter.GetBytes(v.z));
            return bytes.ToArray();
        }

        byte[] GetBytes(Quaternion q)
        {
            List<byte> bytes = new List<byte>(16);
            bytes.AddRange(BitConverter.GetBytes(q.x));
            bytes.AddRange(BitConverter.GetBytes(q.y));
            bytes.AddRange(BitConverter.GetBytes(q.z));
            bytes.AddRange(BitConverter.GetBytes(q.w));
            return bytes.ToArray();
        }

         byte[] GetBytes(Color c)
        {
            List<byte> bytes = new List<byte>(16);
            bytes.AddRange(BitConverter.GetBytes(c.r));
            bytes.AddRange(BitConverter.GetBytes(c.g));
            bytes.AddRange(BitConverter.GetBytes(c.b));
            bytes.AddRange(BitConverter.GetBytes(c.a));
            return bytes.ToArray();
        }
    }
    
}