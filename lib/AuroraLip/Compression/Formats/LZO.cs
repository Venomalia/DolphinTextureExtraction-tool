using IronCompress;

namespace AuroraLib.Compression.Formats
{
    public class LZO : IronCompressBase
    {
        protected override Codec Codec => Codec.LZO;

        public const string Extension = ".lzo";

        public override bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 0x10 && extension.ToLower() == Extension;
    }
}
