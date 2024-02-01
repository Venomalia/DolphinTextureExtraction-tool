using AuroraLib.Common;

namespace AuroraLib.Archives
{
    /*
    * Super Hackio Incorporated
    * "Copyright © Super Hackio Incorporated 2020-2021"
    * https://github.com/SuperHackio/Hack.io
    */

    /// <summary>
    /// The base class for Archive like systems
    /// </summary>
    public abstract class Archive : IDisposable, IFileSystemInfo, IFileRequest
    {
        /// <summary>
        /// Filename of this Archive.
        /// <para/>Set using <see cref="Save(string)"/>;
        /// </summary>
        public string FullPath { get; protected set; } = null;

        /// <summary>
        /// Get the name of the archive without the path
        /// </summary>
        public string Name => FullPath == null ? FullPath : new FileInfo(FullPath).Name;

        /// <summary>
        /// The Root Directory of the Archive
        /// </summary>
        public ArchiveDirectory Root
        {
            get => root;
            set
            {
                if (root != null && root.OwnerArchive == this)
                    root.OwnerArchive = null;

                if (value != null)
                {
                    value.OwnerArchive = this;
                    value.Parent = null;
                }

                root = value;
            }
        }

        private ArchiveDirectory root;

        /// <summary>
        /// The total amount of files inside this archive.
        /// </summary>
        public int TotalFileCount => Root?.GetCountAndChildren() ?? 0;

        public DateTime CreationTimeUtc { get; set; } = DateTime.UtcNow;

        public DateTime LastWriteTimeUtc { get; set; } = DateTime.UtcNow;

        public DateTime LastAccessTimeUtc { get; set; } = DateTime.UtcNow;

        public Events.FileRequestDelegate FileRequest { get; set; }

        #region Constructors

        /// <summary>
        /// Create an empty archive
        /// </summary>
        public Archive()
        { }

        /// <summary>
        /// Open an archive
        /// </summary>
        /// <param name="filename">Archive full filepath</param>
        public Archive(string filename)
        {
            FileStream stream = new FileStream(filename, FileMode.Open);
            Open(stream, filename);
            stream.Close();
        }

        /// <summary>
        /// Open an archive that's stored inside a stream.
        /// <para/> Stream will be a <see cref="MemoryStream"/> if the Hack.io.YAZ0 library was used.
        /// </summary>
        /// <param name="stream">Memorystream containing the archiev</param>
        /// <param name="fullpath">Filename to give</param>
        public Archive(Stream stream, string fullpath = null)
            => Open(stream, fullpath);

        #endregion Constructors

        /// <summary>
        /// The Binary I/O function for reading the file
        /// </summary>
        /// <param name="ArchiveFile"></param>
        protected abstract void Read(Stream ArchiveFile);

        /// <summary>
        /// The Binary I/O function for writing the file
        /// </summary>
        /// <param name="ArchiveFile"></param>
        protected abstract void Write(Stream ArchiveFile);

        #region Public Functions

        /// <summary>
        /// Save the Archive to a File
        /// </summary>
        /// <param name="filepath">New file to save to</param>
        public void Save(string filepath)
        {
            FullPath = filepath;
            FileStream fs = new FileStream(filepath, FileMode.Create);
            Save(fs);
            fs.Close();
        }

        /// <summary>
        /// Write the Archive to a Stream
        /// </summary>
        /// <param name="stream"></param>
        public void Save(Stream stream) => Write(stream);

        /// <summary>
        /// Read a Archive from Stream.
        /// </summary>
        /// <param name="stream"></param>
        public void Open(Stream stream, in string fullpath = null)
        {
            FullPath = fullpath;
            Read(stream);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            int Count = Root?.GetCountAndChildren() ?? 0;
            return $"{new FileInfo(FullPath).Name} - {Count} File{(Count > 1 ? "s" : "")} total";
        }

        #endregion Public Functions

        #region File Functions

