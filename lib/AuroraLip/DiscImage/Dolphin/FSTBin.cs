using AuroraLib.Common.Node;

namespace AuroraLib.DiscImage.Dolphin
{
    public class FSTBin
    {
        public FSTEntry[] Entires;
        public long stringTableOffset;

        private readonly bool _IsGC;

        public FSTBin(Stream stream, bool isGC = true)
        {
            _IsGC = isGC;
            var root = stream.Read<FSTEntry>(Endian.Big);
            Entires = stream.For((int)root.Data - 1, S => S.Read<FSTEntry>(Endian.Big));
            stringTableOffset = stream.Position;
        }

        public void ProcessEntres(Stream stream, DirectoryNode directory)
            => ProcessEntres(stream, directory, 0, Entires.Length);

        private int ProcessEntres(Stream stream, DirectoryNode directory, int i, int l)
        {
            while (i < l)
            {
                stream.Seek(stringTableOffset + (int)Entires[i].NameOffset, SeekOrigin.Begin);
                string name = stream.ReadCString();
                if (Entires[i].IsDirectory)
                {
                    DirectoryNode subdir = new(name);
                    directory.Add(subdir);
                    i = ProcessEntres(stream, subdir, i + 1, (int)Entires[i].Data - 1);
                }
                else
                {
                    if (_IsGC)
                        directory.Add(new FileNode(name, new SubStream(stream, Entires[i].Data, Entires[i].Offset)));
                    else
                        directory.Add(new FileNode(name, new SubStream(stream, Entires[i].Data, Entires[i].Offset << 2)));
                    i++;
                }
            }
            return l;
        }

        public struct FSTEntry
        {
            public byte Flag { get; set; }
            public UInt24 NameOffset { get; set; }
            public uint Offset { get; set; } // file or parent Offset
            public uint Data { get; set; } // fileSize or numberOfFiles Offset

            public bool IsDirectory
            {
                readonly get => Flag != 0;
                set => Flag = (byte)(value ? 1 : 0);
            }
        }
    }
}
