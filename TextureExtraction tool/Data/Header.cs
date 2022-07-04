using AuroraLip.Common;
using Hack.io;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DolphinTextureExtraction_tool
{
    public class Header : IEquatable<string>, IEquatable<Header>, IMagicIdentify
    {
        public byte[] Bytes { get; }

        public string Magic => Bytes.ToValidString(b => b >= 32 && b != 127);

        public string MagicASKI => Bytes.ToValidString(b => b >= 32 && b < 127);

        public int Offset { get; } = 0;

        public Header(byte[] bytes, int offset = 0)
        {
            Bytes = bytes;
            Offset = offset;
        }

        public Header(string magic, int offset = 0)
        {
            Bytes = Encoding.GetEncoding(1252).GetBytes(magic);
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

        public bool Equals(string magic) => Magic == magic || MagicASKI == magic;

        public bool Equals(Header other)
        {
            return (Magic.Length > 2 && Magic == other.Magic) || MagicASKI.Length > 4 && MagicASKI == other.MagicASKI;
        }
    }
}
