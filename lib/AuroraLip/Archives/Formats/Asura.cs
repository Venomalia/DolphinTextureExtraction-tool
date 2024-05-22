using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Rebellion Asura Archive
    /// </summary>
    public sealed class Asura : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier64 _identifier = new("Asura   ");

        public Asura()
        {
        }

        public Asura(string name) : base(name)
        {
        }

        public Asura(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 0x20 && stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);
            Endian endian = source.At(0xC, s => s.DetectByteOrder<int>());

            int i = 0;
            while (source.Position < source.Length - 4)
            {
                long startpos = source.Position;

                string magic = source.ReadString(4);
                uint size = source.ReadUInt32(endian);

                if (magic == string.Empty || size == 0)
                {
                    break;
                }

                int nameoffset = GetNamePos(magic);
                if (nameoffset != -1)
                {
                    source.Seek(startpos + nameoffset, SeekOrigin.Begin);
                    string name = source.ReadString().TrimStart('\\');

                    if (magic == "FCSR")
                    {
                        source.Align(4);
                        size -= (uint)(source.Position - startpos);
                        startpos = source.Position;
                    }


                    Add(new FileNode($"{name}~ID{i++}.{magic.ToLower()}", new SubStream(source, size, startpos)));
                }
                else
                {
                    Add(new FileNode($"{magic}~ID{i++}.{magic.ToLower()}", new SubStream(source, size, startpos)));
                }

                //go to next
                source.Seek(startpos + size, SeekOrigin.Begin);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        private static int GetNamePos(string magic) => magic switch
        {
            "NACH" => 0x2C,
            "STUC" => 0x24,
            "TPXF" => 0x14,
            "TEXF" => 0x14,
            "TPMH" => 0x14,
            "TSXF" => 0x14,
            "DNSH" => 0x14,
            "NAIU" => 0x14,
            "CATC" => 0x18,
            "NKSH" => 0x18,
            "FCSR" => 0x1C,
            "PMIU" => 0x1C,
            "RTTC" => 0x1C,
            "VELD" => 0x10,
            "AMDS" => 0x10,
            "BBSH" => 0x10,
            "NLLD" => 0x10,
            "TLLD" => 0x10,
            "VEDS" => 0x10,
            "MSDS" => 0x10,
            _ => -1,
        };
    }
}
