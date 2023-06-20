using AuroraLib.Compression;

namespace AuroraLib.Compression.Formats
{
    //base on https://github.com/ShrekBoards/shrek-superslam/blob/master/src/compression.rs
    public class Shrek : ICompression
    {
        public bool CanRead => false;

        public bool CanWrite => false;

        private const int MAX_DISTANCE = 0x1011D;

        public byte[] Decompress(Stream source)
        {
            throw new NotImplementedException();
        }

        public static byte[] Decompress_ALG(byte[] compressed, int decomLength)
        {
            List<byte> decompressed = new(decomLength);
            int index = 0;

            while (true)
            {
                int current = compressed[index++];
                int length = (current & 7) + 1;
                int distance = current >> 3;

                if (distance == 0x1E)
                {
                    current = compressed[index++];
                    distance = current + 0x1E;
                }
                else if (distance > 0x1E)
                {
                    distance += compressed[index++];
                    current = compressed[index++];
                    distance += (current << 8) + 0xFF;
                    if (distance == MAX_DISTANCE)
                    {
                        length--;
                    }
                }

                if (distance != 0)
                {
                    Span<byte> segment = compressed.AsSpan(index, distance);
                    decompressed.AddRange(segment.ToArray());
                    index += distance;
                }

                int bound = length;
                for (int i = 0; i < bound; i++)
                {
                    current = compressed[index++];
                    length = current & 7;
                    distance = current >> 3;

                    if (length == 0)
                    {
                        length = compressed[index++];
                        if (length == 0)
                        {
                            return decompressed.ToArray();
                        }
                        length += 7;
                    }

                    if (distance == 0x1E)
                    {
                        current = compressed[index++];
                        distance = current + 0x1E;
                    }
                    else if (distance > 0x1E)
                    {
                        current = compressed[index++];
                        distance += current;
                        current = compressed[index++];
                        distance += (current << 8) + 0xFF;
                    }

                    for (int j = 0; j < length; j++)
                    {
                        decompressed.Add(decompressed[decompressed.Count - 1 - distance]);
                    }
                }
            }
        }


        public void Compress(in byte[] source, Stream destination) => throw new NotImplementedException();

        public bool IsMatch(Stream stream, in string extension = "")
            => false;
    }

}
