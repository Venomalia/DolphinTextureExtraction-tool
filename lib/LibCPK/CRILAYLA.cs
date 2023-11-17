using System.Buffers;
using System.Runtime.CompilerServices;

namespace LibCPK
{
    public static class CRILAYLA
    {
        private const ulong Identifier = 4705233847682945603; // CRILAYLA

        static public void Decompress(Stream source, Stream destination)
        {
            BinaryReader reader = new(source);
            if (reader.ReadUInt64() != Identifier)
                throw new ArgumentException();

            uint decompressedSize = reader.ReadUInt32();
            uint compressedSize = reader.ReadUInt32();

            byte[] destinationBuffer = ArrayPool<byte>.Shared.Rent((int)decompressedSize);
            byte[] sourceBuffer = ArrayPool<byte>.Shared.Rent((int)compressedSize + 0x100);
            try
            {
                Span<byte> destinationSpan = destinationBuffer.AsSpan(0, (int)decompressedSize);
                Span<byte> sourceSpan = sourceBuffer.AsSpan(0, (int)compressedSize);
                Span<byte> headerSpan = sourceBuffer.AsSpan((int)compressedSize, 0x100);
                source.Read(sourceSpan);
                source.Read(headerSpan);
                DecompressHeaderless(sourceSpan, destinationSpan);
                destination.Write(headerSpan);
                destination.Write(destinationSpan);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(destinationBuffer);
                ArrayPool<byte>.Shared.Return(sourceBuffer);
            }
        }

        unsafe static public void Compress(ReadOnlySpan<byte> source, Stream destination)
        {
            byte[] destinationBuffer = ArrayPool<byte>.Shared.Rent(source.Length);
            try
            {
                fixed (byte* src = source)
                {
                    Span<byte> result = CompressHeaderless(src + 0x100, source.Length - 0x100, destinationBuffer);
                    BinaryWriter reader = new(destination);
                    reader.Write(Identifier);
                    reader.Write(source.Length - 0x100);
                    reader.Write(result.Length);
                    destination.Write(result);
                    destination.Write(source[..0x100]);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(destinationBuffer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void DecompressHeaderless(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            int sourcePointer = source.Length - 1;
            int destinationPointer = destination.Length - 1;
            byte flag = 0;
            int flagBitsLeft = 0;
            ReadOnlySpan<int> vleLevels = stackalloc int[4] { 2, 3, 5, 8 };
            ReadOnlySpan<int> vleFlags = stackalloc int[4] { 0x3, 0x7, 0x1F, 0xFF };

            while (destinationPointer >= 0)
            {
                if (GetBits(source, ref sourcePointer, ref flag, ref flagBitsLeft, 1) == 1)
                {
                    int distance = GetBits(source, ref sourcePointer, ref flag, ref flagBitsLeft, 13) + 3;
                    int length = 3;

                    int vle = 0, value;
                    while (true)
                    {
                        length += value = GetBits(source, ref sourcePointer, ref flag, ref flagBitsLeft, vleLevels[vle]);
                        if (value != vleFlags[vle])
                            break;

                        if (value != 255)
                            vle++;
                    }

                    for (int i = 0; i < length; i++)
                    {
                        destination[destinationPointer] = destination[destinationPointer + distance];
                        destinationPointer--;
                    }
                }
                else
                {
                    destination[destinationPointer--] = (byte)GetBits(source, ref sourcePointer, ref flag, ref flagBitsLeft, 8);
                }
            }
        }

        private static ushort GetBits(ReadOnlySpan<byte> input, ref int inputPointer, ref byte flag, ref int flagBitsLeft, int bitCount)
        {
            ushort value = 0;

            while (bitCount > 0)
            {
                if (flagBitsLeft == 0)
                {
                    flag = input[inputPointer--];
                    flagBitsLeft = 8;
                }

                int read = Math.Min(flagBitsLeft, bitCount);

                value <<= read;

                value |= (ushort)(flag >> (flagBitsLeft - read) & ((1 << read) - 1));

                flagBitsLeft -= read;
                bitCount -= read;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe Span<byte> CompressHeaderless(byte* source, int srcLen, Span<byte> destination)
        {
            int n = srcLen - 1;
            int m = destination.Length - 0x1;
            int T = 0, d = 0, p, q = 0, i, j, k;
            while (n >= 0)
            {
                j = n + 3 + 0x2000;
                if (j > srcLen) j = srcLen;
                for (i = n + 3, p = 0; i < j; i++)
                {
                    for (k = 0; k <= n; k++)
                    {
                        if (*(source + n - k) != *(source + i - k))
                            break;
                    }
                    if (k > p)
                    {
                        q = i - n - 3;
                        p = k;
                    }
                }
                if (p < 3)
                {
                    d = (d << 9) | (*(source + n--));
                    T += 9;
                }
                else
                {
                    d = (((d << 1) | 1) << 13) | q; T += 14; n -= p;
                    if (p < 6)
                    {
                        d = (d << 2) | (p - 3);
                        T += 2;
                    }
                    else if (p < 13)
                    {
                        d = (((d << 2) | 3) << 3) | (p - 6);
                        T += 5;
                    }
                    else if (p < 44)
                    {
                        d = (((d << 5) | 0x1f) << 5) | (p - 13);
                        T += 10;
                    }
                    else
                    {
                        d = ((d << 10) | 0x3ff); T += 10; p -= 44;
                        while (true)
                        {
                            while (T >= 8)
                            {
                                destination[m--] = (byte)((d >> (T - 8)) & 0xff);
                                T -= 8;
                                d &= ((1 << T) - 1);
                            }
                            if (p < 255)
                                break;
                            d = (d << 8) | 0xff;
                            T += 8;
                            p -= 0xff;
                        }
                        d = (d << 8) | p;
                        T += 8;
                    }
                }
                while (T >= 8)
                {
                    destination[m--] = (byte)((d >> (T - 8)) & 0xff);
                    T -= 8;
                    d &= ((1 << T) - 1);
                }
            }

            if (T != 0)
            {
                destination[m--] = (byte)(d << (8 - T));
            }
            destination[m--] = 0;
            while (true)
            {
                if (((destination.Length - m) & 3) == 0) break;
                destination[m--] = 0;
            }
            return destination[m..];
        }
    }
}
