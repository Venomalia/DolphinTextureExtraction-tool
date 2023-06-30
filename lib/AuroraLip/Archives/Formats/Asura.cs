using AuroraLib.Common;
using AuroraLib.Common.Struct;

namespace AuroraLib.Archives.Formats
{
    public class Asura : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier64 _identifier = new("Asura   ");

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 0x20 && stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);

            Endian endian = stream.At(0xC, s => s.DetectByteOrder<int>());

            Root = new ArchiveDirectory() { OwnerArchive = this };

            int i = 0;
            while (stream.Position < stream.Length - 4)
            {
                long startpos = stream.Position;

                string magic = stream.ReadString(4);
                uint size = stream.ReadUInt32(endian);

                if (magic == string.Empty || size == 0)
                {
                    break;
                }

                int nameoffset = GetNamePos(magic);
                if (nameoffset != -1)
                {
                    stream.Seek(startpos + nameoffset, SeekOrigin.Begin);
                    string name = stream.ReadString().TrimStart('\\');

                    if (magic == "FCSR")
                    {
                        stream.Align(4);
                        size -= (uint)(stream.Position - startpos);
                        startpos = stream.Position;
                    }


                    Root.AddArchiveFile(stream, size, startpos, $"{name}~ID{i++}.{magic.ToLower()}");
                }
                else
                {
                    Root.AddArchiveFile(stream, size, startpos, $"{magic}~ID{i++}.{magic.ToLower()}");
                }

                //go to next
                stream.Seek(startpos + size, SeekOrigin.Begin);
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

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
