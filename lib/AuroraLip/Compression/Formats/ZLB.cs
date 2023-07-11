using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Compression.Formats
{
    public class ZLB : ICompression, IHasIdentifier
    {

        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new((byte)'Z', (byte)'L', (byte)'B', 0x0);

        private static readonly ZLib ZLipInstance = new();

        public void Compress(in byte[] source, Stream destination)
            => throw new NotImplementedException();

        public byte[] Decompress(Stream source)
        {
            MemoryStream buffer = new();
            do
            {
                if (!IsMatch(source))
                {
                    if (!source.Search(_identifier.AsSpan().ToArray()))
                    {
                        break;
                    }
                    continue;
                }
                Header header = source.Read<Header>(Endian.Big);

                ZLipInstance.Decompress(source.Read((int)header.CompLength), buffer);
                source.Align(32);
            } while (source.Position + 16 < source.Length);
            return buffer.ToArray();
        }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier);

        public unsafe struct Header
        {
            public uint Version;
            public uint DeLength;
            public uint CompLength;
        }

    }
}
