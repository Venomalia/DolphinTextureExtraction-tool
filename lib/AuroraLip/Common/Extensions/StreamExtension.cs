using System.IO;

namespace AuroraLip.Common
{
    public static class StreamExtension
    {
        public static byte[] ToArray(this Stream stream)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
