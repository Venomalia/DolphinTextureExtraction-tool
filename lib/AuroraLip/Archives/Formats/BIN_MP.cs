using AuroraLip.Common;
using AuroraLip.Compression;
using AuroraLip.Compression.Formats;
using System;
using System.IO;

namespace AuroraLip.Archives.Formats
{
    // base on https://github.com/gamemasterplc/mpbintools/blob/master/bindump.c
    public class BIN_MP : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public static string Extension => ".bin";

        private static readonly LZSS lZSS = new LZSS(10, 6, 2);

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
        {
            if (!extension.ToLower().Equals(Extension))
                return false;

            uint files = stream.ReadUInt32(Endian.Big);
            if (files > 1000)
                return false;
            uint[] offsets = new uint[files];
            for (int i = 0; i < files; i++)
            {
                offsets[i] = stream.ReadUInt32(Endian.Big);
            }

            uint lastoffset = 0;
            for (int i = 0; i < files; i++)
            {
                if (offsets[i] < lastoffset || stream.Length < offsets[i] + 10)
                    return false;
                lastoffset = offsets[i];

                stream.Seek(offsets[i] + 4, SeekOrigin.Begin);
                uint type = stream.ReadUInt32(Endian.Big);

                if (!Enum.IsDefined(typeof(CompressionType), type))
                    return false;
            }

            return true;
        }

        protected override void Read(Stream stream)
        {
            uint files = stream.ReadUInt32(Endian.Big);
            uint[] offsets = new uint[files];
            for (int i = 0; i < files; i++)
            {
                offsets[i] = stream.ReadUInt32(Endian.Big);
            }


            Root = new ArchiveDirectory() { OwnerArchive = this };
            for (int i = 0; i < files; i++)
            {
                stream.Seek(offsets[i], SeekOrigin.Begin);
                uint DeSize = stream.ReadUInt32(Endian.Big);
                CompressionType type = (CompressionType)stream.ReadUInt32(Endian.Big);

                long compressed = 0;
                if (i == offsets.Length - 1)
                {
                    compressed = stream.Length - offsets[i] - 8;
                }
                else
                {
                    compressed = offsets[i + 1] - offsets[i];
                }

                Stream DeStream;
                switch (type)
                {
                    case CompressionType.NONE:
                        DeStream = new SubStream(stream, DeSize);
                        break;
                    case CompressionType.LZSS:
                        //DeStream = new MemoryStream(LZSS.Decompress(stream, (int)DeSize));
                        DeStream = lZSS.Decompress(stream, (int)DeSize);
                        break;
                    case CompressionType.SLIDE:
                    case CompressionType.FSLIDE_ALT:
                    case CompressionType.FSLIDE:
                        DeStream = new MemoryStream(DecompressSlide(stream, (int)DeSize));
                        break;
                    case CompressionType.RLE:
                        Root.AddArchiveFile(stream, compressed, $"{i}_{type}.cmpres");
                        continue;
                    case CompressionType.INFLATE:
                        uint decompressed_size = stream.ReadUInt32(Endian.Big);
                        uint compressed_size = stream.ReadUInt32(Endian.Big);
                        DeStream = new MemoryStream(Compression<ZLib>.Decompress(stream.Read((int)compressed_size)));
                        break;
                    default:
                        Events.NotificationEvent?.Invoke(NotificationType.Warning, $"{nameof(CompressionType)}, unknown Compression type {type}.");
                        DeStream = new SubStream(stream, DeSize);
                        break;
                }

                DeStream.Seek(0, SeekOrigin.Begin);
                if (DeStream.ReadString(4) == "HSFV")
                {
                    Root.AddArchiveFile(DeStream, $"{i}_{type}.hsf");
                }
                else
                {
                    DeStream.Seek(12, SeekOrigin.Begin);
                    if (DeStream.ReadUInt32(Endian.Big) == 20)
                    {
                        Root.AddArchiveFile(DeStream, $"{i}_{type}.atb");
                    }
                    else
                    {
                        Root.AddArchiveFile(DeStream, $"{i}_{type}.bin");
                    }
                }
                DeStream.Seek(0, SeekOrigin.Begin);
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        public enum CompressionType : uint
        {
            NONE = 0,
            LZSS = 1,
            SLIDE = 2,
            FSLIDE_ALT = 3,
            FSLIDE = 4,
            RLE = 5,
            INFLATE = 7,
        }
        /// <summary>
        /// (F)SLIDE compression algorithm is similar to LZSS with a 4bit flag.
        /// </summary>
        /// <param name="fp"></param>
        /// <param name="decompressed_size"></param>
        /// <returns></returns>
        //base https://github.com/gamemasterplc/mpbintools/blob/master/bindump.c#L240
        private byte[] DecompressSlide(Stream fp, int decompressed_size)
        {
            uint temp_len = fp.ReadUInt32(Endian.Big);

            int dest_offset = 0;
            int code_word_bits_left = 0;
            uint code_word = 0;
            byte[] decompress_buffer = new byte[decompressed_size];


            while (dest_offset < decompressed_size)
            {
                //Reads New Code Word from Compressed Stream if Expired
                if (code_word_bits_left == 0)
                {
                    code_word = fp.ReadUInt32(Endian.Big);
                    code_word_bits_left = 32;
                }

                //Copies a Byte from the Source to the Destination and Window Buffer
                if ((code_word & 0x80000000) != 0)
                {
                    decompress_buffer[dest_offset++] = fp.ReadUInt8();
                }
                else
                {
                    //Interpret Next 2 Bytes as a Backwards Distance and Length
                    byte byte1 = fp.ReadUInt8();
                    byte byte2 = fp.ReadUInt8();

                    int dist_back = (((byte1 & 0x0F) << 8) | byte2) + 1;
                    int copy_length = ((byte1 & 0xF0) >> 4) + 2;

                    //Special Case Where the Upper 4 Bits of byte1 are 0
                    if (copy_length == 2)
                    {
                        copy_length = fp.ReadUInt8() + 18;
                    }

                    //Copy Some Bytes from Window Buffer
                    byte value;
                    for (int i = 0; i < copy_length && dest_offset < decompressed_size; i++)
                    {
                        if (dist_back > dest_offset)
                            value = 0;
                        else
                            value = decompress_buffer[dest_offset - dist_back];

                        decompress_buffer[dest_offset++] = value;
                    }
                }
                code_word <<= 1;
                code_word_bits_left--;
            }
            return decompress_buffer;
        }




    }
}
