using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DolphinTextureExtraction_tool
{
    public class Header : IEquatable<string>, IEquatable<Header>, IMagicNumber
    {
        public byte[] Bytes { get; }

        public string Magic => Encoding();

        public string MagicASKI => Encoding(b => b >= 32 && b < 127);

        public int Offset { get; } = 0;

        private static Predicate<byte> Validbytes = b => b >= 32 && b != 127;

        public Header(byte[] bytes, int offset = 0)
        {
            Bytes = bytes;
            Offset = offset;
        }

        public Header(string magic, int offset = 0)
        {
            Bytes = System.Text.Encoding.GetEncoding(1252).GetBytes(magic);
            Offset = offset;
        }



        public Header(Stream stream, int length, int offset = 0)
        {
            List<byte> bytes = new List<byte>();
            long position = stream.Position;
            stream.Position += offset;

            int readbyte;
            while ((readbyte = stream.ReadByte()) > -1 && bytes.Count < length)
            {
                if (readbyte == 10) break;
                bytes.Add((byte)readbyte);
            }

            Bytes = bytes.ToArray();
            stream.Position = position;
        }

        /// <summary>
        /// Find a header in a data stream.
        /// </summary>
        /// <param name="stream"></param>
        public Header(Stream stream)
        {
            List<byte> bytes = new List<byte>();
            long position = stream.Position;

            int readbyte;
            while ((readbyte = stream.ReadByte()) > -1 && bytes.Count < 16)
            {
                if (readbyte == 0 || readbyte == 10)
                {
                    if (readbyte == 0 && bytes.Count == 0)
                    {
                        Offset++;
                        continue;
                    }
                    break;
                }

                bytes.Add((byte)readbyte);
            }

            Bytes = bytes.ToArray();
            stream.Position = position;
        }

        public string Encoding(Predicate<byte> validbytes) => Encoding(null, validbytes);

        public string Encoding(Encoding encoder = null, Predicate<byte> validbytes = null)
        {
            if (validbytes == null) validbytes = Validbytes;

            List<byte> magicbytes = new List<byte>();
            foreach (byte b in Bytes)
            {
                if (validbytes.Invoke(b))
                {
                    magicbytes.Add(b);
                }
            }

            if (encoder == null) encoder = System.Text.Encoding.GetEncoding(1252);
            return encoder.GetString(magicbytes.ToArray());
        }

        public bool Equals(string magic) => Magic == magic || MagicASKI == magic;

        public bool Equals(Header other)
        {
            return (Magic.Length > 2 && Magic == other.Magic) || MagicASKI.Length > 4 && MagicASKI == other.MagicASKI;
        }
    }
}
