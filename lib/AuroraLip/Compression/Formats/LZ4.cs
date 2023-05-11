using IronCompress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraLip.Compression.Formats
{
    public class LZ4 : IronCompressBase
    {
        protected override Codec Codec => Codec.LZ4;

        public const string Extension = ".lz4";

        public override bool IsMatch(Stream stream, in string extension = "")
            => extension.ToLower() == Extension;
    }
}
