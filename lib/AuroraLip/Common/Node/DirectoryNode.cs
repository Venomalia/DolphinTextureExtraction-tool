using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;

namespace AuroraLib.Common.Node
{
    /// <summary>
    /// Represents a directory, capable of containing multiple <see cref="ObjectNode"/> like <see cref="DirectoryNode"/> or <see cref="FileNode"/>.
    /// </summary>
    [DebuggerDisplay("{Name}, Items:{Count}")]
    public class DirectoryNode : ObjectNode
    {
        /// <inheritdoc/>
        public override long Size => Objects.Sum(A => A.Value.Size);

        private readonly Dictionary<string, ObjectNode> Objects;

        /// <summary>
        /// Gets the number of archive objects contained within the current note.
        /// </summary>
        public int Count => Objects.Count;

        /// <summary>
        /// Gets a collection containing the name keys of the archive objects contained within the current note.
        /// </summary>
        public ICollection<string> Keys => Objects.Keys;

        /// <summary>
        /// Gets a collection containing the archive objects contained within the current note.
        /// </summary>
        public ICollection<ObjectNode> Values => Objects.Values;

        /// <summary>
        /// Initializes a new instance of the DirectoryNode class with the specified name and an empty list of items.
        /// </summary>
        /// <param name="name">The name of the directory node.</param>
        public DirectoryNode(string name) : base(name) => Objects = new();

