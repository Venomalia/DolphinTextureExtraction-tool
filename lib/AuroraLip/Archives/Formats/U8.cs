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
    /// Based on https://wiki.tockdom.com/wiki/U8_(File_Format)
    /// </summary>
    public class U8 : Archive, IFileFormat, IMagicIdentify
    {
        public FileType FileType => FileType.Archive;

        public string Description => description;

        private const string description = "U8 Archive";

        public string Extension => ".arc";

        public string Magic => magic;

        private const string magic = "Uª8-";

        public bool CanRead => true;

        public bool CanWrite => true;

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        /// <summary>
        /// Create an empty U8 archive
        /// </summary>
        public U8() { }

        /// <summary>
        /// Open a U8 from a file
        /// </summary>
        /// <param name="filename"></param>
        public U8(string filename)
        {
            FileStream BTPFile = new FileStream(filename, FileMode.Open);
            Read(BTPFile);
            BTPFile.Close();
            FileName = filename;
        }
        /// <summary>
        /// Open a U8 from a stream
        /// </summary>
        /// <param name="BTPFile"></param>
        ///<param name="filename"></param>
        public U8(Stream BTPFile, string filename = null)
        {
            Read(BTPFile);
            FileName = filename;
        }

        protected override void Read(Stream U8File)
        {
            if (!IsMatch(U8File))
                throw new Exception($"Invalid Magic. Expected \"{Magic}\"");

            uint OffsetToNodeSection = BitConverter.ToUInt32(U8File.ReadBigEndian(0, 4), 0); //usually 0x20
            uint NodeSectionSize = BitConverter.ToUInt32(U8File.ReadBigEndian(0, 4), 0);
            uint FileDataOffset = BitConverter.ToUInt32(U8File.ReadBigEndian(0, 4), 0);
            U8File.Position += 0x10; //Skip 16 reserved bytes. All are 0xCC

            //Node time
            //Each node is 0x0C bytes each
            //The first node is node 0

            //Node format:
            //0x00 = byte = IsDirectory
            //0x01 = Int24..... oh no...
            //0x04 = File: Offset to data start | Directory: Index of Parent Directory
            //0x08 = File: Size of the File | Directory: Index of the directory's first node?

            //Root has total number of nodes
            U8File.Position = OffsetToNodeSection;
            U8Node RootNode = new U8Node(U8File);

            //Root has total number of nodes 
            int TotalNodeCount = RootNode.Size;
            long StringTableLocation = OffsetToNodeSection + (TotalNodeCount * 0x0C);

            //Read all our entries
            List<U8Node> entries = new List<U8Node>
            {
                RootNode
            };
            List<object> FlatItems = new List<object>();
            //entries.Add(RootNode);
            for (int i = 0; i < TotalNodeCount; i++)
            {
                var node = new U8Node(U8File);
                entries.Add(node);
                long PausePosition = U8File.Position;
                if (entries[i].IsDirectory)
                {
                    ArchiveDirectory dir = new ArchiveDirectory();
                    U8File.Position = StringTableLocation + entries[i].NameOffset;
                    dir.Name = U8File.ReadString(); // x => x != 0
                    FlatItems.Add(dir);
                    dir.OwnerArchive = this;
                }
                else
                {
                    ArchiveFile file = new ArchiveFile();
                    U8File.Position = StringTableLocation + entries[i].NameOffset;
                    file.Name = U8File.ReadString();
                    U8File.Position = entries[i].DataOffset;
                    file.FileData = U8File.Read(0, entries[i].Size);
                    FlatItems.Add(file);
                }
                U8File.Position = PausePosition;
            }
            entries.RemoveAt(entries.Count - 1);
            Stack<ArchiveDirectory> DirectoryStack = new Stack<ArchiveDirectory>();
            DirectoryStack.Push((ArchiveDirectory)FlatItems[0]);
            for (int i = 1; i < entries.Count; i++)
            {
                if (entries[i].IsDirectory)
                {
                    int parent = entries[i].DataOffset;
                    int EntryCount = entries[i].Size;

                    if (FlatItems[parent] is ArchiveDirectory dir)
                    {
                        ArchiveDirectory curdir = (ArchiveDirectory)FlatItems[i];
                        dir.Items.Add(curdir.Name, curdir);
                        curdir.Parent = dir;
                    }
                    DirectoryStack.Push((ArchiveDirectory)FlatItems[i]);
                }
                else
                {
                    //if there is already an ellement with the names.
                    if (DirectoryStack.Peek().Items.ContainsKey(((dynamic)FlatItems[i]).Name))
                    {
                        string name = ((ArchiveFile)FlatItems[i]).Name;
                        ((ArchiveFile)FlatItems[i]).Name = Path.GetFileName(name) + i + Path.GetExtension(name);
                    }

                    DirectoryStack.Peek().Items.Add(((dynamic)FlatItems[i]).Name, FlatItems[i]);
                }
                if (i == entries[FlatItems.IndexOf(DirectoryStack.Peek())].Size - 1)
                {
                    DirectoryStack.Pop();
                }
            }
            Root = (ArchiveDirectory)FlatItems[0];
        }

        protected override void Write(Stream U8File)
        {
            List<dynamic> FlatItems = new List<dynamic>();

            AddItems(Root);
            //The archive has been flattened hooray
            Dictionary<string, uint> StringOffsets = new Dictionary<string, uint>();
            List<byte> StringBytes = GetStringTableBytes(FlatItems, ref StringOffsets);

            uint DataOffset = (uint)(0x20 + (FlatItems.Count * 0x0C) + StringBytes.Count);
            DataOffset += 0x20 - (DataOffset % 0x20);
            //while (DataOffset % 16 != 0)
            //    DataOffset++;
            Dictionary<ArchiveFile, uint> DataOffsets = new Dictionary<ArchiveFile, uint>();
            List<byte> DataBytes = GetDataBytes(FlatItems, DataOffset, ref DataOffsets);

            List<U8Node> Nodes = new List<U8Node>();
            Stack<ArchiveDirectory> DirectoryStack = new Stack<ArchiveDirectory>();
            for (int i = 0; i < FlatItems.Count; i++)
            {
                U8Node newnode = new U8Node() { NameOffset = new Int24((int)StringOffsets[FlatItems[i].Name]) };
                if (FlatItems[i] is ArchiveDirectory dir)
                {
                    if (DirectoryStack.Count > 1)
                        while (!object.ReferenceEquals(DirectoryStack.Peek(), dir.Parent))
                        {
                            Nodes[FlatItems.IndexOf(DirectoryStack.Peek())].Size = i;
                            DirectoryStack.Pop();
                        }
                    newnode.IsDirectory = true;
                    if (i != 0) //Node is not the Root
                    {
                        newnode.DataOffset = FlatItems.IndexOf(dir.Parent);
                    }
                    else
                    {
                        newnode.Size = FlatItems.Count;
                    }
                    DirectoryStack.Push(dir);
                }
                else
                {
                    newnode.DataOffset = (int)DataOffsets[(ArchiveFile)FlatItems[i]];
                    newnode.Size = ((ArchiveFile)FlatItems[i]).FileData.Length;
                }
                Nodes.Add(newnode);
                if (DirectoryStack.Peek().Items.Count == 0 || object.ReferenceEquals(FlatItems[i], DirectoryStack.Peek().Items.Last().Value))
                {
                    int index = FlatItems.IndexOf(DirectoryStack.Peek());
                    Nodes[index].Size = i + 1;
                    DirectoryStack.Pop();
                }
            }

            while (DirectoryStack.Count > 0)
            {
                Nodes[FlatItems.IndexOf(DirectoryStack.Peek())].Size = Nodes.Count;
                DirectoryStack.Pop();
            }

            //Write the Header
            U8File.WriteString(Magic);
            U8File.WriteBigEndian(BitConverter.GetBytes(0x20), 0, 4);
            U8File.WriteBigEndian(BitConverter.GetBytes(Nodes.Count * 0x0C + StringBytes.Count), 0, 4);
            U8File.WriteBigEndian(BitConverter.GetBytes(DataOffset), 0, 4);
            U8File.WriteBigEndian(new byte[16] { 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC }, 0, 16);

            //Write the Nodes
            for (int i = 0; i < Nodes.Count; i++)
                Nodes[i].Write(U8File);

            //Write the strings
            U8File.Write(StringBytes.ToArray(), 0, StringBytes.Count);
            U8File.PadTo(0x20, 0);

            //Write the File Data
            U8File.Write(DataBytes.ToArray(), 0, DataBytes.Count);

            void AddItems(ArchiveDirectory dir)
            {
                FlatItems.Add(dir);
                List<ArchiveDirectory> subdirs = new List<ArchiveDirectory>();
                foreach (var item in dir.Items)
                {
                    if (item.Value is ArchiveDirectory d)
                        subdirs.Add(d);
                    else
                        FlatItems.Add(item.Value);
                }
                for (int i = 0; i < subdirs.Count; i++)
                    AddItems(subdirs[i]);
            }
        }

        #region Internals
        /// <summary>
        /// Only used when Reading / Writing
        /// </summary>
        internal class U8Node
        {
            public bool IsDirectory;
            public Int24 NameOffset;
            public int DataOffset;
            public int Size;

            public U8Node() { }

            public U8Node(Stream U8File)
            {
                IsDirectory = U8File.ReadByte() == 1;
                NameOffset = BitConverterEx.ToInt24(U8File.ReadBigEndian(0, 3), 0);
                DataOffset = BitConverter.ToInt32(U8File.ReadBigEndian(0, 4), 0);
                Size = BitConverter.ToInt32(U8File.ReadBigEndian(0, 4), 0);
            }

            public void Write(Stream U8File)
            {
                U8File.WriteByte((byte)(IsDirectory ? 0x01 : 0x00));
                U8File.WriteBigEndian(BitConverterEx.GetBytes(NameOffset), 0, 3);
                U8File.WriteBigEndian(BitConverter.GetBytes(DataOffset), 0, 4);
                U8File.WriteBigEndian(BitConverter.GetBytes(Size), 0, 4);
            }

            public override string ToString() => $"{(IsDirectory ? "Directory" : "File")}: {NameOffset.ToString()} | {DataOffset.ToString()} | {Size.ToString()}";
        }

        private List<byte> GetDataBytes(List<dynamic> FlatFileList, uint DataStart, ref Dictionary<ArchiveFile, uint> Offsets)
        {
            List<byte> FileBytes = new List<byte>();
            for (int i = 0; i < FlatFileList.Count; i++)
            {
                if (!(FlatFileList[i] is ArchiveFile file))
                    continue;

                if (Offsets.Any(OFF => OFF.Key.FileData.SequenceEqual(file.FileData)))
                {
                    Offsets.Add(file, Offsets[Offsets.Keys.First(FILE => FILE.FileData.SequenceEqual(file.FileData))]);
                }
                else
                {
                    List<byte> CurrentMRAMFile = file.FileData.ToList();
                    while (CurrentMRAMFile.Count % 32 != 0)
                        CurrentMRAMFile.Add(0x00);
                    Offsets.Add(file, (uint)FileBytes.Count + DataStart);
                    FileBytes.AddRange(CurrentMRAMFile);
                }
            }
            return FileBytes;
        }
        private List<byte> GetStringTableBytes(List<dynamic> FlatFileList, ref Dictionary<string, uint> Offsets)
        {
            List<byte> strings = new List<byte>();
            Encoding enc = Encoding.GetEncoding(932);

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
        #endregion
    }
}
