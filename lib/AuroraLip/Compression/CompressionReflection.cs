using AuroraLib.Common;
using AuroraLib.Compression.Interfaces;
using System.IO;

namespace AuroraLib.Compression
{
    public class CompressionReflection : FileAccessReflection<ICompressionAlgorithm>
    {
        public CompressionReflection() : base()
        {
        }

        /// <summary>
        /// Trying to decompress the data
        /// </summary>
        /// <param name="source">stream to be decrypted</param>
        /// <param name="destination">Decompressed stream</param>
        /// <param name="type">Matching algorithm</param>
        /// <returns></returns>
        public bool TryToDecompress(Stream source, out Stream destination, out Type type)
        {
            destination = new MemoryPoolStream();

            foreach (ICompressionDecoder decoder in Instances)
            {
                try
                {
                    if (source.Peek(s => decoder.IsMatch(s)))
                    {
                        decoder.Decompress(source, destination);
                        type = decoder.GetType();

                        if (source.Position + 0x10 < source.Length) // Segmented?
                        {
                            DecompressSegmented(decoder, source, destination);
                        }

                        return true;
                    }
                }
                catch (Exception t)
                {

                    destination.SetLength(0);
                }
            }

            destination.Dispose();
            destination = null;
            type = null;
            return false;
        }

        private static void DecompressSegmented(ICompressionDecoder algorithm, Stream source, Stream destination)
        {
            while (source.Position + 0x10 < source.Length)
            {
                while (source.ReadByte() == 0)
                { }
                source.Position--;

                if (source.Peek(s => algorithm.IsMatch(s)))
                {
                    algorithm.Decompress(source, destination);
                }
                else
                {
                    break;
                }
            }
        }
    }
}
