using AuroraLib.Common;
using AuroraLib.Compression;
using IronCompress;
using System.Buffers;

namespace AuroraLib.Compression.Formats
{
    public class LZO : IronCompressBase
    {
        protected override Codec Codec => Codec.LZO;

        public const string Extension = ".lzo";

        public override bool IsMatch(Stream stream, in string extension = "")
            => extension.ToLower() == Extension;
    }
}
