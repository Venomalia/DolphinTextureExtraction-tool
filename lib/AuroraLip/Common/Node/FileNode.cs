namespace AuroraLib.Common.Node
{
    /// <summary>
    /// Represents a file node.
    /// </summary>
    public sealed class FileNode : ObjectNode
    {

        /// <inheritdoc/>
        public override long Size => Data != null && Data.CanRead ? Data.Length : 0;

        /// <summary>
        /// The Actual Data for the file
        /// </summary>
        public Stream Data { get; set; }

        /// <summary>
        /// Initializes a new instance of the FileNode class with the specified name and data stream.
        /// </summary>
        /// <param name="name">The name of the file node.</param>
        /// <param name="stream">The data stream associated with the file node.</param>
        public FileNode(string name, Stream stream) : base(name)
        {
            Data = stream;
            Data.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Initializes a new instance of the FileNode class with information from the specified FileInfo object.
        /// </summary>
        /// <param name="info">The FileInfo object containing information about the file.</param>
        /// <param name="access">The FileAccess mode for opening the file.</param>
        /// <exception cref="ArgumentNullException">Thrown when the FileInfo object is null.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file specified by the FileInfo object does not exist.</exception>
        /// <exception cref="IOException">Thrown when an IO error occurs while opening the file stream.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when access to the file is denied.</exception>
        public FileNode(FileInfo info, FileAccess access = FileAccess.Read) : base(Path.GetFileName(info.FullName))
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info), "FileInfo cannot be null.");

            if (!info.Exists)
                throw new FileNotFoundException($"File '{info.FullName}' does not exist.");

            // Set basic file properties and open the file stream
            CreationTimeUtc = info.CreationTimeUtc;
            LastWriteTimeUtc = info.LastWriteTimeUtc;
            LastAccessTimeUtc = info.LastAccessTimeUtc;
            Data = new FileStream(info.FullName, FileMode.Open, access,FileShare.Read);
        }

        /// <inheritdoc/>
        public override ObjectNode Duplicate()
            => new FileNode(Name, new MemoryPoolStream(Data))
            {
                CreationTimeUtc = CreationTimeUtc,
                LastAccessTimeUtc = LastAccessTimeUtc,
                LastWriteTimeUtc = LastWriteTimeUtc,
                ID = ID,
                Properties = Properties,
            };

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Data?.Dispose();
                Data = null;
            }
        }
    }
}
