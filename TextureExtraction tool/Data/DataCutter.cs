using AuroraLib.Archives;
using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Text;
using AuroraLib.Texture;
using AuroraLib.Texture.Formats;

namespace DolphinTextureExtraction
{
    internal class DataCutter : ArchiveNode
    {
        protected int maxErr = 15;

        private static readonly List<byte[]> Pattern = new List<byte[]>();

        public override bool CanWrite => throw new NotImplementedException();

        static DataCutter()
        {
            foreach (FormatInfo item in FormatDictionary.Master)
            {
                if (item.Class != null && item.Identifier != null && item.Identifier.AsSpan().Length >= 4)
                {
                    Pattern.Add(item.Identifier.AsSpan().ToArray());
                }
            }
        }

        public DataCutter() : base(nameof(DataCutter)) { }

        public DataCutter(string name) : base(name) { }

        public DataCutter(Stream stream, string name = nameof(DataCutter)) : base(name)
            => BinaryDeserialize(stream);

        public DataCutter(Stream stream, IEnumerable<byte[]> pattern) : this()
            => Read(stream, pattern);

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default) => false;

        protected override void Deserialize(Stream source)
        => Read(source, Pattern);

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        protected void Read(Stream stream, IEnumerable<byte[]> pattern)
        {
            int err = 0;

            while (stream.Search(pattern, out byte[] match))
            {
                FormatDictionary.TryGetValue(match, out FormatInfo format);
                if (format == null) format = new FormatInfo(".bin", 0, match, FormatType.Unknown);
                long entrystart = stream.Position - format.IdentifierOffset;

                // if Compresst?
                if (Count <= 1 && stream.Position > 6 + format.IdentifierOffset)
                {
                    stream.Seek(-5 - format.IdentifierOffset, SeekOrigin.Current);
                    switch (stream.ReadByte())
                    {
                        case 16: //LZ77
                                 //case 17: //LZ11
                            stream.Seek(-1, SeekOrigin.Current);
                            FileNode ComSub = new($"entrys.lz", new SubStream(stream, stream.Length - stream.Position, stream.Position));
                            Add(ComSub);
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
                    stream.Seek(entrystart + format.IdentifierOffset + 1, SeekOrigin.Begin);
                    err++;
                    continue;
                }

                switch (EncodingX.GetValidString(match))
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
                        int ImageOffsetTableOffset = stream.ReadInt32(Endian.Big);
                        stream.Seek(entrystart + ImageOffsetTableOffset, SeekOrigin.Begin);
                        TPL.ImageOffsetEntry[] ImageOffsetTable = stream.For((int)TotalImageCount, s => s.Read<TPL.ImageOffsetEntry>(Endian.Big));
                        var ImageOffset = ImageOffsetTable.Last();
                        stream.Seek(entrystart + ImageOffset.ImageHeaderOffset, SeekOrigin.Begin);
                        TPL.ImageHeader imageHeader = stream.Read<TPL.ImageHeader>(Endian.Big);
                        stream.Seek(entrystart + imageHeader.ImageDataAddress + imageHeader.Format.GetCalculatedTotalDataSize(imageHeader.Width, imageHeader.Height, imageHeader.MaxLOD), SeekOrigin.Begin);
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
                    stream.Seek(entrystart + format.IdentifierOffset + 1, SeekOrigin.Begin);
                    err++;
                    continue;
                }
                err--;

                FileNode Sub = new($"entry_{Count + 1}.{format.Extension}", new SubStream(stream, TotalSize, entrystart));
                Add(Sub);

                stream.Position = entrystart + TotalSize;
            }

            if (err > maxErr)
                throw new Exception($"maximum error tolerance of {maxErr} exceeded.");

            if (Count > 1)
                Events.NotificationEvent?.Invoke(NotificationType.Info, $"{nameof(DataCutter)} has seperated {Count} files.");
        }
    }
}
