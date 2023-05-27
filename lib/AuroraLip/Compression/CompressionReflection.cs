using AuroraLib.Common;

namespace AuroraLib.Compression
{
    public class CompressionReflection : FileAccessReflection<ICompression>
    {
        public CompressionReflection() : base()
        {
        }

        /// <summary>
        /// Trying to decompress the data
        /// </summary>
        /// <param name="stream">stream to be decrypted</param>
        /// <param name="outstream">Decompressed stream</param>
        /// <param name="type">Matching algorithm</param>
        /// <returns></returns>
        public bool TryToDecompress(Stream stream, out Stream outstream, out Type type)
        {
            var startPosition = stream.Position;

            foreach (var Instance in Instances)
            {
                if (Instance.CanRead && Instance.IsMatch(stream))
                {
                    try
                    {
                        stream.Seek(startPosition, SeekOrigin.Begin);
                        outstream = new MemoryStream(Instance.Decompress(stream));
                        type = Instance.GetType();
                        return true;
                    }
                    catch (Exception)
                    {
                    }
                }
                stream.Seek(startPosition, SeekOrigin.Begin);
            }

            outstream = null;
            type = null;
            return false;
        }
    }
}
