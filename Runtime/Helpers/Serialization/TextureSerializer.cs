using UnityEngine;

namespace CipherDuo.Utility.Serialization
{
    public static class TextureSerializer
    {
        public static TexturePacket.TextureFile Serialize(Texture2D tex, bool useAlpha = false)
        {
            var serTexture = new TexturePacket.TextureFile();

            serTexture.content = useAlpha ? tex.EncodeToPNG() : tex.EncodeToJPG();

            return serTexture;
        }

        public static Texture2D Deserialize(byte[] data, TexturePacket.TextureMetadata serTexture)
        {
            Texture2D tex = new Texture2D(serTexture.heigth, serTexture.width, serTexture.format, false);

            ImageConversion.LoadImage(tex, data);

            return tex;
        }
    }
}
