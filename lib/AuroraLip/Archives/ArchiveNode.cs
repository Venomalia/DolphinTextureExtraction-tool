using AuroraLib.Common.Node;
using AuroraLib.Common.Node.Interfaces;

namespace AuroraLib.Archives
{
    /// <summary>
    /// The base class for Archive like systems
    /// </summary>
    public abstract class ArchiveNode : DirectoryNode, IBinaryObjectNode
    {
        /// <inheritdoc/>
        public virtual bool CanRead => true;

        /// <inheritdoc/>
        public abstract bool CanWrite { get; }

        private Stream RefStream;

        public ArchiveNode() : base("null")
        {
        }

        public ArchiveNode(string name) : base(name)
        {
        }

        public ArchiveNode(FileNode source) : base(source.Name)
            => BinaryDeserialize(source);

        /// <summary>
        /// Deserializes binary data for this node from the specified stream.
        /// </summary>
        /// <param name="source">The stream containing the binary data to deserialize.</param>
        public void BinaryDeserialize(Stream source)
        {
            RefStream?.Dispose();
            RefStream = source;
            Clear();
            Deserialize(source);
        }

        /// <inheritdoc/>
        public void BinaryDeserialize(FileNode source)
        {
            Name = source.Name;
            CreationTimeUtc = source.CreationTimeUtc;
            LastWriteTimeUtc = source.LastWriteTimeUtc;
            ID = source.ID;
            Properties = source.Properties;
            if (source.Parent != null)
            {
                DirectoryNode parent = source.Parent;
                parent.Remove(source);
                parent.Add(this);
            }
            try
            {
                Clear();
                Deserialize(source.Data);
            }
            catch (Exception)
            {
                DirectoryNode parent = Parent;
                parent.Remove(this);
                parent.Add(source);
                throw;
            }
        }


        /// <summary>
        /// Serializes this node to the specified stream.
        /// </summary>
        /// <param name="dest">The stream to which the binary data will be serialized.</param>
        public void BinarySerialize(Stream dest) => Serialize(dest);

        /// <inheritdoc/>
        public FileNode BinarySerialize()
        {
            if (Parent == null)
                throw new NullReferenceException(nameof(Parent));

            FileNode file = new(Name, new MemoryPoolStream())
            {
                CreationTimeUtc = CreationTimeUtc,
                LastWriteTimeUtc = LastWriteTimeUtc,
                ID = ID,
                Properties = Properties
            };
            BinarySerialize(file.Data);

            if (Parent != null)
            {
                DirectoryNode parent = Parent;
                parent.Remove(this);
                parent.Add(file);
            }
            file.Data.Seek(0, SeekOrigin.Begin);
            return file;
        }

        protected bool TryGetRefFile(string path, out FileNode file)
        {
            if (Parent is not null && Parent.TryGet(path, out ObjectNode refNode) && refNode is FileNode refFile)
            {
                file = refFile;
                refFile.Data.Position = 0;
                return true;
            }
            else if (RefStream is FileStream fileStream)
            {
                string directory = Path.GetDirectoryName(fileStream.Name);
                if (directory != null)
                {
                    string fullPath = Path.Combine(directory, path);
                    if (File.Exists(fullPath))
                    {
                        file = new FileNode(new FileInfo(fullPath));
                        return true;
                    }
                }
            }
            file = null;
            return false;
        }

        /// <inheritdoc/>
        public abstract bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default);
        protected abstract void Deserialize(Stream source);
        protected abstract void Serialize(Stream dest);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                RefStream?.Dispose();
                RefStream = null;
            }
        }

    }
}