        /// <summary>
        /// Get or Set a file based on a path. When setting, if the file doesn't exist, it will be added (Along with any missing subdirectories). Set the file to null to delete it
        /// </summary>
        /// <param name="Path">The Path to take. Does not need the Root name to start, but cannot start with a '/'</param>
        /// <returns></returns>
        public ArchiveObject this[string Path]
        {
            get
            {
                if (Root is null || Path is null)
                    return null;
                if (Path.StartsWith(Root.Name + "/"))
                    Path = Path.Substring(Root.Name.Length + 1);
                return Root[Path];
            }
            set
            {
                if (!(value is ArchiveFile || value is ArchiveDirectory || value is null))
                    throw new Exception($"Invalid object type of {value.GetType().ToString()}");

                if (Root is null)
                {
                    Root = NewDirectory(this, null);
                    Root.Name = Path.Split('/')[0];
                }

                if (Path.StartsWith(Root.Name + "/"))
                    Path = Path.Substring(Root.Name.Length + 1);

                OnItemSet(value, Path);
                Root[Path] = value;
            }
        }

        /// <summary>
        /// Executed when you use ArchiveBase["FilePath"] to set a file
        /// </summary>
        /// <param name="value"></param>
        /// <param name="Path"></param>
        protected virtual void OnItemSet(object value, string Path)
        {
        }

        /// <summary>
        /// Checks to see if an Item Exists based on a Path
        /// </summary>
        /// <param name="path">The path to take</param>
        /// <param name="IgnoreCase">Ignore casing of the file</param>
        /// <returns>false if the Item isn't found</returns>
        public bool ItemExists(string path, bool IgnoreCase = false)
        {
            if (path.StartsWith(Root.Name + Path.DirectorySeparatorChar))
                path = path.Substring(Root.Name.Length + 1);
            return Root.ItemExists(path, IgnoreCase);
        }

        /// <summary>
        /// This will return the absolute path of an item if it exists in some way. Useful if you don't know the casing of the filename inside the file. Returns null if nothing is found.
        /// </summary>
        /// <param name="Path">The path to get the Actual path from</param>
        /// <returns>null if nothing is found</returns>
        public string GetItemKeyFromNoCase(string Path)
        {
            if (Path.ToLower().StartsWith(Root.Name.ToLower() + "/"))
                Path = Path.Substring(Root.Name.Length + 1);
            return Root.GetItemKeyFromNoCase(Path, true);
        }

        /// <summary>
        /// Clears all the files out of this archive
        /// </summary>
        public void ClearAll()
        { Root.Clear(); }

        /// <summary>
        /// Moves an item to a new directory
        /// </summary>
        /// <param name="OriginalPath"></param>
        /// <param name="NewPath"></param>
        public void MoveItem(string OriginalPath, string NewPath)
        {
            if (OriginalPath.StartsWith(Root.Name + "/"))
                OriginalPath = OriginalPath.Substring(Root.Name.Length + 1);
            if (OriginalPath.Equals(NewPath))
                return;
            if (ItemExists(NewPath))
                throw new Exception("An item with that name already exists in that directory");

            dynamic dest = this[OriginalPath];
            string[] split = NewPath.Split('/');
            dest.Name = split[split.Length - 1];
            this[OriginalPath] = null;
            this[NewPath] = dest;
        }

        /// <summary>
        /// Search the archive for files that match the regex
        /// </summary>
        /// <param name="Pattern">The regex pattern to use</param>
        /// <param name="RootLevelOnly">If true, all subdirectories will be skipped</param>
        /// <param name="IgnoreCase">Ignore the filename casing</param>
        /// <returns></returns>
        public List<string> FindItems(string Pattern, bool RootLevelOnly = false, bool IgnoreCase = false) => Root.FindItems(Pattern, RootLevelOnly, IgnoreCase);

        #endregion File Functions

        /// <summary>
        /// Create an Archive from a Folder
        /// </summary>
        /// <param name="Folderpath">Folder to make an archive from</param>
        public void Import(string Folderpath) => Root = NewDirectory(Folderpath, this);

        protected virtual ArchiveDirectory NewDirectory() => new ArchiveDirectory();

        protected virtual ArchiveDirectory NewDirectory(Archive Owner, ArchiveDirectory parent) => new ArchiveDirectory(Owner, parent);

        protected virtual ArchiveDirectory NewDirectory(string filename, Archive Owner) => new ArchiveDirectory(filename, Owner);

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    Root?.Dispose();

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
