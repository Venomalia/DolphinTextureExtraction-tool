﻿using AuroraLib.Common;
using AuroraLib.Core.Buffers;
using AuroraLib.Core.Cryptography;
using AuroraLib.Core.Text;

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

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
        {
            if (extension.Contains(HeaderExtension, StringComparison.InvariantCultureIgnoreCase))
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
            using SpanBuffer<HEntry> hEntries = new((int)Entrys);
            hStream.Read(hEntries.Span, Endian.Big);

            try
            {
                datname = Path.ChangeExtension(Path.GetFileNameWithoutExtension(FullPath), FSExtension);
                Stream fsStream = FileRequest.Invoke(datname);

                //Read PFS
                FSHeader header = fsStream.Read<FSHeader>(Endian.Big);


                using SpanBuffer<DirectoryEntry> directories = new((int)header.Directorys);
                fsStream.Read(directories.Span, Endian.Big);
                using SpanBuffer<uint> directoryNameOffsets = new((int)header.Directorys);
                fsStream.Read(directoryNameOffsets.Span, Endian.Big);
                using SpanBuffer<uint> fileNameOffsets = new((int)header.Files);
                fsStream.Read(fileNameOffsets.Span, Endian.Big);
                long nameTabelPos = fsStream.Position;

                //Process
                Crc32 crc32 = new(Crc32Algorithm.BZIP2);
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
                        crc32.Compute(Path.Combine(dir.FullPath, filename).Replace('\\', '/').ToLower().Replace('?', 'L').GetBytes());

                        HEntry fileEntry = default;
                        foreach (HEntry entry in hEntries)
                        {
                            if (entry.CRC32 == crc32.Value)
                            {
                                fileEntry = entry;
                                break;
                            }
                        }

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