        /// <summary>
        /// Initializes a new instance of the DirectoryNode class with information from the specified DirectoryInfo object.
        /// </summary>
        /// <param name="info">The DirectoryInfo object containing information about the directory.</param>
        /// <exception cref="ArgumentNullException">Thrown when the DirectoryInfo object is null.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified by the DirectoryInfo object does not exist.</exception>
        /// <exception cref="IOException">Thrown when an IO error occurs while accessing directory contents.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when access a the file is denied.</exception>
        public DirectoryNode(DirectoryInfo info) : this(info.Name)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info), "DirectoryInfo cannot be null.");

            if (!info.Exists)
                throw new DirectoryNotFoundException($"Directory '{info.FullName}' does not exist.");

            // Set basic directory properties
            CreationTimeUtc = info.CreationTimeUtc;
            LastWriteTimeUtc = info.LastWriteTimeUtc;
            LastAccessTimeUtc = info.LastAccessTimeUtc;

            try
            {
                foreach (FileSystemInfo sys in info.GetFileSystemInfos())
                {
                    if (sys is DirectoryInfo dir)
                        Add(new DirectoryNode(dir));
                    else if (sys is FileInfo file)
                        Add(new FileNode(file));
                }
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// Gets or sets the archive object at the specified path within the current note tree.
        /// </summary>
        /// <param name="path">The path of the archive object to get or set.</param>
        /// <returns>The archive object at the specified path, or null if no such object exists.</returns>
        /// <exception cref="InvalidOperationException">Thrown when an object with the same name already exists at the specified path and is not an Note.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the specified value is null.</exception>
        public ObjectNode this[string path]
        {
            get => TryGet(path, out ObjectNode item) ? item : null;
            set => AddPath(path, value);
        }

        /// <summary>
        /// Adds an archive object to the current note (automatically renames the node if the name is already assigned).
        /// </summary>
        /// <param name="value">The archive object to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if the specified value is null.</exception>
        public void Add(ObjectNode value)
        {
            if (!TryAdd(value))
            {
                ForceAdd(value);
            }
        }

        private void ForceAdd(ObjectNode value)
        {
            if (value.Parent != null)
                value = value.Duplicate();

            ReadOnlySpan<char> name = value.Name;
            int i = 1;

            do
            {
                value.Name = string.Concat(Path.GetFileNameWithoutExtension(name), $"~{i++}", Path.GetExtension(name));
            } while (!Objects.TryAdd(value.Name, value));

            value.SetParent(this);
            Events.NotificationEvent.Invoke(NotificationType.Warning, $"Node with the same name already exists and was renamed from {name} to {value.Name} for this reason.");
        }

        /// <summary>
        /// Tries to add an archive object to the current note.
        /// </summary>
        /// <param name="value">The archive object to add.</param>
        /// <returns>True if the archive object was added successfully, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the specified value is null.</exception>
        public bool TryAdd(ObjectNode value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "The specified value cannot be null.");

            if (Objects.ContainsKey(value.Name))
                return false;

            if (value.Parent != null)
                value = value.Duplicate();

            value.SetParent(this);
            return Objects.TryAdd(value.Name, value);

        }

        /// <summary>
        /// Adds an object to the current note tree at the specified path. If intermediate notes in the path do not exist, they are automatically created.
        /// </summary>
        /// <param name="path">The path where the archive object should be added.</param>
        /// <param name="value">The archive object to add.</param>
        /// <exception cref="InvalidOperationException">Thrown when an object with the same name already exists at the specified path and is not an Note.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the specified value is null.</exception>
        public void AddPath(ReadOnlySpan<char> path, ObjectNode value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "The specified value cannot be null.");
            int pos = path.IndexOf('\\');
            if (pos == -1)
            {
                pos = path.IndexOf('/');
                if (pos == -1)
                {
                    Add(value);
                    return;
                }
            }
            ReadOnlySpan<char> name = path[..pos++];
            if (name.SequenceEqual(".."))
            {
                if (Parent is not null)
                    Parent.AddPath(path[pos..], value);
                else
                    throw new InvalidOperationException("Parent note doesn't exist. Cannot navigate up in the hierarchy.");
            }
            else if (Objects.TryGetValue(name.ToString(), out ObjectNode sub))
            {
                if (sub is DirectoryNode note)
                    note.AddPath(path[pos..], value);
                else
                    throw new InvalidOperationException("An object with the same name already exists and is not an note.");
            }
            else
            {
                DirectoryNode note = new(name.ToString());
                Add(note);
                note.AddPath(path[pos..], value);
            }
        }

        /// <summary>
        /// Removes the archive object with the specified key from the current archive note.
        /// </summary>
        /// <param name="key">The key of the archive object to remove.</param>
        /// <returns>True if the archive object was successfully removed, otherwise false.</returns>
        public bool Remove(string key) => Remove(key, out _);

        /// <summary>
        /// Removes the specified archive object from the current archive note.
        /// </summary>
        /// <param name="value">The archive object to remove.</param>
        /// <returns>True if the archive object was successfully removed, otherwise false.</returns>
        public bool Remove(ObjectNode value) => Remove(value.Name, out _);

        /// <summary>
        /// Removes the archive object with the specified key from the current archive note and retrieves it.
        /// </summary>
        /// <param name="key">The key of the archive object to remove.</param>
        /// <param name="value">When this method returns, contains the removed archive object, if found; otherwise, null.</param>
        /// <returns>True if the archive object was successfully removed, otherwise false.</returns>
        public bool Remove(string key, [MaybeNullWhen(false)] out ObjectNode value)
        {
            if (Objects.Remove(key, out value))
            {
                value.SetParent(null);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes all objects from the current note and disposes them.
        /// </summary>
        public void Clear()
        {
            foreach (var item in Objects)
                item.Value?.Dispose();
            Objects.Clear();
        }

        /// <summary>
        /// Determines whether the current note contains a specific <see cref="ObjectNode"/>.
        /// </summary>
        /// <param name="item">The archive object to locate.</param>
        /// <returns>True if the <see cref="ObjectNode"/> is found within the current archive note; otherwise, false.</returns>
        public bool Contains(ObjectNode item) => Objects.ContainsKey(item.Name);

        /// <summary>
        /// Determines whether the current note tree contains an object with the specified path.
        /// </summary>
        /// <param name="path">The path to locate in the current note tree.</param>
        /// <returns>True if an archive object with the specified path is found; otherwise, false.</returns>
        public bool Contains(string path) => TryGet(path, out _);

        /// <summary>
        /// Tries to get the archive object with the specified path from the note tree.
        /// </summary>
        /// <param name="path">The path of the archive object to retrieve.</param>
        /// <param name="value">When this method returns, contains the archive object associated with the specified path, if found; otherwise, null.</param>
        /// <returns>True if the archive object with the specified path was found; otherwise, false.</returns>
        public bool TryGet(ReadOnlySpan<char> path, [MaybeNullWhen(false)] out ObjectNode value)
        {
            int pos = path.IndexOf('\\');
            if (pos == -1)
            {
                pos = path.IndexOf('/');
                if (pos == -1)
                    return Objects.TryGetValue(path.ToString(), out value);
            }

            ReadOnlySpan<char> name = path[..pos++];
            if (name.SequenceEqual("..") && Parent is not null)
            {
                return Parent.TryGet(path[pos..], out value);
            }
            else if (Objects.TryGetValue(name.ToString(), out value) && value is DirectoryNode note)
            {
                return note.TryGet(path[pos..], out value);
            }

            return false;
        }

        /// <summary>
        /// Searches for node objects of type <typeparamref name="T"/> whose names match the given pattern.
        /// </summary>
        /// <typeparam name="T">The type of objects to search for.</typeparam>
        /// <param name="pattern">The pattern to match against object names.</param>
        /// <param name="rekursiv">Whether to search recursively in subdirectories.</param>
        /// <returns>An enumerable of matching objects of type T.</returns>
        public IEnumerable<T> Search<T>(string pattern, bool rekursiv = true, RegexOptions options = RegexOptions.Singleline) where T : ObjectNode
        {
            List<T> results = new();
            // Convert the pattern to a regular expression
            string regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            Regex regex = new(regexPattern, options);

            Search(results, regex, rekursiv);
            return results;
        }

        private void Search<T>(List<T> results, Regex regex, bool rekursiv = true) where T : ObjectNode
        {
            foreach (var item in Objects)
            {
                if (item.Value is T data)
                {
                    string name = data.Name;
                    if (regex.IsMatch(name))
                    {
                        results.Add(data);
                    }
                }
                else if (rekursiv && item.Value is DirectoryNode dir)
                {
                    dir.Search(results, regex, rekursiv);
                }
            }
        }

        /// <summary>
        /// Retrieves all values of the specified type <typeparamref name="T"/> recursively from this node and its subdirectories.
        /// </summary>
        /// <typeparam name="T">The type of values to retrieve.</typeparam>
        /// <returns>An IEnumerable containing all values of type <typeparamref name="T"/>.</returns>
        public IEnumerable<T> GetAllValuesOf<T>() where T : ObjectNode
        {
            foreach (ObjectNode item in Values)
            {
                if (item is T returnNode)
                {
                    yield return returnNode;
                }
                else if (item is DirectoryNode dir)
                {
                    foreach (var subItem in dir.GetAllValuesOf<T>())
                        yield return subItem;
                }
            }
        }

        /// <summary>
        /// Counts the total number of items of the specified type <typeparamref name="T"/> recursively from this node and its subdirectories.
        /// </summary>
        /// <typeparam name="T">The type of items to count.</typeparam>
        /// <returns>The total count of items of type <typeparamref name="T"/>.</returns>
        public int TotalItemsCountOf<T>() where T : ObjectNode
        {
            int count = 0;

            foreach (ObjectNode item in Values)
            {
                if (item is T returnNode)
                    count++;
                if (item is DirectoryNode dir)
                    count += dir.TotalItemsCountOf<T>();
            }
            return count;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection of <seealso cref="ObjectNode"/> contained within the current note.
        /// </summary>
        /// <returns>An enumerator for the collection of archive objects.</returns>
        public IEnumerator<KeyValuePair<string, ObjectNode>> GetEnumerator() => Objects.GetEnumerator();

        /// <summary>
        /// Retrieves all <see cref="DirectoryNode"/> objects from this node.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="DirectoryNode"/>  objects.</returns>
        public IEnumerable<DirectoryNode> GetDirectories() => Objects.Values.Where(obj => obj is DirectoryNode).Cast<DirectoryNode>();

        /// <summary>
        /// Retrieves all <see cref="FileNode"/> objects from this node.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="FileNode"/> objects.</returns>
        public IEnumerable<FileNode> GetFiles() => Objects.Values.Where(obj => obj is FileNode).Cast<FileNode>();

        /// <inheritdoc/>
        public override ObjectNode Duplicate()
        {
            DirectoryNode dup = new(Name)
            {
                CreationTimeUtc = CreationTimeUtc,
                LastAccessTimeUtc = LastAccessTimeUtc,
                LastWriteTimeUtc = LastWriteTimeUtc,
                ID = ID,
                Properties = Properties,
            };
            foreach (var item in Objects)
            {
                dup.Add(item.Value.Duplicate());
            }
            return dup;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Clear();
        }
    }
}
