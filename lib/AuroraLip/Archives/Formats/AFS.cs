using AFSLib;
using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
{
    public class AFS : Archive, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "AFS";

        private AFSLib.AFS AFSBase;

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

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
