using AuroraLib.Common;
using System.Text;

namespace AuroraLib.Archives
{
    public abstract class ArchiveObject : IDisposable, IFileSystemInfo
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
        /// Size of the <see cref="ArchiveObject" />.
        /// </summary>
        public abstract long Size { get; }

        /// <summary>
        /// The parent directory (Null if non-existant)
        /// </summary>
        public ArchiveDirectory Parent { get; set; }

        /// <summary>
        /// The Archive that owns this <see cref="ArchiveObject" />.
        /// </summary>
        public Archive OwnerArchive { get; set; }

        public DateTime CreationTimeUtc { get; set; } = DateTime.UtcNow;

        public DateTime LastWriteTimeUtc { get; set; } = DateTime.UtcNow;

        public DateTime LastAccessTimeUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Create a new <see cref="ArchiveObject" />.
        /// </summary>
        protected ArchiveObject() { }

        internal void GetFullPath(StringBuilder PathBuilder)
        {
            if (Parent?.Parent != null)
            {
                int i = PathBuilder.Length;
                Parent.GetFullPath(PathBuilder);
                if (i != PathBuilder.Length)
                    PathBuilder.Append(Path.DirectorySeparatorChar);
            }
            PathBuilder.Append(Name);
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
