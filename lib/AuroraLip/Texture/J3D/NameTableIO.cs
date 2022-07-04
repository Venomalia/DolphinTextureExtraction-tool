using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace AuroraLip.Texture.J3D
{

    /*
    * Super Hackio Incorporated
    * "Copyright © Super Hackio Incorporated 2020-2021"
    * https://github.com/SuperHackio/Hack.io
    */

    public static class NameTableIO
    {
        public static List<string> ReadStringTable(this Stream reader, int offset)
        {
            List<string> names = new List<string>();

            reader.Position = offset;

            short stringCount = BitConverter.ToInt16(reader.ReadBigEndian(0, 2), 0);
            reader.Position += 0x02;

            for (int i = 0; i < stringCount; i++)
            {
                reader.Position += 0x02;
                short nameOffset = BitConverter.ToInt16(reader.ReadBigEndian(0, 2), 0);
                long saveReaderPos = reader.Position;
                reader.Position = offset + nameOffset;

                names.Add(reader.ReadString());

                reader.Position = saveReaderPos;
            }

            return names;
        }

        public static void WriteStringTable(this Stream writer, List<string> names)
        {
            long start = writer.Position;

            writer.WriteBigEndian(BitConverter.GetBytes((short)names.Count), 0, 2);
            writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);

            foreach (string st in names)
            {
                writer.WriteBigEndian(BitConverter.GetBytes(HashString(st)), 0, 2);
                writer.Write(new byte[2], 0, 2);
            }

            long curOffset = writer.Position;
            for (int i = 0; i < names.Count; i++)
            {
                writer.Seek((int)(start + (6 + i * 4)), SeekOrigin.Begin);
                writer.WriteBigEndian(BitConverter.GetBytes((short)(curOffset - start)), 0, 2);
                writer.Seek((int)curOffset, SeekOrigin.Begin);

                writer.WriteString(names[i], 0x00);

                curOffset = writer.Position;
            }
        }

        private static ushort HashString(string str)
        {
            ushort hash = 0;

            foreach (char c in str)
            {
                hash *= 3;
                hash += (ushort)c;
            }

            return hash;
        }
    }
}
