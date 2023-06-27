using AuroraLib.Common;
using ICSharpCode.SharpZipLib.Checksum;

namespace AuroraLib.Archives.Formats
{
    //base on https://forum.xentax.com/viewtopic.php?f=10&t=5938
    public class PK_AQ : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public static string FSExtension => ".pfs";

        public static string HeaderExtension => ".pkh";

        public static string DataExtension => ".pk";

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
        {
            if (extension.ToLower() == HeaderExtension)
            {
                uint Entrys = stream.ReadUInt32(Endian.Big);
                return Entrys * 0x10 + 4 == stream.Length;
            }
            return false;
        }

        protected override void Read(Stream hStream)
        {
            //try to request external files.
            string datname = Path.ChangeExtension(Path.GetFileNameWithoutExtension(FullPath), DataExtension);
            try
            {
                pkstream = FileRequest.Invoke(datname);
            }
            catch (Exception)
            {
                throw new Exception($"{nameof(PK_AQ)}: could not request the file {datname}.");
            }

            //Read PKH
            uint Entrys = hStream.ReadUInt32(Endian.Big);
            HEntry[] hEntries = hStream.For((int)Entrys, s => s.Read<HEntry>(Endian.Big));

            try
            {
                datname = Path.ChangeExtension(Path.GetFileNameWithoutExtension(FullPath), FSExtension);
                Stream fsStream = FileRequest.Invoke(datname);

                //Read PFS
                FSHeader header = fsStream.Read<FSHeader>(Endian.Big);
                DirectoryEntry[] directories = fsStream.For((int)header.Directorys, s => s.Read<DirectoryEntry>(Endian.Big));
                uint[] directoryNameOffsets = fsStream.For((int)header.Directorys, s => s.ReadUInt32(Endian.Big));
                uint[] fileNameOffsets = fsStream.For((int)header.Files, s => s.ReadUInt32(Endian.Big));
                long nameTabelPos = fsStream.Position;

                //Process
                BZip2Crc crc32 = new();
                Dictionary<int, ArchiveDirectory> directoryPairs = new();
                for (int i = 0; i < directories.Length; i++)
                {
                    DirectoryEntry dirEntry = directories[i];
                    fsStream.Seek(nameTabelPos + directoryNameOffsets[i], SeekOrigin.Begin);
                    string name = fsStream.ReadString();

                    ArchiveDirectory dir = new()
                    {
                        OwnerArchive = this,
                        Name = name,
                    };
                    directoryPairs.Add(directories[i].Index, dir);
                    if (directories[i].ParentIndex != -1)
                    {
                        directoryPairs[dirEntry.ParentIndex].Items.Add(name, dir);
                        dir.Parent = directoryPairs[dirEntry.ParentIndex];
                    }

                    //Process FileEntrys
                    for (int fi = 0; fi < dirEntry.FileEntrys; fi++)
                    {
                        int FileIndex = dirEntry.FileStartChild + fi;
                        fsStream.Seek(nameTabelPos + fileNameOffsets[FileIndex], SeekOrigin.Begin);
                        string filename = fsStream.ReadString();
                        //Get CRC32
                        crc32.Reset();
                        crc32.Update(Path.Combine(dir.FullPath, filename).Replace('\\', '/').ToLower().Replace('?', 'L').GetBytes());
                        uint test = (uint)crc32.Value;
                        HEntry fileEntry = Array.Find(hEntries, e => e.CRC32 == crc32.Value);

                        if (fileEntry.IsCompressed)
                            dir.AddArchiveFile(pkstream, fileEntry.ComprSize, fileEntry.Offset, filename + ".lz");
                        else
                            dir.AddArchiveFile(pkstream, fileEntry.DecomSize, fileEntry.Offset, filename);
                    }
                }
                Root = directoryPairs[0];
                fsStream.Close();
            }
            catch (Exception)
            {
                //Process without PFS
                Root = new ArchiveDirectory() { OwnerArchive = this };
                for (int i = 0; i < Entrys; i++)
                {
                    if (hEntries[i].IsCompressed)
                        Root.AddArchiveFile(pkstream, hEntries[i].ComprSize, hEntries[i].Offset, $"{i}_{hEntries[i].CRC32}.lz");
                    else
                        Root.AddArchiveFile(pkstream, hEntries[i].DecomSize, hEntries[i].Offset, $"{i}_{hEntries[i].CRC32}.bin");
                }
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        public struct FSHeader
        {
            public uint Pad1;
            public uint Pad2;
            public uint Directorys;
            public uint Files;
        }

        public struct DirectoryEntry
        {
            public int Index;
            public int ParentIndex;
            public int DirectoryStartChild;
            public uint DirectoryEntrys;
            public int FileStartChild;
            public uint FileEntrys;
        }

        public struct HEntry
        {
            public uint CRC32;
            public uint Offset;
            public uint DecomSize;
            public uint ComprSize;

            public bool IsCompressed => ComprSize != 0;
        }

        private Stream pkstream;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (pkstream != null)
                {
                    pkstream.Dispose();
                }
            }
        }
    }
}
