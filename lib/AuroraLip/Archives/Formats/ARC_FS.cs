using AuroraLib.Common;
using AuroraLib.Core.IO;
using System;

namespace AuroraLib.Archives.Formats
{
    public class ARC_FS : Archive, IFileAccess
    {
        public bool CanRead => true;

        public virtual bool CanWrite => false;

        public static readonly string[] Extension = new[] { ".tex", ".ptm", ".ctm", "" };

        public virtual bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
        {
            bool match = false;
            for (int i = 0; i < Extension.Length; i++)
            {
                if (extension.Contains(Extension[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    match = true;
                    break;
                }
            }

            if (stream.Length > 0x20 && match && stream.ReadUInt32(Endian.Big) == stream.Length)
            {
                uint entrys = stream.ReadUInt32(Endian.Big);
                uint[] pointers = stream.Read<uint>(entrys, Endian.Big);
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

        protected override void Read(Stream stream)
        {
            Root = new ArchiveDirectory() { OwnerArchive = this };
            Process(stream, Root);
            if (Root.Items.Count == 2)
            {
                Stream texStream = ((ArchiveFile)Root.Items.Values.First()).FileData;
                texStream.Seek(8, SeekOrigin.Begin);
                uint gtxPos = texStream.ReadUInt32(Endian.Big);
                texStream.Seek(gtxPos + 4, SeekOrigin.Begin);
                Identifier32 identifier = texStream.Read<Identifier32>();
                texStream.Seek(0, SeekOrigin.Begin);
                if (identifier == 827872327) //GTX1
                {
                    ArchiveDirectory helper = Root;
                    Root = new ArchiveDirectory() { OwnerArchive = this };
                    ArchiveDirectory texturs = new(this, Root) { Name = "Textures" };
                    Root.Items.Add(texturs.Name, texturs);
                    ArchiveObject modeldata = helper.Items.Values.Last();
                    modeldata.Name = "Model";
                    Root.Items.Add(modeldata.Name, modeldata);
                    Process(((ArchiveFile)helper.Items.Values.First()).FileData, texturs);
                }
            }
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
