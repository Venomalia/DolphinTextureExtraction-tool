using AuroraLip.Archives;
using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace DolphinTextureExtraction_tool
{
    internal class DataCutter : Archive
    {
        protected int maxErr = 15;

        private static readonly List<byte[]> Pattern = new List<byte[]>();

        static DataCutter()
        {
            foreach (var item in FormatDictionary.Header)
            {
                if (item.Value.Class != null && item.Value.Header.Bytes.Length >= 3)
                {
                    Pattern.Add(item.Value.Header.Bytes);
                }
            }
        }

        public DataCutter() { }

        public DataCutter(string filename) : base(filename) { }

        public DataCutter(Stream stream, string filename = null) : base(stream, filename) { }

        public DataCutter(Stream stream, IEnumerable<byte[]> pattern, string filename = null) : base()
        {
            FullPath = filename;
            Read(stream, pattern);
        }

        protected override void Read(Stream stream)
            => Read(stream, Pattern);

        protected void Read(Stream stream, IEnumerable<byte[]> pattern)
        {
            Root = new ArchiveDirectory() { OwnerArchive = this };
            int err = 0;

            while (stream.Search(pattern, out byte[] match))
            {
                FormatDictionary.Header.TryGetValue(match.ToValidString(), out FormatInfo format);
                if (format == null) format = new FormatInfo(".bin", match, 0, FormatType.Unknown);
                long entrystart = stream.Position - format.Header.Offset;

                // if Compresst?
                if (Root.Count <= 1 && stream.Position > 6 + format.Header.Offset)
                {
                    stream.Seek(-5-format.Header.Offset, SeekOrigin.Current);
                    switch (stream.ReadByte())
                    {
                        case 16: //LZ77
                        //case 17: //LZ11
                            stream.Seek(-1, SeekOrigin.Current);
                            ArchiveFile ComSub = new ArchiveFile
                            {
                                Parent = Root,
                                Name = $"entrys.lz",
                                FileData = new SubStream(stream,stream.Length - stream.Position, stream.Position)
                            };
                            Root.Items.Add(ComSub.Name, ComSub);
                            return;
                    }
                }
                stream.Seek(entrystart, SeekOrigin.Begin);


                if (err > maxErr)
                    break;

                uint TotalSize = 0;
                stream.Seek(entrystart, SeekOrigin.Begin);
                if (!format.IsMatch(stream))
                {
                    stream.Seek(entrystart + format.Header.Offset + 1, SeekOrigin.Begin);
                    err++;
                    continue;
                }

                switch (match.ToValidString())
                {
                    case "CLZ":
                    case "pack":
                        continue;
                    case "AFS":
                        uint magic = stream.ReadUInt32();
                        if (magic == 0x00534641 || magic == 0x20534641)
                            goto default;

                        continue;
                    case "GVRT":
                        TotalSize = stream.ReadUInt32() + 8;
                        break;
                    case "RSTM":
                    case "REFT":
                    case "bres":
                        uint ByteOrder = BitConverter.ToUInt16(stream.Read(2), 0); //65534 BigEndian
                        ushort Version = stream.ReadUInt16(Endian.Big);
                        TotalSize = stream.ReadUInt32(Endian.Big);
                        if (ByteOrder != 65534)
                        {
                            err++;
                            continue;
                        }
                        break;
                    case "RARC":
                        TotalSize = stream.ReadUInt32(Endian.Big);
                        break;
                    case "PLT0":
                    case "MDL0":
                    case "TEX0":
                        TotalSize = stream.ReadUInt32(Endian.Big);
                        if (TotalSize > stream.Length - entrystart)
                        {
                            err++;
                            continue;
                        }
                        break;
                    case " ¯0": //tpl
                        uint TotalImageCount = stream.ReadUInt32(Endian.Big);
                        if (TotalImageCount < 1 || TotalImageCount > 1024)
                        {
                            err++;
                            continue;
                        }
                        goto default;
                    case "RTDP":
                        int EOH = stream.ReadInt32(Endian.Big);
                        int NrEntries = (int)stream.ReadUInt32(Endian.Big);
                        TotalSize = stream.ReadUInt32(Endian.Big);
                        if (NrEntries < 1 || NrEntries > 256)
                        {
                            err++;
                            continue;
                        }
                        break;
                    default:
                        stream.Search(pattern, out _);
                        TotalSize = (uint)(stream.Position - entrystart);
                        break;
                }

                if (TotalSize > stream.Length - entrystart)
                {
                    stream.Seek(entrystart + format.Header.Offset + 1, SeekOrigin.Begin);
                    err++;
                    continue;
                }
                err--;

                ArchiveFile Sub = new ArchiveFile
                {
                    Parent = Root,
                    Name = $"entry_{TotalFileCount + 1}.{format.Extension}",
                    FileData = new SubStream(stream, TotalSize, entrystart)
                };
                Root.Items.Add(Sub.Name, Sub);

                stream.Position = entrystart + TotalSize;
            }

            if (err > maxErr)
                throw new Exception($"maximum error tolerance of {maxErr} exceeded.");

            if (Root.Count > 1)
                Events.NotificationEvent?.Invoke(NotificationType.Info, $"{nameof(DataCutter)} has seperated {this.Root.Count} files.");
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }
    }
}
