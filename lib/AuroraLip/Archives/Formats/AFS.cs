using AFSLib;
using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// CRIWARE AFS archive
    /// </summary>
    public sealed class AFS : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifierA;

        public static readonly Identifier32 _identifierA = new("AFS ");

        public static readonly Identifier32 _identifierB = new((byte)'A', (byte)'F', (byte)'S', 0);

        private AFSLib.AFS AFSBase;

        public AFS()
        {
        }

        public AFS(string name) : base(name)
        {
        }

        public AFS(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
        {
            Identifier32 identifier = stream.Read<Identifier32>();
            return identifier == _identifierA || identifier == _identifierB;
        }

        protected override void Deserialize(Stream source)
        {
            AFSBase = new AFSLib.AFS(source);

            foreach (Entry item in AFSBase.Entries)
            {
                if (item is StreamEntry Streamitem)
                {
                    Add(new FileNode(Streamitem.SanitizedName, Streamitem.GetSubStream()));
                }
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        private bool disposedValue;

        protected override void Dispose(bool disposing)
        {
            // Call base class implementation.
            base.Dispose(disposing);
            if (!disposedValue)
            {
                if (disposing && AFSBase != null)
                {
                    AFSBase.Dispose();
                }
                disposedValue = true;
            }

        }
    }
}
