using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Buffers;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// From Software Archive
    /// </summary>
    public sealed class ARC_FS : ArchiveNode
    {
        public override bool CanWrite => false;

        private static readonly string[] extensions = new[] { ".tex", ".ptm", ".ctm", "" };

        public ARC_FS()
        {
        }

        public ARC_FS(string name) : base(name)
        {
        }

        public ARC_FS(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
        {
            bool match = false;
            for (int i = 0; i < extensions.Length; i++)
            {
                if (extension.Contains(extensions[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    match = true;
                    break;
                }
            }

            if (stream.Length > 0x20 && match && stream.ReadUInt32(Endian.Big) == stream.Length)
            {
                uint entrys = stream.ReadUInt32(Endian.Big);
                using SpanBuffer<uint> pointers = new((int)entrys);
                stream.Read(pointers.Span, Endian.Big);
                long pos = pointers[0];
                for (int i = 0; i < pointers.Length; i++)
                {
                    if (pos != pointers[i])
                    {
                        return false;
                    }
                    stream.Seek(pointers[i], SeekOrigin.Begin);
                    pos += stream.ReadUInt32(Endian.Big);
                }
                return true;
            }
            return false;
        }

        protected override void Deserialize(Stream source)
        {
            Process(source, this);
            if (Count == 2)
            {
                Stream texStream = ((FileNode)Values.First()).Data;
                texStream.Seek(8, SeekOrigin.Begin);
                uint gtxPos = texStream.ReadUInt32(Endian.Big);
                texStream.Seek(gtxPos + 4, SeekOrigin.Begin);
                Identifier32 identifier = texStream.Read<Identifier32>();
                texStream.Seek(0, SeekOrigin.Begin);
                if (identifier == 827872327) //GTX1
                {
                    using FileNode texturdata = (FileNode)Values.First();
                    FileNode modeldata = (FileNode)Values.Last();
                    DirectoryNode texturs = new("Textures");
                    Add(texturs);
                    modeldata.Name = "Model";
                    Process(texturdata.Data, texturs);
                    Remove(texturdata);
                }
            }
        }

        private static void Process(Stream source, DirectoryNode parent)
        {
            uint size = source.ReadUInt32(Endian.Big);
            uint entrys = source.ReadUInt32(Endian.Big);
            using SpanBuffer<uint> pointers = new((int)entrys);
            source.Read(pointers.Span, Endian.Big);

            for (int i = 0; i < pointers.Length; i++)
            {
                source.Seek(pointers[i], SeekOrigin.Begin);
                uint eSize = source.ReadUInt32(Endian.Big);
                parent.Add(new FileNode($"entry{i}", new SubStream(source, eSize, pointers[i])));
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();
    }
}
