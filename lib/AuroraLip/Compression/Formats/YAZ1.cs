using AuroraLib.Core.Interfaces;

namespace AuroraLib.Compression.Formats
{
    public class YAZ1 : YAZ0
    {
        public override IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("Yaz1");
    }
}
