using AuroraLib.Common;
using AuroraLib.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraLib.Texture.Formats
{
    public class TPX : JUTTexture, IFileAccess, IHasIdentifier
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => Magic;

        public static readonly Identifier32 Magic = new(302581280);

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 12 && stream.Match(Magic) && stream.At(0x100, s => s.Match(TPL.Magic));

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(Magic);
            stream.Position += 252;
            long HeaderStart = stream.Position;
            stream.MatchThrow(TPL.Magic);
            TPL.ProcessStream(stream, HeaderStart, this);
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();
    }
}
