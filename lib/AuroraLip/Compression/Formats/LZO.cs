using IronCompress;

namespace AuroraLib.Compression.Formats
{
    public class LZO : IronCompressBase
    {
        protected override Codec Codec => Codec.LZO;

        public const string Extension = ".lzo";

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 0x10 && extension.Contains(Extension, StringComparison.InvariantCultureIgnoreCase);
    }
}
