using System;
using System.Text;

namespace AuroraLip.Archives
{
    public abstract class ArchiveObject: IDisposable
    {
        /// <summary>
        /// Name of the <see cref="ArchiveObject" />.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The full path of this file.
        /// </summary>
        public string FullPath
        {
            get
            {
                StringBuilder path = new StringBuilder();
                GetFullPath(path);
                return path.ToString();
            }
        }

        /// <summary>
        /// The parent directory (Null if non-existant)
        /// </summary>
        public ArchiveDirectory Parent { get; set; }

        /// <summary>
        /// The Archive that owns this <see cref="ArchiveObject" />.
        /// </summary>
        public Archive OwnerArchive { get; set; }

        /// <summary>
        /// Create a new <see cref="ArchiveObject" />.
        /// </summary>
        protected ArchiveObject() { }

        internal void GetFullPath(StringBuilder Path)
        {
            if (Parent != null)
            {
                Parent.GetFullPath(Path);
                Path.Append("/");
                Path.Append(Name);
            }
            else
            {
                Path.Append(Name);
            }
        }

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Parent = null;
                OwnerArchive = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
