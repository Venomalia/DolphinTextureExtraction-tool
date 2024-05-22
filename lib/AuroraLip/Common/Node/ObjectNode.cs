using AuroraLib.Core.Interfaces;
using System.Diagnostics;
using System.Text;

namespace AuroraLib.Common.Node
{
    /// <summary>
    /// Represents an abstract base object for Archive like systems.
    /// </summary>
    [DebuggerDisplay("{Name}, Size:{Size}")]
    public abstract class ObjectNode : IDisposable, IObjectName, IDataTime
    {
        /// <summary>
        /// Gets or sets the name of the object.
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value), "Name cannot be null or empty.");
                if (value.AsSpan().Contains('/') || value.AsSpan().Contains('\\'))
                    throw new ArgumentException("Name cannot contain directory separator characters.", nameof(value));
                if (value == "..")
                    throw new ArgumentException("Name cannot be \"..\".", nameof(value));

                if (Parent != null)
                {
                    if (!Parent.Contains(value))
                    {
                        DirectoryNode parent = Parent;
                        parent.Remove(this);
                        name = value;
                        parent.Add(this);
                    }
                }
                else
                {
                    name = value;
                }
            }
        }
        private string name;

        /// <summary>
        /// Gets or sets the extension of this node.
        /// </summary>
        public ReadOnlySpan<char> Extension
        {
            get => Path.GetExtension(Name.AsSpan());
            set => Name = string.Concat(Path.GetFileNameWithoutExtension(Name.AsSpan()), value);
        }

        /// <inheritdoc/>
        public DateTime CreationTimeUtc
        {
            get => creationTimeUtc;
            set => creationTimeUtc = lastWriteTimeUtc = LastAccessTimeUtc = value;
        }
        private DateTime creationTimeUtc;

        /// <inheritdoc/>
        public DateTime LastWriteTimeUtc
        {
            get => lastWriteTimeUtc;
            set => lastWriteTimeUtc = LastAccessTimeUtc = value;
        }
        private DateTime lastWriteTimeUtc;

        /// <inheritdoc/>
        public DateTime LastAccessTimeUtc { get; set; }

        /// <summary>
        /// Gets the data size of this object.
        /// </summary>
        public abstract long Size { get; }

        public ulong ID { get; set; }

        public string Properties { get; set; }

        /// <summary>
        /// Gets the parent <see cref="DirectoryNode"/> of the current object, or null if the parent is not set.</returns>
        /// </summary>
        public DirectoryNode Parent { get; private set; }

        protected ObjectNode(string name)
        {
            Name = name;
            CreationTimeUtc = LastWriteTimeUtc = LastAccessTimeUtc = DateTime.UtcNow;
            ID = 0;
            Properties = string.Empty;
        }

        internal virtual void SetParent(DirectoryNode value) => Parent = value;

        /// <summary>
        /// Gets the root <see cref="DirectoryNode"/> of the current object.
        /// </summary>
        /// <returns>The root <see cref="DirectoryNode"/> of the current object.</returns>
        public ObjectNode GetRoot()
        {
            if (Parent == null)
                return this;
            return Parent.GetRoot();
        }

        /// <summary>
        /// Gets the full path of the current object.
        /// </summary>
        /// <returns>The full path of the current archive object.</returns>
        public string GetFullPath()
        {
            StringBuilder path = new();
            GetFullPath(path);
            return path.ToString();
        }

        /// <summary>
        /// Appends the full path of the current object to the provided StringBuilder.
        /// </summary>
        /// <param name="PathBuilder">The StringBuilder to append the full path to.</param>
        public void GetFullPath(StringBuilder PathBuilder)
        {
            if (Parent != null)
            {
                int i = PathBuilder.Length;
                Parent.GetFullPath(PathBuilder);
                if (i != PathBuilder.Length)
                    PathBuilder.Append(Path.DirectorySeparatorChar);
            }
            PathBuilder.Append(Name);
        }

        /// <summary>
        /// Moves the node to a new parent directory.
        /// </summary>
        /// <param name="newParent">The new parent directory node.</param>
        /// <exception cref="ArgumentNullException">Thrown when the newNode parameter is null.</exception>
        public void MoveTo(DirectoryNode newParent)
        {
            if (newParent == null)
                throw new ArgumentNullException(nameof(newParent), "The new parent directory node cannot be null.");

            if (newParent == Parent)
                return;

            Parent?.Remove(this);
            newParent.Add(this);
        }

        /// <summary>
        /// Creates a duplicate of the current object.
        /// </summary>
        /// <returns>A duplicate of the current object.</returns>
        public abstract ObjectNode Duplicate();

        /// <summary>
        /// Releases the resources used by the object.
        /// </summary>
        /// <param name="disposing">true if the method is called directly, false if called by the finalizer.</param>
        protected abstract void Dispose(bool disposing);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            Parent = null;
            GC.SuppressFinalize(this);
        }
    }
}
