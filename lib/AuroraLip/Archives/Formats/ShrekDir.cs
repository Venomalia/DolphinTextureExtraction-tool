using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Compression.Algorithms;
using AuroraLib.Core.Buffers;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Shaba Games Shrek SuperSlam Dir
    /// </summary>
    public sealed class ShrekDir : ArchiveNode
    {
        public override bool CanWrite => false;

        public ShrekDir()
        {
        }

        public ShrekDir(string name) : base(name)
        {
        }

        public ShrekDir(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
        {
            if (extension.SequenceEqual(".DIR"))
            {
                Endian endian = stream.DetectByteOrder<uint>();
                uint firstOffset = stream.ReadUInt32(endian);
                stream.Position = firstOffset - 8;
                uint lastOffset = stream.ReadUInt32(endian);
                uint end = stream.ReadUInt32(endian); // 0
                stream.Position = lastOffset;
                Entry entry = new(stream, endian);
                return stream.Position == stream.Length && end == 0;
            }
            return false;
        }

        protected override void Deserialize(Stream source)
        {
            //try to request an external file.
            string datname = Path.GetFileNameWithoutExtension(Name) + ".DAT";
            if (TryGetRefFile(datname, out FileNode refFile))
            {
                using Stream sourceDat = refFile.Data;

                Endian endian = source.DetectByteOrder<uint>();
                //Starts with a pointer list, last entry ends with 0x0
                uint firstOffset = source.ReadUInt32(endian);

                uint files = (firstOffset - 8) / 4;
                source.Position = firstOffset;
                for (int i = 0; i < files; i++)
                {
                    Entry entry = new(source, endian);
                    if (!Contains(entry.Name))
                    {
                        using SpanBuffer<byte> buffer = new((int)entry.CompSize);
                        sourceDat.Seek(entry.Offset, SeekOrigin.Begin);
                        sourceDat.Read(buffer.Span);
                        MemoryPoolStream decomp = new();
                        LZShrek.DecompressHeaderless(buffer, decomp, (int)entry.DecompSize);
                        FileNode file = new(entry.Name, decomp);
                        Add(file);
                    }
                }
            }
            else
            {
                throw new Exception($"{nameof(ShrekDir)}: could not request the file {datname}.");
            }
        }

        private struct Entry
        {
            public uint Offset;
            public uint DecompSize;
            public uint CompSize;
            public string Name;

            public Entry(Stream stream, Endian endian)
            {
                Offset = stream.ReadUInt32(endian);
                DecompSize = stream.ReadUInt32(endian);
                CompSize = stream.ReadUInt32(endian);
                Name = stream.ReadString();
                stream.Align(4);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();
    }
}
