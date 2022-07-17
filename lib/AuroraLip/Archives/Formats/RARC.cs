using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AuroraLip.Archives.Formats
{
    /*
    * Super Hackio Incorporated
    * "Copyright © Super Hackio Incorporated 2020-2021"
    * https://github.com/SuperHackio/Hack.io
    */

    /// <summary>
    /// Nintendo File Archive used in WII/GC Games.
    /// <para/> NOTE: THIS IS NOT A U8 ARCHIVE!
    /// </summary>
    public class RARC : Archive, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public string Magic => magic;

        private const string magic = "RARC";

        public RARC() { }

        public RARC(string filename) : base(filename) { }

        public RARC(Stream stream, string filename = null) : base(stream, filename) { }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        #region Fields and Properties
        /// <summary>
        /// If false, the user must set all unique ID's for each file
        /// </summary>
        public bool KeepFileIDsSynced { get; set; } = true;
        /// <summary>
        /// Gets the next free File ID
        /// </summary>
        public short NextFreeFileID => GetNextFreeID();
        #endregion

        /// <summary>
        /// Folder contained inside the Archive. Can contain more <see cref="Directory"/>s if desired, as well as <see cref="File"/>s
        /// </summary>
        public class Directory : ArchiveDirectory
        {
            /// <summary>
            /// Create a new Archive Directory
            /// </summary>
            public Directory() { }
            /// <summary>
            /// Create a new, child directory
            /// </summary>
            /// <param name="Owner">The Owner Archive</param>
            /// <param name="parentdir">The Parent Directory. NULL if this is the Root Directory</param>
            public Directory(RARC Owner, Directory parentdir) { OwnerArchive = Owner; Parent = parentdir; }
            /// <summary>
            /// Import a Folder into a RARCDirectory
            /// </summary>
            /// <param name="FolderPath"></param>
            /// <param name="Owner"></param>
            public Directory(string FolderPath, RARC Owner)
            {
                DirectoryInfo DI = new DirectoryInfo(FolderPath);
                Name = DI.Name;
                CreateFromFolder(FolderPath, Owner);
                OwnerArchive = Owner;
            }
            internal Directory(RARC Owner, int ID, List<RARCDirEntry> DirectoryNodeList, List<RARCFileEntry> FlatFileList, uint DataBlockStart, Stream RARCFile)
            {
                OwnerArchive = Owner;
                Name = DirectoryNodeList[ID].Name;
                for (int i = (int)DirectoryNodeList[ID].FirstFileOffset; i < DirectoryNodeList[ID].FileCount + DirectoryNodeList[ID].FirstFileOffset; i++)
                {
                    //IsDirectory
                    if (FlatFileList[i].Type == 0x0200)
                    {
                        Items.Add(FlatFileList[i].Name, FlatFileList[i]);
                    }
                    else
                    {
                        Items.Add(FlatFileList[i].Name, new File(FlatFileList[i], DataBlockStart, RARCFile));
                    }
                }
            }

            internal string ToTypeString() => Name.ToUpper().PadRight(4, ' ').Substring(0, 4);
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString() => $"{Name} - {Items.Count} Item(s)";
            /// <summary>
            /// Create an ArchiveDirectory. You cannot use this function unless this directory is empty
            /// </summary>
            /// <param name="FolderPath"></param>
            /// <param name="OwnerArchive"></param>
            public new void CreateFromFolder(string FolderPath, Archive OwnerArchive = null)
            {
                if (!(OwnerArchive is RARC r))
                    throw new Exception();

                if (Items.Count > 0)
                    throw new Exception("Cannot create a directory from a folder if Items exist");
                string[] Found = System.IO.Directory.GetFiles(FolderPath, "*.*", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < Found.Length; i++)
                {
                    File temp = new File(Found[i]);
                    Items[temp.Name] = temp;
                }

                string[] SubDirs = System.IO.Directory.GetDirectories(FolderPath, "*.*", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < SubDirs.Length; i++)
                {
                    Directory temp = new Directory(SubDirs[i], r);
                    Items[temp.Name] = temp;
                }
            }
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            protected override ArchiveDirectory NewDirectory() => new Directory();
            /// <summary>
            /// 
            /// </summary>
            /// <param name="Owner"></param>
            /// <param name="parent"></param>
            /// <returns></returns>
            protected override ArchiveDirectory NewDirectory(Archive Owner, ArchiveDirectory parent) => new Directory((RARC)Owner, (Directory)parent);
            /// <summary>
            /// 
            /// </summary>
            /// <param name="filename"></param>
            /// <param name="Owner"></param>
            /// <returns></returns>
            protected override ArchiveDirectory NewDirectory(string filename, Archive Owner) => new Directory(filename, (RARC)Owner);
        }

        /// <summary>
        /// File contained inside the Archive
        /// </summary>
        public class File : ArchiveFile
        {
            /// <summary>
            /// Extra settings for this File.<para/>Default: <see cref="FileAttribute.FILE"/> | <see cref="FileAttribute.PRELOAD_TO_MRAM"/>
            /// </summary>
            public FileAttribute FileSettings { get; set; } = FileAttribute.FILE | FileAttribute.PRELOAD_TO_MRAM;
            /// <summary>
            /// The ID of the file in the archive
            /// </summary>
            public short ID { get; set; } = -1;
            /// <summary>
            /// Empty file
            /// </summary>
            public File() { }
            /// <summary>
            /// Load a File's Data based on a path
            /// </summary>
            /// <param name="Filepath"></param>
            public File(string Filepath) : base(Filepath)
            {
            }
            /// <summary>
            /// Create a File from a MemoryStream
            /// </summary>
            /// <param name="name">The name of the file</param>
            /// <param name="ms">The Memory Stream to use</param>
            public File(string name, MemoryStream ms) : base(name, ms)
            {
            }
            internal File(RARCFileEntry entry, uint DataBlockStart, Stream RARCFile)
            {
                Name = entry.Name;
                FileSettings = entry.RARCFileType;
                ID = entry.FileID;
                RARCFile.Position = DataBlockStart + entry.ModularA;
                FileData = RARCFile.Read(entry.ModularB);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString() => $"{ID} - {Name} ({FileSettings.ToString()}) [0x{FileData.Length.ToString("X8")}]";

            //=====================================================================

            /// <summary>
            /// Cast a File to a MemoryStream
            /// </summary>
            /// <param name="x"></param>
            public static explicit operator MemoryStream(File x) => new MemoryStream(x.FileData);
        }

        #region Internals
        /// <summary>
        /// Only used when Reading / Writing
        /// </summary>
        internal class RARCDirEntry
        {
            /// <summary>
            /// Directory Type. usually the first 4 letters of the <see cref="Name"/>. If the <see cref="Name"/> is shorter than 4, the missing spots will be ' ' (space)
            /// </summary>
            public string Type { get; set; }
            public string Name { get; set; }
            public uint NameOffset { get; set; }
            public ushort NameHash { get; set; }
            public ushort FileCount { get; set; }
            public uint FirstFileOffset { get; set; }

            public RARCDirEntry() { }
            public RARCDirEntry(Stream RARCFile, uint StringTableOffset)
            {
                Type = RARCFile.ReadString(4);
                NameOffset = BitConverter.ToUInt32(RARCFile.ReadBigEndian(4), 0);
                NameHash = BitConverter.ToUInt16(RARCFile.ReadBigEndian(2), 0);
                FileCount = BitConverter.ToUInt16(RARCFile.ReadBigEndian(2), 0);
                FirstFileOffset = BitConverter.ToUInt32(RARCFile.ReadBigEndian(4), 0);

                long pauseposition = RARCFile.Position;
                RARCFile.Position = StringTableOffset + NameOffset;
                Name = RARCFile.ReadString();
                RARCFile.Position = pauseposition;
            }

            internal void Write(Stream RARCFile, Dictionary<string, uint> StringLocations)
            {

            }

            public override string ToString() => $"{Name} ({Type}) [0x{NameHash.ToString("X4")}] {FileCount} File(s)";
        }

        /// <summary>
        /// Only used when Reading / Writing
        /// </summary>
        internal class RARCFileEntry
        {
            public short FileID;
            public short Type;
            public string Name;
            /// <summary>
            /// For files: offset to file data in file data section, for subdirectories: index of the corresponding directory node
            /// </summary>
            public int ModularA;
            /// <summary>
            /// For files: size of the file, for subdirectories: always 0x10 (size of the node entry?)
            /// </summary>
            public int ModularB;
            internal short NameHash;
            internal FileAttribute RARCFileType => (FileAttribute)((Type & 0xFF00) >> 8);

            public override string ToString() => $"({FileID}) {Name}, {Type.ToString("X").PadLeft(4, '0')} ({RARCFileType.ToString()}), [{ModularA.ToString("X").PadLeft(8, '0')}][{ModularB.ToString("X").PadLeft(8, '0')}]";
        }

        /// <summary>
        /// Generates a 2 byte hash from a string
        /// </summary>
        /// <param name="Input">string to convert</param>
        /// <returns>hashed string</returns>
        internal ushort StringToHash(string Input)
        {
            int Hash = 0;
            for (int i = 0; i < Input.Length; i++)
            {
                Hash *= 3;
                Hash += Input[i];
                Hash = 0xFFFF & Hash; //cast to short 
            }

            return (ushort)Hash;
        }
        #endregion

        #region Privates
        /// <summary>
        /// Sets new file's File ID
        /// </summary>
        /// <param name="value"></param>
        /// <param name="Path"></param>
        protected override void OnItemSet(object value, string Path)
        {
            if (!KeepFileIDsSynced && value is File file && file.ID == -1 && !ItemExists(Path))
                file.ID = GetNextFreeID();
        }

        protected override ArchiveDirectory NewDirectory() => new Directory();

        protected override ArchiveDirectory NewDirectory(Archive Owner, ArchiveDirectory parent) => new Directory((RARC)Owner, (Directory)parent);

        protected override ArchiveDirectory NewDirectory(string filename, Archive Owner) => new Directory(filename, (RARC)Owner);

        protected override void Read(Stream RARCFile)
        {
            #region Header
            if (!IsMatch(RARCFile))
                throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");
            uint FileSize = BitConverter.ToUInt32(RARCFile.ReadBigEndian(4), 0),
                DataHeaderOffset = BitConverter.ToUInt32(RARCFile.ReadBigEndian(4), 0),
                DataOffset = BitConverter.ToUInt32(RARCFile.ReadBigEndian(4), 0) + 0x20,
                DataLength = BitConverter.ToUInt32(RARCFile.ReadBigEndian(4), 0),
                MRAMSize = BitConverter.ToUInt32(RARCFile.ReadBigEndian(4), 0),
                ARAMSize = BitConverter.ToUInt32(RARCFile.ReadBigEndian(4), 0);
            RARCFile.Position += 0x04; //Skip the supposed padding
            #endregion

            #region Data Header
            uint DirectoryCount = BitConverter.ToUInt32(RARCFile.ReadBigEndian(4), 0),
                    DirectoryTableOffset = BitConverter.ToUInt32(RARCFile.ReadBigEndian(4), 0) + 0x20,
                    FileEntryCount = BitConverter.ToUInt32(RARCFile.ReadBigEndian(4), 0),
                    FileEntryTableOffset = BitConverter.ToUInt32(RARCFile.ReadBigEndian(4), 0) + 0x20,
                    StringTableSize = BitConverter.ToUInt32(RARCFile.ReadBigEndian(4), 0),
                    StringTableOffset = BitConverter.ToUInt32(RARCFile.ReadBigEndian(4), 0) + 0x20;
            ushort NextFreeFileID = BitConverter.ToUInt16(RARCFile.ReadBigEndian(2), 0);
            KeepFileIDsSynced = RARCFile.ReadByte() != 0x00;
            #endregion

#if DEBUG
            //string XML = $"<RarcHeader Magic=\"{Magic}\"  FileSize=\"0x{FileSize.ToString("X8")}\"  DataHeaderOffset=\"0x{DataHeaderOffset.ToString("X8")}\"  DataOffset=\"0x{DataOffset.ToString("X8")}\"  DataLength=\"0x{DataLength.ToString("X8")}\"  MRAM=\"0x{MRAMSize.ToString("X8")}\"  ARAM=\"0x{ARAMSize.ToString("X8")}\"/>\n" +
            //    $"<DataHeader DirectoryCount=\"{DirectoryCount.ToString("2")}\"  DirectoryTableOffset=\"0x{DirectoryTableOffset.ToString("X8")}\"  FileEntryCount=\"{FileEntryCount.ToString("2")}\"  FileEntryTableOffset=\"0x{FileEntryTableOffset.ToString("X8")}\"  StringTableSize=\"0x{StringTableSize.ToString("X8")}\"  StringTableOffset=\"0x{StringTableOffset.ToString("X8")}\"  NextFreeID=\"{NextFreeFileID.ToString("0000")}\"  SyncFileIDs=\"{KeepFileIDsSynced}\"/>\n";
#endif

            #region Directory Nodes
            RARCFile.Position = DirectoryTableOffset;

            List<RARCDirEntry> FlatDirectoryList = new List<RARCDirEntry>();

            for (int i = 0; i < DirectoryCount; i++)
#if DEBUG
            {
                RARCDirEntry DEBUGTEMP = new RARCDirEntry(RARCFile, StringTableOffset);
                FlatDirectoryList.Add(DEBUGTEMP);
                long pauseposition = RARCFile.Position;
                RARCFile.Position = StringTableOffset + DEBUGTEMP.NameOffset;
                string DEBUGDIRNAME = RARCFile.ReadString();
                //XML += $"<RarcDirectoryEntry Name=" + ($"\"{DEBUGDIRNAME}\"").PadRight(20, ' ') + $" Type=\"{DEBUGTEMP.Type.PadLeft(4, ' ')}\" NameHash=\"0x{DEBUGTEMP.NameHash.ToString("X4")}\" FirstFileOffset=\"{DEBUGTEMP.FirstFileOffset}\" FileCount=\"{DEBUGTEMP.FileCount}\"/>\n";
                RARCFile.Position = pauseposition;
            }
#else
                FlatDirectoryList.Add(new RARCDirEntry(RARCFile, StringTableOffset));
#endif
            #endregion

            #region File Nodes
            List<RARCFileEntry> FlatFileList = new List<RARCFileEntry>();
            RARCFile.Seek(FileEntryTableOffset, SeekOrigin.Begin);
            for (int i = 0; i < FileEntryCount; i++)
            {
                FlatFileList.Add(new RARCFileEntry()
                {
                    FileID = BitConverter.ToInt16(RARCFile.ReadBigEndian(2), 0),
                    NameHash = BitConverter.ToInt16(RARCFile.ReadBigEndian(2), 0),
                    Type = BitConverter.ToInt16(RARCFile.ReadBigEndian(2), 0)
                });
                ushort CurrentNameOffset = BitConverter.ToUInt16(RARCFile.ReadBigEndian(2), 0);
                FlatFileList[FlatFileList.Count - 1].ModularA = BitConverter.ToInt32(RARCFile.ReadBigEndian(4), 0);
                FlatFileList[FlatFileList.Count - 1].ModularB = BitConverter.ToInt32(RARCFile.ReadBigEndian(4), 0);
                RARCFile.Position += 0x04;
                long Pauseposition = RARCFile.Position;
                RARCFile.Seek(StringTableOffset + CurrentNameOffset, SeekOrigin.Begin);
                FlatFileList[FlatFileList.Count - 1].Name = RARCFile.ReadString();
                RARCFile.Position = Pauseposition;
            }
#if DEBUG
            //for (int i = 0; i < FlatFileList.Count; i++)
            //    XML += $"<RarcFileEntry ID=\"{FlatFileList[i].FileID.ToString("0000").PadLeft(5, '+')}\" Name=" + ($"\"{FlatFileList[i].Name}\"").PadRight(30, ' ') + $" Type=\"{FlatFileList[i].Type.ToString("X4")}\"\t RARCFileType=\"{(FlatFileList[i].RARCFileType.ToString()+ "\"").PadRight(12, ' ')}\t FileOrDirectory=\"{FlatFileList[i].ModularA.ToString("X").PadLeft(8, '0')}\"\t Size=\"{FlatFileList[i].ModularB.ToString("X").PadLeft(8, '0')}\" />\n";

            //System.IO.File.WriteAllText("Original.xml", XML);
#endif


            List<Directory> Directories = new List<Directory>();
            for (int i = 0; i < FlatDirectoryList.Count; i++)
            {
                Directories.Add(new Directory(this, i, FlatDirectoryList, FlatFileList, DataOffset, RARCFile));
            }

            for (int i = 0; i < Directories.Count; i++)
            {
                List<KeyValuePair<string, object>> templist = new List<KeyValuePair<string, object>>();
                foreach (KeyValuePair<string, object> DirectoryItem in Directories[i].Items)
                {
                    if (DirectoryItem.Value is RARCFileEntry fe)
                    {
                        if (DirectoryItem.Key.Equals("."))
                        {
                            if (fe.ModularA == 0)
                                Root = Directories[fe.ModularA];
                            continue;
                        }
                        if (DirectoryItem.Key.Equals(".."))
                        {
                            if (fe.ModularA == -1 || fe.ModularA > Directories.Count)
                                continue;
                            Directories[i].Parent = Directories[fe.ModularA];
                            continue;
                        }
                        if (!Directories[fe.ModularA].Name.Equals(DirectoryItem.Key))
                            Directories[fe.ModularA].Name = DirectoryItem.Key;
                        templist.Add(new KeyValuePair<string, object>(DirectoryItem.Key, Directories[fe.ModularA]));
                    }
                    else
                    {
                        ((File)DirectoryItem.Value).Parent = Directories[i];
                        templist.Add(DirectoryItem);
                    }
                }
                Directories[i].Items = templist.ToDictionary(K => K.Key, V => V.Value);
            }
            #endregion
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="RARCFile"></param>
        protected override void Write(Stream RARCFile)
        {
            Dictionary<ArchiveFile, uint> FileOffsets = new Dictionary<ArchiveFile, uint>();
            uint dataoffset = 0;
            uint MRAMSize = 0, ARAMSize = 0, DVDSize = 0;
            byte[] DataByteBuffer = GetDataBytes(Root, ref FileOffsets, ref dataoffset, ref MRAMSize, ref ARAMSize, ref DVDSize).ToArray();
            short FileID = 0;
            int NextFolderID = 1;
            List<RARCFileEntry> FlatFileList = GetFlatFileList(Root, FileOffsets, ref FileID, 0, ref NextFolderID, -1);
            uint FirstFileOffset = 0;
            List<RARCDirEntry> FlatDirectoryList = GetFlatDirectoryList(Root, ref FirstFileOffset);
            FlatDirectoryList.Insert(0, new RARCDirEntry() { FileCount = (ushort)(Root.Items.Count + 2), FirstFileOffset = 0, Name = Root.Name, NameHash = StringToHash(Root.Name), NameOffset = 0, Type = "ROOT" });
            Dictionary<string, uint> StringLocations = new Dictionary<string, uint>();
            byte[] StringDataBuffer = GetStringTableBytes(FlatFileList, Root.Name, ref StringLocations).ToArray();

            #region File Writing
            RARCFile.WriteString(Magic);
            RARCFile.Write(new byte[16] { 0xDD, 0xDD, 0xDD, 0xDD, 0x00, 0x00, 0x00, 0x20, 0xDD, 0xDD, 0xDD, 0xDD, 0xEE, 0xEE, 0xEE, 0xEE }, 0, 16);
            RARCFile.WriteBigEndian(BitConverter.GetBytes(MRAMSize), 4);
            RARCFile.WriteBigEndian(BitConverter.GetBytes(ARAMSize), 4);
            RARCFile.WriteBigEndian(BitConverter.GetBytes(DVDSize), 4);
            //Data Header
            RARCFile.WriteBigEndian(BitConverter.GetBytes(FlatDirectoryList.Count), 4);
            RARCFile.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); //Directory Nodes Location (-0x20)
            RARCFile.WriteBigEndian(BitConverter.GetBytes(FlatFileList.Count), 4);
            RARCFile.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); //File Entries Location (-0x20)
            RARCFile.Write(new byte[4] { 0xEE, 0xEE, 0xEE, 0xEE }, 0, 4); //String Table Size
            RARCFile.Write(new byte[4] { 0xEE, 0xEE, 0xEE, 0xEE }, 0, 4); //string Table Location (-0x20)
            RARCFile.WriteBigEndian(BitConverter.GetBytes((ushort)FlatFileList.Count), 2);
            RARCFile.WriteByte((byte)(KeepFileIDsSynced ? 0x01 : 0x00));
            RARCFile.Write(new byte[5], 0, 5);
            long DirectoryEntryOffset = RARCFile.Position;

            #region Directory Nodes
            for (int i = 0; i < FlatDirectoryList.Count; i++)
            {
                RARCFile.WriteString(FlatDirectoryList[i].Type);
                RARCFile.WriteBigEndian(BitConverter.GetBytes(StringLocations[FlatDirectoryList[i].Name]), 4);
                RARCFile.WriteBigEndian(BitConverter.GetBytes(FlatDirectoryList[i].NameHash), 2);
                RARCFile.WriteBigEndian(BitConverter.GetBytes(FlatDirectoryList[i].FileCount), 2);
                RARCFile.WriteBigEndian(BitConverter.GetBytes(FlatDirectoryList[i].FirstFileOffset), 4);
            }

            #region Padding
            while (RARCFile.Position % 32 != 0)
                RARCFile.WriteByte(0x00);
            #endregion
            #endregion

            long FileEntryOffset = RARCFile.Position;

            #region File Entries
            for (int i = 0; i < FlatFileList.Count; i++)
            {
                RARCFile.WriteBigEndian(BitConverter.GetBytes(FlatFileList[i].FileID), 2);
                RARCFile.WriteBigEndian(BitConverter.GetBytes(StringToHash(FlatFileList[i].Name)), 2);
                RARCFile.WriteBigEndian(BitConverter.GetBytes(FlatFileList[i].Type), 2);
                RARCFile.WriteBigEndian(BitConverter.GetBytes((ushort)StringLocations[FlatFileList[i].Name]), 2);
                RARCFile.WriteBigEndian(BitConverter.GetBytes(FlatFileList[i].ModularA), 4);
                RARCFile.WriteBigEndian(BitConverter.GetBytes(FlatFileList[i].ModularB), 4);
                RARCFile.Write(new byte[4], 0, 4);
            }
            #region Padding
            while (RARCFile.Position % 32 != 0)
                RARCFile.WriteByte(0x00);
            #endregion
            #endregion

            long StringTableOffset = RARCFile.Position;

            #region String Table
            RARCFile.Write(StringDataBuffer, 0, StringDataBuffer.Length);

            #region Padding
            while (RARCFile.Position % 32 != 0)
                RARCFile.WriteByte(0x00);
            #endregion
            #endregion

            long FileTableOffset = RARCFile.Position;

            #region File Table
            RARCFile.Write(DataByteBuffer, 0, DataByteBuffer.Length);
            #endregion

            #region Header
            RARCFile.Position = 0x04;
            RARCFile.WriteBigEndian(BitConverter.GetBytes((int)RARCFile.Length), 4);
            RARCFile.Position += 0x04;
            RARCFile.WriteBigEndian(BitConverter.GetBytes((int)(FileTableOffset - 0x20)), 4);
            RARCFile.WriteBigEndian(BitConverter.GetBytes((int)(RARCFile.Length - FileTableOffset)), 4);
            RARCFile.Position += 0x10;
            RARCFile.WriteBigEndian(BitConverter.GetBytes((int)(DirectoryEntryOffset - 0x20)), 4);
            RARCFile.Position += 0x04;
            RARCFile.WriteBigEndian(BitConverter.GetBytes((int)(FileEntryOffset - 0x20)), 4);
            RARCFile.WriteBigEndian(BitConverter.GetBytes((int)(FileTableOffset - StringTableOffset)), 4);
            RARCFile.WriteBigEndian(BitConverter.GetBytes((int)(StringTableOffset - 0x20)), 4);
            #endregion

            #endregion
        }
        private List<byte> GetDataBytes(ArchiveDirectory Root, ref Dictionary<ArchiveFile, uint> Offsets, ref uint LocalOffset, ref uint MRAMSize, ref uint ARAMSize, ref uint DVDSize)
        {
            List<byte> DataBytesMRAM = new List<byte>();
            List<byte> DataBytesARAM = new List<byte>();
            List<byte> DataBytesDVD = new List<byte>();
            //First, we must sort the files in the correct order
            //MRAM First. ARAM Second, DVD Last
            List<ArchiveFile> MRAM = new List<ArchiveFile>(), ARAM = new List<ArchiveFile>(), DVD = new List<ArchiveFile>();
            SortFilesByLoadType(Root, ref MRAM, ref ARAM, ref DVD);

            for (int i = 0; i < MRAM.Count; i++)
            {

                if (Offsets.Any(OFF => OFF.Key.FileData.SequenceEqual(MRAM[i].FileData)))
                {
                    Offsets.Add(MRAM[i], Offsets[Offsets.Keys.First(FILE => FILE.FileData.SequenceEqual(MRAM[i].FileData))]);
                }
                else
                {
                    List<byte> CurrentMRAMFile = MRAM[i].FileData.ToList();
                    while (CurrentMRAMFile.Count % 32 != 0)
                        CurrentMRAMFile.Add(0x00);
                    Offsets.Add(MRAM[i], LocalOffset);
                    DataBytesMRAM.AddRange(CurrentMRAMFile);
                    LocalOffset += (uint)CurrentMRAMFile.Count;
                }
            }
            MRAMSize = LocalOffset;
            for (int i = 0; i < ARAM.Count; i++)
            {
                Offsets.Add(ARAM[i], LocalOffset);
                List<byte> temp = new List<byte>();
                temp.AddRange(ARAM[i].FileData);

                while (temp.Count % 32 != 0)
                    temp.Add(0x00);
                DataBytesARAM.AddRange(temp);
                LocalOffset += (uint)temp.Count;
            }
            ARAMSize = LocalOffset - MRAMSize;
            for (int i = 0; i < DVD.Count; i++)
            {
                Offsets.Add(DVD[i], LocalOffset);
                List<byte> temp = new List<byte>();
                temp.AddRange(DVD[i].FileData);

                while (temp.Count % 32 != 0)
                    temp.Add(0x00);
                DataBytesDVD.AddRange(temp);
                LocalOffset += (uint)temp.Count;
            }
            DVDSize = LocalOffset - ARAMSize - MRAMSize;

            List<byte> DataBytes = new List<byte>();
            DataBytes.AddRange(DataBytesMRAM);
            DataBytes.AddRange(DataBytesARAM);
            DataBytes.AddRange(DataBytesDVD);
            return DataBytes;
        }
        private void SortFilesByLoadType(ArchiveDirectory Root, ref List<ArchiveFile> MRAM, ref List<ArchiveFile> ARAM, ref List<ArchiveFile> DVD)
        {
            foreach (KeyValuePair<string, object> item in Root.Items)
            {
                if (item.Value is Directory dir)
                {
                    SortFilesByLoadType(dir, ref MRAM, ref ARAM, ref DVD);
                }
                else if (item.Value is File file)
                {
                    if (file.FileSettings.HasFlag(FileAttribute.PRELOAD_TO_MRAM))
                    {
                        MRAM.Add(file);
                    }
                    else if (file.FileSettings.HasFlag(FileAttribute.PRELOAD_TO_ARAM))
                    {
                        ARAM.Add(file);
                    }
                    else if (file.FileSettings.HasFlag(FileAttribute.LOAD_FROM_DVD))
                    {
                        DVD.Add(file);
                    }
                    else
                        throw new Exception($"File entry \"{file.ToString()}\" is not set as being loaded into any type of RAM, or from DVD.");
                }
            }
        }
        private List<RARCFileEntry> GetFlatFileList(ArchiveDirectory Root, Dictionary<ArchiveFile, uint> FileOffsets, ref short GlobalFileID, int CurrentFolderID, ref int NextFolderID, int BackwardsFolderID)
        {
            List<RARCFileEntry> FileList = new List<RARCFileEntry>();
            List<KeyValuePair<int, Directory>> Directories = new List<KeyValuePair<int, Directory>>();
            foreach (KeyValuePair<string, object> item in Root.Items)
            {
                if (item.Value is File file)
                {
                    FileList.Add(new RARCFileEntry() { FileID = KeepFileIDsSynced ? GlobalFileID++ : file.ID, Name = file.Name, ModularA = (int)FileOffsets[file], ModularB = file.FileData.Length, Type = (short)((ushort)file.FileSettings << 8) });
                }
                else if (item.Value is Directory Currentdir)
                {
                    Directories.Add(new KeyValuePair<int, Directory>(FileList.Count, Currentdir));
                    //Dirs.Add(new RARCDirEntry() { FileCount = (ushort)(Currentdir.Items.Count + 2), FirstFileOffset = 0xFFFFFFFF, Name = Currentdir.Name, NameHash = Currentdir.NameToHash(), NameOffset = 0xFFFFFFFF, Type = Currentdir.ToTypeString() });
                    FileList.Add(new RARCFileEntry() { FileID = -1, Name = Currentdir.Name, ModularA = NextFolderID++, ModularB = 0x10, Type = 0x0200 });
                    GlobalFileID++;
                }
            }
            FileList.Add(new RARCFileEntry() { FileID = -1, Name = ".", ModularA = CurrentFolderID, ModularB = 0x10, Type = 0x0200 });
            FileList.Add(new RARCFileEntry() { FileID = -1, Name = "..", ModularA = BackwardsFolderID, ModularB = 0x10, Type = 0x0200 });
            GlobalFileID += 2;
            for (int i = 0; i < Directories.Count; i++)
            {
                FileList.AddRange(GetFlatFileList(Directories[i].Value, FileOffsets, ref GlobalFileID, FileList[Directories[i].Key].ModularA, ref NextFolderID, CurrentFolderID));
            }
            return FileList;
        }
        private List<ArchiveFile> GetFlatFileList(ArchiveDirectory Root)
        {
            List<ArchiveFile> FileList = new List<ArchiveFile>();
            foreach (KeyValuePair<string, object> item in Root.Items)
            {
                if (item.Value is ArchiveFile file)
                {
                    FileList.Add(file);
                }
                else if (item.Value is ArchiveDirectory Currentdir)
                {
                    FileList.AddRange(GetFlatFileList(Currentdir));
                    FileList.Add(null);
                }
            }
            return FileList;
        }
        private List<RARCDirEntry> GetFlatDirectoryList(ArchiveDirectory Root, ref uint FirstFileOffset)
        {
            List<RARCDirEntry> FlatDirectoryList = new List<RARCDirEntry>();
            List<RARCDirEntry> TemporaryList = new List<RARCDirEntry>();
            FirstFileOffset += (uint)(Root.Items.Count + 2);
            foreach (KeyValuePair<string, object> item in Root.Items)
            {
                if (item.Value is Directory Currentdir)
                {
                    FlatDirectoryList.Add(new RARCDirEntry() { FileCount = (ushort)(Currentdir.Items.Count + 2), FirstFileOffset = FirstFileOffset, Name = Currentdir.Name, NameHash = StringToHash(Currentdir.Name), NameOffset = 0xFFFFFFFF, Type = Currentdir.ToTypeString() });
                    TemporaryList.AddRange(GetFlatDirectoryList(Currentdir, ref FirstFileOffset));
                }
            }
            FlatDirectoryList.AddRange(TemporaryList);
            return FlatDirectoryList;
        }
        private List<byte> GetStringTableBytes(List<RARCFileEntry> FlatFileList, string RootName, ref Dictionary<string, uint> Offsets)
        {
            List<byte> strings = new List<byte>();
            Encoding enc = Encoding.GetEncoding(932);
            strings.AddRange(enc.GetBytes(RootName));
            strings.Add(0x00);
            Offsets.Add(RootName, 0);

            Offsets.Add(".", (uint)strings.Count);
            strings.AddRange(enc.GetBytes("."));
            strings.Add(0x00);

            Offsets.Add("..", (uint)strings.Count);
            strings.AddRange(enc.GetBytes(".."));
            strings.Add(0x00);

            for (int i = 0; i < FlatFileList.Count; i++)
            {
                if (!Offsets.ContainsKey(FlatFileList[i].Name))
                {
                    Offsets.Add(FlatFileList[i].Name, (uint)strings.Count);
                    strings.AddRange(enc.GetBytes(FlatFileList[i].Name));
                    strings.Add(0x00);
                }
            }
            return strings;
        }

        private short GetNextFreeID()
        {
            List<short> AllIDs = new List<short>();
            List<ArchiveFile> FlatFileList = GetFlatFileList(Root);
            for (int i = 0; i < FlatFileList.Count; i++)
                AllIDs.Add(((File)FlatFileList[i])?.ID ?? (short)AllIDs.Count);
            if (AllIDs.Count == 0)
                return 0;
            int a = AllIDs.OrderBy(x => x).First();
            int b = AllIDs.OrderBy(x => x).Last();
            List<int> LiterallyAllIDs = Enumerable.Range(0, b - a + 1).ToList();
            List<short> Shorts = new List<short>();
            for (int i = 0; i < LiterallyAllIDs.Count; i++)
            {
                Shorts.Add((short)LiterallyAllIDs[i]);
            }

            List<short> Remaining = Shorts.Except(AllIDs).ToList();
            if (Remaining.Count == 0)
                return (short)AllIDs.Count;
            else
                return Remaining.First();
        }
        #endregion

        /// <summary>
        /// File Attibutes for <see cref="File"/>
        /// </summary>
        [Flags]
        public enum FileAttribute
        {
            /// <summary>
            /// Indicates this is a File
            /// </summary>
            FILE = 0x01,
            /// <summary>
            /// Directory. Not allowed to be used for <see cref="File"/>s, only here for reference
            /// </summary>
            DIRECTORY = 0x02,
            /// <summary>
            /// Indicates that this file is compressed
            /// </summary>
            COMPRESSED = 0x04,
            /// <summary>
            /// Indicates that this file gets Pre-loaded into Main RAM
            /// </summary>
            PRELOAD_TO_MRAM = 0x10,
            /// <summary>
            /// Indicates that this file gets Pre-loaded into Auxiliary RAM
            /// </summary>
            PRELOAD_TO_ARAM = 0x20,
            /// <summary>
            /// Indicates that this file does not get pre-loaded, but rather read from the DVD
            /// </summary>
            LOAD_FROM_DVD = 0x40,
            /// <summary>
            /// Indicates that this file is YAZ0 Compressed
            /// </summary>
            YAZ0_COMPRESSED = 0x80
        }
    }
}
