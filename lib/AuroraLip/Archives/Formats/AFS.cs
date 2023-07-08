using AFSLib;
using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    public class AFS : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifierA;

        public static readonly Identifier32 _identifierA = new("AFS ");

        public static readonly Identifier32 _identifierB = new((byte)'A', (byte)'F', (byte)'S', 0);

        private AFSLib.AFS AFSBase;

        public bool IsMatch(Stream stream, in string extension = "")
        {
            Identifier32 identifier = stream.Read<Identifier32>();
            return identifier == _identifierA || identifier == _identifierB;
        }

        protected override void Read(Stream ArchiveFile)
        {
            AFSBase = new AFSLib.AFS(ArchiveFile);
            Root = new ArchiveDirectory() { OwnerArchive = this };

            foreach (Entry item in AFSBase.Entries)
            {
                if (item is StreamEntry Streamitem)
                {
                    Root.AddArchiveFile(Streamitem.GetSubStream(), Streamitem.SanitizedName);
                }
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        private bool disposedValue;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && AFSBase != null)
                {
                    AFSBase.Dispose();
                }
                disposedValue = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }
    }
}
