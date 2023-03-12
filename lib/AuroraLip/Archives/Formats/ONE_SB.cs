using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Archive use in the Sonic Storybook Series
    /// </summary>
    public class ONE_SB : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public static string Extension => ".one";

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
            => extension.ToLower() == Extension && stream.At(4, SeekOrigin.Begin, S => S.ReadUInt32(Endian.Big)) == 16;

        protected override void Read(Stream stream)
        {
            uint numEntries = stream.ReadUInt32(Endian.Big);
            uint offset = stream.ReadUInt32(Endian.Big); //16
            uint unk = stream.ReadUInt32(Endian.Big);
            int version = stream.ReadInt32(Endian.Big); // 0 Sonic and the Secret Rings or -1 for Sonic and the Black Knight

            stream.Seek(offset, SeekOrigin.Begin);

            Root = new ArchiveDirectory() { OwnerArchive = this };
            for (int i = 0; i < numEntries; i++)
            {
                string entryFilename = stream.ReadString(32) + ".lz";

                uint entryIndex = stream.ReadUInt32(Endian.Big);
                uint entryOffset = stream.ReadUInt32(Endian.Big);
                uint entryLength = stream.ReadUInt32(Endian.Big);
                uint entryUnk = stream.ReadUInt32(Endian.Big);

                if (Root.ItemKeyExists(entryFilename))
                {
                    Root.AddArchiveFile(stream, entryLength, entryOffset, entryFilename + i);
                }
                else
                {
                    Root.AddArchiveFile(stream, entryLength, entryOffset, entryFilename);
                }
            }
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
