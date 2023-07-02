using AuroraLib.Common;
using System.Reflection;

namespace AuroraLib.Archives.Formats
{
    public class ARC_FS : Archive, IFileAccess
    {
        public bool CanRead => true;

        public virtual bool CanWrite => false;

        public static readonly string[] Extension = new[] { ".tex", ".ptm" };

        public virtual bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 0x20 && Extension.Contains(extension.ToLower()) && stream.ReadUInt32(Endian.Big) == stream.Length;

        protected override void Read(Stream stream)
        {
            Root = new ArchiveDirectory() { OwnerArchive = this };
            Process(stream, Root);
        }

        protected static void Process(Stream stream, ArchiveDirectory Parent)
        {
            uint size = stream.ReadUInt32(Endian.Big);
            uint entrys = stream.ReadUInt32(Endian.Big);
            uint[] pointers = stream.Read<uint>(entrys, Endian.Big);

            for (int i = 0; i < pointers.Length; i++)
            {
                stream.Seek(pointers[i], SeekOrigin.Begin);
                uint eSize = stream.ReadUInt32(Endian.Big);
                Parent.AddArchiveFile(stream, eSize, pointers[i], $"entry{i}");
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();
    }
}
