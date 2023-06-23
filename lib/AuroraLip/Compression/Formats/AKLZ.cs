﻿using AuroraLib.Common;

namespace AuroraLib.Compression.Formats
{
    /// <summary>
    /// This LZSS header was used in Skies of Arcadia Legends
    /// </summary>
    public class AKLZ : ICompression, IMagicIdentify
    {
        public bool CanWrite { get; } = true;

        public bool CanRead { get; } = true;

        public string Magic => magic;

        private const string magic = "AKLZ~?Qd=ÌÌÍ";

        private static readonly LZSS LZSS = new(12, 4, 2);

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 4 && stream.MatchString(Magic);

        public byte[] Decompress(Stream source)
        {
            source.Position += 0xC;
            uint decompressedSize = source.ReadUInt32(Endian.Big);
            return LZSS.Decompress(source, (int)decompressedSize).ToArray();
        }

        public void Compress(in byte[] source, Stream destination) => throw new NotImplementedException();
    }
}