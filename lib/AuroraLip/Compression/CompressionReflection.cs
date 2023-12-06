using AuroraLib.Common;
using AuroraLib.Compression.Interfaces;

namespace AuroraLib.Compression
{
    public class CompressionReflection : FileAccessReflection<ICompressionAlgorithm>
    {
        public CompressionReflection() : base()
        {
        }

        public bool TryToDecompress(Stream source, out List<Stream> destinations, out Type type)
        {
            destinations = new();
            if (TryToDecompress(source, out Stream destination, out ICompressionDecoder decoder))
            {
                type = decoder.GetType();
                destinations.Add(destination);
                while (source.Position + 0x10 < source.Length)
                {
                    while (source.ReadByte() == 0)
                    { }
                    source.Position--;
                    if (source.Peek(s => decoder.IsMatch(s)))
                    {
                        destination = new MemoryPoolStream();
                        decoder.Decompress(source, destination);
                        destinations.Add(destination);
                    }
                    else
                    {
                        break;
                    }
                }
                return true;
            }
            destinations = null;
            type = null;
            return false;
        }

        public bool TryToDecompress(Stream source, out Stream destination, out ICompressionDecoder type)
        {
            destination = new MemoryPoolStream();

            foreach (ICompressionDecoder decoder in Instances)
            {
                try
                {
                    if (source.Peek(s => decoder.IsMatch(s)))
                    {
                        decoder.Decompress(source, destination);
                        type = decoder;
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
    }
}
