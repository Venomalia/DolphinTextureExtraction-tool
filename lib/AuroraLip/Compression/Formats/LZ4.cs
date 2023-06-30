using IronCompress;

namespace AuroraLib.Compression.Formats
{
    public class LZ4 : IronCompressBase
    {
        protected override Codec Codec => Codec.LZ4;

        public const string Extension = ".lz4";

        public override bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 0x10 && extension.ToLower() == Extension;
    }
}
