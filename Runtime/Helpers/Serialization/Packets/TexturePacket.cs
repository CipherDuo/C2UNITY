using UnityEngine;

namespace CipherDuo.Utility.Serialization
{
    [System.Serializable]
    public class TexturePacket
    {
        public struct TextureMetadata
        {
            public string hash;
            public int heigth;
            public int width;
            public TextureFormat format;
        }

        public struct TextureFile
        {
            public byte[] content;
        }
        
        public static TextureMetadata Metadata(Texture2D tex)
        {
            var metTexture = new TextureMetadata();

            metTexture.heigth = tex.height;
            metTexture.width = tex.width;
            metTexture.format = tex.format;

            return metTexture;
        }
    }
}
