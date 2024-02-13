using AuroraLib.Common;
using AuroraLib.Compression.Interfaces;
using System.Buffers;
using System.IO.Compression;

namespace AuroraLib.Compression.Algorithms
{
    /// <summary>
    /// LH compression algorithm base on (LZ77 + Huffman)
    /// Used in Mario Sports Mix and Newer Super Mario Bros
    /// </summary>
    // https://github.com/Treeki/RandomStuff/blob/master/LHDecompressor.cpp
    public class LH : ICompressionAlgorithm, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 0x10 && stream.ReadUInt8() == 64;


        public void Decompress(Stream source, Stream destination)
        {
            source.Position = 1;
            int destinationLength = (int)source.ReadUInt24();
            if (destinationLength == 0) destinationLength = (int)source.ReadUInt32();

            Decompress_ALG(source, destination, destinationLength);
        }

        public void Compress(ReadOnlySpan<byte> source, Stream destination, CompressionLevel level = CompressionLevel.Optimal) => throw new NotImplementedException();

        public static void Decompress_ALG(Stream source, Stream destination, int decomLength)
        {
            BitReaderX Reader = new(source, Endian.Big);
            //Read Huff length Byts
            int Huffsize = (Reader.ReadUInt16(Endian.Little) << 5) + 16;
            Span<short> Hufflength = stackalloc short[Huffsize / 9 + 1];

            for (int i = 1; i < Hufflength.Length; i++)
                Hufflength[i] = (short)Reader.ReadInt(9);

            Reader.BitPosition += Huffsize - (Hufflength.Length - 1) * 9;

            //Read Huff offset Byts
            Huffsize = (Reader.ReadUInt8(Endian.Big) << 5) + 24;
            Span<byte> Huffoffset = stackalloc byte[Huffsize / 5 + 1];

            for (int i = 1; i < Huffoffset.Length; i++)
                Huffoffset[i] = (byte)Reader.ReadInt(5);

            Reader.BitPosition += Huffsize - (Huffoffset.Length - 1) * 5;

            Decompress_ALG(Reader, destination, decomLength, Hufflength, Huffoffset);
        }

        public static void Decompress_ALG(BitReaderX source, Stream outStream, int decomLength, ReadOnlySpan<short> hufflength, ReadOnlySpan<byte> huffoffset)
        {
            int destinationPointer = 0, huffPointer, offset;
            byte flag;
            short lengthdata;

            byte[] destination = ArrayPool<byte>.Shared.Rent(decomLength);
            try
            {
                while (true)
                {
                    // Get lengthdata
                    huffPointer = 1;
                    while (true)
                    {
                        flag = (byte)source.ReadInt(1);
                        offset = ((hufflength[huffPointer] & 0x7F) + 1 << 1) + flag;
                        offset = (huffPointer * 2 & ~3) / 2 + offset;

                        if ((hufflength[huffPointer] & 0x100 >> flag) > 0)
                        {
                            lengthdata = hufflength[offset];
                            break;
                        }
                        huffPointer = offset;
                    }

                    // if Huff or LZ block, copy?
                    if (lengthdata < 0x100)
                    {
                        //If bit 9 is zero, copy the data
                        destination[destinationPointer++] = (byte)lengthdata;
                    }
                    else
                    {
                        lengthdata = (short)((lengthdata & 0xFF) + 3);

                        // Get offsetdata
                        huffPointer = 1;
                        while (true)
                        {
                            flag = (byte)source.ReadInt(1);
                            offset = ((huffoffset[huffPointer] & 7) + 1 << 1) + flag;

                            if ((huffoffset[huffPointer] & 0x10 >> flag) > 0)
                            {
                                huffPointer = (huffPointer * 2 & ~3) / 2;
                                offset = huffoffset[huffPointer + offset];
                                break;
                            }
                            huffPointer = (huffPointer * 2 & ~3) / 2 + offset;
                        }

                        if (offset > 1)
                            offset = (int)source.ReadInt(offset - 1) | 1 << offset - 1;

                        offset++;

                        // copy LZ block
                        while (lengthdata-- > 0)
                        {
                            destination[destinationPointer] = destination[destinationPointer - offset];
                            destinationPointer++;
                        }
                    }

                    // Check to see if we reached the end
                    if (destinationPointer == decomLength)
                    {
                        outStream.Write(destination, 0, decomLength);
                        return;
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(destination);
            }
        }
    }
}
