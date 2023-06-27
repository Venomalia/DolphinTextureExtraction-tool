namespace AuroraLib.Common
{
    public class HeaderInfo : IEquatable<HeaderInfo>, IEquatable<Stream>
    {
        public readonly byte[] Bytes;

        public int Offset { get; } = 0;

        public string Magic => EncodingEX.DefaultEncoding.GetString(Bytes);

        public string MagicASKI => EncodingEX.GetValidString(Bytes, b => b < 32 && b >= 127);

        public HeaderInfo(byte[] bytes, int offset = 0)
        {
            Bytes = bytes;
            Offset = offset;
        }

        public HeaderInfo(string magic, int offset = 0)
        {
            Bytes = magic.GetBytes();
            Offset = offset;
        }

        public HeaderInfo(Stream stream)
        {
            List<byte> bytes = new();
            stream.Seek(0, SeekOrigin.Begin);

            int readbyte;
            while ((readbyte = stream.ReadByte()) > -1)
            {
                if (bytes.Count >= 4)
                {
                    int X = 0;
                    foreach (byte b in bytes)
                        if (b == readbyte) X++;
                    if (X > 3)
                    {
                        bytes.Clear();
                        break;
                    }
                }
                if (readbyte == 10)
                {
                    while ((readbyte = stream.ReadByte()) > -1 && stream.Position < 24)
                        if (readbyte == 0) break;

                    if (stream.Position >= 24 || readbyte == -1)
                        bytes.Clear();

                    break;
                }
                if (bytes.Count > 16)
                {
                    bytes.Clear();
                    break;
                }
                if (readbyte == 0)
                {
                    if (bytes.Count == 0)
                    {
                        Offset++;
                        continue;
                    }
                    break;
                }
                bytes.Add((byte)readbyte);
            }
            if (EncodingEX.ValidSize(bytes.ToArray(), b => b < 32 || b >= 127) <= 1)
                bytes.Clear();

            Bytes = bytes.ToArray();
            stream.Seek(0, SeekOrigin.Begin);
        }

        public bool Equals(HeaderInfo other)
            => other != null && Bytes.ArrayEqual(other.Bytes) && Offset == other.Offset;

        public bool Equals(Stream stream)
        {
            stream.Position = Offset;
            return stream.Length >= Bytes.Length && stream.Read(Bytes.Length).ArrayEqual(Bytes);
        }
    }
}
