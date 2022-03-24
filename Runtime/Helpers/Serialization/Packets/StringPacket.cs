namespace CipherDuo.Utility.Serialization
{
    [System.Serializable]
    public class StringPacket
    {
        public struct StringMetadata
        {
            public string hash;
        }

        public struct StringFile
        {
            public byte[] content;
        }

        public static StringMetadata Metadata(string msg)
        {
            var metString = new StringMetadata();

            return metString;
        }
    }
}