using IronCompress;

namespace AuroraLib.Compression.Formats
{
    public class Zstd : IronCompressBase
    {
        protected override Codec Codec => Codec.Zstd;

        public const string Extension = ".zs";

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.Contains(Extension, StringComparison.InvariantCultureIgnoreCase);
    }
}
