using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;

namespace AFSLib
{
    /// <summary>
    /// Class that represents one AFS archive. It can be created from scratch or loaded from a file or stream.
    /// </summary>
    public class AFS : IDisposable
    {
        /// <summary>
        /// Each of the entries in the AFS object.
        /// </summary>
        public ReadOnlyCollection<Entry> Entries => readonlyEntries;

        /// <summary>
        /// The header magic that the AFS file will have.
        /// </summary>
        public HeaderMagicType HeaderMagicType { get; set; }

        /// <summary>
        /// The location where the attributes info will be stored. Or if the file won't contain any attributes.
        /// </summary>
        public AttributesInfoType AttributesInfoType { get; set; }

        /// <summary>
        /// The amount of bytes the entry block will be aligned to. Minimum accepted value is 2048, which is what most games use. But some games have bigger values.
        /// </summary>
        public uint EntryBlockAlignment { get => entryBlockAlignment; set => entryBlockAlignment = Math.Max(value, MIN_ENTRY_BLOCK_ALIGNMENT_SIZE); }
        uint entryBlockAlignment;

        /// <summary>
        /// The amount of entries in this AFS object.
        /// </summary>
        public uint EntryCount => (uint)entries.Count;

        /// <summary>
        /// If the AFS object contains attributes or not. It will be false if AttributesInfoType == AttributesInfoType.NoAttributes.
        /// </summary>
        public bool ContainsAttributes => AttributesInfoType != AttributesInfoType.NoAttributes;

        /// <summary>
        /// Event that will be called each time some process wants to report something.
        /// </summary>
        public event NotifyProgressDelegate NotifyProgress;

        /// <summary>
        /// Represents the method that will handle the NotifyProgress event.
        /// </summary>
        /// <param name="type">Type of notification.</param>
        /// <param name="message">The notification message.</param>
        public delegate void NotifyProgressDelegate(NotificationType type, string message);

        internal const uint HEADER_MAGIC_00 = 0x00534641; // AFS
        internal const uint HEADER_MAGIC_20 = 0x20534641;
        internal const uint HEADER_SIZE = 0x8;
        internal const uint ENTRY_INFO_ELEMENT_SIZE = 0x8;
        internal const uint ATTRIBUTE_INFO_SIZE = 0x8;
        internal const uint ATTRIBUTE_ELEMENT_SIZE = 0x30;
        internal const uint MAX_ENTRY_NAME_LENGTH = 0x20;
        internal const uint MIN_ENTRY_BLOCK_ALIGNMENT_SIZE = 0x800;
        internal const uint ALIGNMENT_SIZE = 0x800;

        internal const string DUMMY_ENTRY_NAME_FOR_BLANK_NAME = "_NO_NAME";

        private readonly List<Entry> entries;
        private readonly ReadOnlyCollection<Entry> readonlyEntries;

        private Stream afsStream;
        private bool leaveAfsStreamOpen;

        private bool isDisposed;

        /// <summary>
        /// Create an empty AFS object.
        /// </summary>
        public AFS()
        {
            entries = new List<Entry>();
            readonlyEntries = entries.AsReadOnly();
            duplicates = new Dictionary<string, uint>();

            HeaderMagicType = HeaderMagicType.AFS_00;
            AttributesInfoType = AttributesInfoType.InfoAtBeginning;
            EntryBlockAlignment = 0x800;

            afsStream = null;
            leaveAfsStreamOpen = true;

            isDisposed = false;
        }

        /// <summary>
        /// Create an AFS object out of a stream. The stream will need to remain open until the AFS object is disposed, as it will need to access the stream's data during various operations.
        /// </summary>
        /// <param name="afsStream">Stream containing the AFS file data.</param>
        public AFS(Stream afsStream) : this()
        {
            if (afsStream == null)
            {
                throw new ArgumentNullException(nameof(afsStream));
            }

            LoadFromStream(afsStream);
            leaveAfsStreamOpen = true;
        }

        /// <summary>
        /// Create an AFS object out of a file. The file will remain open until the AFS object is disposed.
        /// </summary>
        /// <param name="afsFilePath">Path to the AFS file containing the data.</param>
        public AFS(string afsFilePath) : this()
        {
            if (string.IsNullOrEmpty(afsFilePath))
            {
                throw new ArgumentNullException(nameof(afsFilePath));
            }

            if (!File.Exists(afsFilePath))
            {
                throw new FileNotFoundException($"File \"{afsFilePath}\" has not been found.", afsFilePath);
            }

            LoadFromStream(File.OpenRead(afsFilePath));
            leaveAfsStreamOpen = false;
        }

        /// <summary>
        /// Dispose the AFS object.
        /// </summary>
        public void Dispose()
        {
            CheckDisposed();

            if (afsStream != null && !leaveAfsStreamOpen)
            {
                afsStream.Dispose();
                afsStream = null;
                leaveAfsStreamOpen = true;
            }

            entries.Clear();
            duplicates.Clear();

            isDisposed = true;
        }

        /// <summary>
        /// Saves the contents of this AFS object into a file.
        /// </summary>
        /// <param name="outputFilePath">The path to the file where the data is going to be saved.</param>
        public void SaveToFile(string outputFilePath)
        {
            if (string.IsNullOrEmpty(outputFilePath))
            {
                throw new ArgumentNullException(nameof(outputFilePath));
            }

            using (FileStream outputStream = File.Create(outputFilePath))
            {
                SaveToStream(outputStream);
            }
        }

        /// <summary>
        /// Saves the contents of this AFS object into a stream.
        /// </summary>
        /// <param name="outputStream">The stream where the data is going to be saved.</param>
        public void SaveToStream(Stream outputStream)
        {
            CheckDisposed();

            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }

            if (outputStream == afsStream)
            {
                throw new ArgumentException("Can't save into the same stream the AFS data is being read from.", nameof(outputStream));
            }

            // Start creating the AFS file

            NotifyProgress?.Invoke(NotificationType.Info, "Creating AFS stream...");

            using (BinaryWriter bw = new BinaryWriter(outputStream))
            {
                bw.Write(HeaderMagicType == HeaderMagicType.AFS_20 ? HEADER_MAGIC_20 : HEADER_MAGIC_00);
                bw.Write(EntryCount);

                // Calculate the offset of each entry

                uint[] offsets = new uint[EntryCount];

                uint firstEntryOffset = Utils.Pad(HEADER_SIZE + (ENTRY_INFO_ELEMENT_SIZE * EntryCount) + ATTRIBUTE_INFO_SIZE, EntryBlockAlignment);
                uint currentEntryOffset = firstEntryOffset;

                for (int e = 0; e < EntryCount; e++)
                {
                    if (entries[e] is NullEntry)
                    {
                        offsets[e] = 0;
                    }
                    else
                    {
                        offsets[e] = currentEntryOffset;

                        DataEntry dataEntry = entries[e] as DataEntry;
                        currentEntryOffset += dataEntry.Size;
                        currentEntryOffset = Utils.Pad(currentEntryOffset, ALIGNMENT_SIZE);
                    }
                }

                // Write entries info

                for (int e = 0; e < EntryCount; e++)
                {
                    NotifyProgress?.Invoke(NotificationType.Info, $"Writing entry info... {e + 1}/{EntryCount}");

                    if (entries[e] is NullEntry)
                    {
                        bw.Write((uint)0);
                        bw.Write((uint)0);
                    }
                    else
                    {
                        DataEntry dataEntry = entries[e] as DataEntry;

                        bw.Write(offsets[e]);
                        bw.Write(dataEntry.Size);
                    }
                }

                // Write attributes info if available

                outputStream.Position = HEADER_SIZE + (EntryCount * ENTRY_INFO_ELEMENT_SIZE);
                Utils.FillStreamWithZeroes(outputStream, firstEntryOffset - (uint)outputStream.Position);

                uint attributesInfoOffset = currentEntryOffset;

                if (ContainsAttributes)
                {
                    if (AttributesInfoType == AttributesInfoType.InfoAtBeginning)
                        outputStream.Position = HEADER_SIZE + (EntryCount * ENTRY_INFO_ELEMENT_SIZE);
                    else if (AttributesInfoType == AttributesInfoType.InfoAtEnd)
                        outputStream.Position = firstEntryOffset - ATTRIBUTE_INFO_SIZE;

                    bw.Write(attributesInfoOffset);
                    bw.Write(EntryCount * ATTRIBUTE_ELEMENT_SIZE);
                }

                // Write entries data to stream

                for (int e = 0; e < EntryCount; e++)
                {
                    if (entries[e] is NullEntry)
                    {
                        NotifyProgress?.Invoke(NotificationType.Info, $"Null file... {e + 1}/{EntryCount}");
                    }
                    else
                    {
                        NotifyProgress?.Invoke(NotificationType.Info, $"Writing entry... {e + 1}/{EntryCount}");

                        outputStream.Position = offsets[e];

                        using (Stream entryStream = entries[e].GetStream())
                        {
                            entryStream.CopyTo(outputStream);
                        }
                    }
                }

                // Write attributes if available

                if (ContainsAttributes)
                {
                    outputStream.Position = attributesInfoOffset;

                    for (int e = 0; e < EntryCount; e++)
                    {
                        if (entries[e] is NullEntry)
                        {
                            NotifyProgress?.Invoke(NotificationType.Info, $"Null file... {e + 1}/{EntryCount}");

                            outputStream.Position += ATTRIBUTE_ELEMENT_SIZE;
                        }
                        else
                        {
                            NotifyProgress?.Invoke(NotificationType.Info, $"Writing attribute... {e + 1}/{EntryCount}");

                            DataEntry dataEntry = entries[e] as DataEntry;

                            byte[] name = Encoding.Default.GetBytes(dataEntry.Name);
                            outputStream.Write(name, 0, name.Length);
                            outputStream.Position += MAX_ENTRY_NAME_LENGTH - name.Length;

                            bw.Write((ushort)dataEntry.LastWriteTime.Year);
                            bw.Write((ushort)dataEntry.LastWriteTime.Month);
                            bw.Write((ushort)dataEntry.LastWriteTime.Day);
                            bw.Write((ushort)dataEntry.LastWriteTime.Hour);
                            bw.Write((ushort)dataEntry.LastWriteTime.Minute);
                            bw.Write((ushort)dataEntry.LastWriteTime.Second);
                            bw.Write(dataEntry.UnknownAttribute);
                        }
                    }
                }

                // Pad final zeroes

                uint currentPosition = (uint)outputStream.Position;
                uint endOfFile = Utils.Pad(currentPosition, ALIGNMENT_SIZE);
                Utils.FillStreamWithZeroes(outputStream, endOfFile - currentPosition);

                // Make sure the stream is the size of the AFS data (in case the stream was bigger)

                outputStream.SetLength(endOfFile);
            }

            NotifyProgress?.Invoke(NotificationType.Success, "AFS stream has been saved successfully.");
        }

        /// <summary>
        /// Adds a new entry from a file.
        /// </summary>
        /// <param name="fileNamePath">Path to the file that will be added.</param>
        /// <param name="entryName">The name of the entry. If null, it will be the name of the file in fileNamePath.</param>
        /// <returns>A reference to the added entry.</returns>
        public FileEntry AddEntryFromFile(string fileNamePath, string entryName = null)
        {
            CheckDisposed();

            if (string.IsNullOrEmpty(fileNamePath))
            {
                throw new ArgumentNullException(nameof(fileNamePath));
            }

            if (!File.Exists(fileNamePath))
            {
                throw new FileNotFoundException($"File \"{fileNamePath}\" has not been found.", fileNamePath);
            }

            if (entryName == null)
            {
                entryName = Path.GetFileName(fileNamePath);
            }

            FileEntry fileEntry = new FileEntry(fileNamePath, entryName);
            Internal_AddEntry(fileEntry);
            UpdateSanitizedEntriesNames();

            return fileEntry;
        }

        /// <summary>
        /// Adds a new entry from a stream.
        /// </summary>
        /// <param name="entryStream">Stream that contains the file that will be added.</param>
        /// <param name="entryName">The name of the entry. If null, it will be considered as string.Empty.</param>
        /// <returns>A reference to the added entry.</returns>
        public StreamEntry AddEntryFromStream(Stream entryStream, string entryName)
        {
            CheckDisposed();

            if (entryStream == null)
            {
                throw new ArgumentNullException(nameof(entryStream));
            }

            if (entryName == null)
            {
                entryName = string.Empty;
            }

            StreamEntryInfo info = new StreamEntryInfo()
            {
                Offset = 0,
                Name = entryName,
                Size = (uint)entryStream.Length,
                LastWriteTime = DateTime.Now,
                UnknownAttribute = (uint)entryStream.Length
            };

            StreamEntry streamEntry = new StreamEntry(entryStream, info);
            Internal_AddEntry(streamEntry);
            UpdateSanitizedEntriesNames();

            return streamEntry;
        }

        /// <summary>
        /// Adds a new null entry.
        /// </summary>
        /// <returns>A reference to the added entry.</returns>
        public NullEntry AddNullEntry()
        {
            CheckDisposed();

            NullEntry nullEntry = new NullEntry();
            Internal_AddEntry(nullEntry);
            UpdateSanitizedEntriesNames();

            return nullEntry;
        }

        /// <summary>
        /// Adds all files in the specified directory to the AFS object.
        /// </summary>
        /// <param name="directory">The path to the directory.</param>
        /// <param name="recursiveSearch">When true, it adds all files in the specified directory and its subdirectories. When false, it ignores any subdirectories.</param>
        public void AddEntriesFromDirectory(string directory, bool recursiveSearch = false)
        {
            CheckDisposed();

            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentNullException(nameof(directory));
            }

            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"Directory \"{directory}\" has not been found.");
            }

            string[] files = Directory.GetFiles(directory, "*.*", recursiveSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            for (int f = 0; f < files.Length; f++)
            {
                string entryName = Path.GetFileName(files[f]);
                Entry entry = new FileEntry(files[f], entryName);
                Internal_AddEntry(entry);
            }

            UpdateSanitizedEntriesNames();
        }

        /// <summary>
        /// Removes an entry from the AFS object.
        /// </summary>
        /// <param name="entry">The entry to remove.</param>
        public void RemoveEntry(Entry entry)
        {
            CheckDisposed();

            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            if (entries.Contains(entry))
            {
                Internal_RemoveEntry(entry);
                UpdateSanitizedEntriesNames();
            }
        }

        /// <summary>
        /// Extracts one entry to a file.
        /// </summary>
        /// <param name="entry">The entry to extract.</param>
        /// <param name="outputFilePath">The path to the file where the entry will be saved. If it doesn't exist, it will be created.</param>
        public void ExtractEntryToFile(Entry entry, string outputFilePath)
        {
            CheckDisposed();

            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            if (entry is NullEntry)
            {
                NotifyProgress?.Invoke(NotificationType.Warning, $"Trying to extract a null entry. Ignored.");
                return;
            }

            if (string.IsNullOrEmpty(outputFilePath))
            {
                throw new ArgumentNullException(nameof(outputFilePath));
            }

            string directory = Path.GetDirectoryName(outputFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (FileStream outputStream = File.Create(outputFilePath))
            using (Stream entryStream = entry.GetStream())
            {
                entryStream.CopyTo(outputStream);
            }

            if (ContainsAttributes)
            {
                DataEntry dataEntry = entry as DataEntry;

                try
                {
                    File.SetLastWriteTime(outputFilePath, dataEntry.LastWriteTime);
                }
                catch (ArgumentOutOfRangeException)
                {
                    File.SetLastWriteTime(outputFilePath, DateTime.Now);
                    NotifyProgress?.Invoke(NotificationType.Warning, "Invalid date/time. Setting current date/time.");
                }
            }
        }

        /// <summary>
        /// Extracts all the entries from the AFS object.
        /// </summary>
        /// <param name="outputDirectory">The directory where the entries will be saved. If it doesn't exist, it will be created.</param>
        public void ExtractAllEntriesToDirectory(string outputDirectory)
        {
            CheckDisposed();

            if (string.IsNullOrEmpty(outputDirectory))
            {
                throw new ArgumentNullException(nameof(outputDirectory));
            }

            if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

            for (int e = 0; e < EntryCount; e++)
            {
                if (entries[e] is NullEntry)
                {
                    NotifyProgress?.Invoke(NotificationType.Warning, $"Null entry. Skipping... {e + 1}/{EntryCount}");
                    continue;
                }

                NotifyProgress?.Invoke(NotificationType.Info, $"Extracting entry... {e + 1}/{EntryCount}");

                DataEntry dataEntry = entries[e] as DataEntry;

                string outputFilePath = Path.Combine(outputDirectory, dataEntry.SanitizedName);
                if (File.Exists(outputFilePath))
                {
                    NotifyProgress?.Invoke(NotificationType.Warning, $"File \"{outputFilePath}\" already exists. Overwriting...");
                }

                ExtractEntryToFile(entries[e], outputFilePath);
            }

            NotifyProgress?.Invoke(NotificationType.Success, $"Finished extracting all entries successfully.");
        }

        private void LoadFromStream(Stream afsStream)
        {
            this.afsStream = afsStream;

            using (BinaryReader br = new BinaryReader(afsStream, Encoding.UTF8, true))
            {
                // Check if the Magic is valid

                uint magic = br.ReadUInt32();

                if (magic == HEADER_MAGIC_00)
                {
                    HeaderMagicType = HeaderMagicType.AFS_00;
                }
                else if (magic == HEADER_MAGIC_20)
                {
                    HeaderMagicType = HeaderMagicType.AFS_20;
                }
                else
                {
                    throw new InvalidDataException("Stream doesn't seem to contain valid AFS data.");
                }

                // Start gathering info about entries and attributes

                uint entryCount = br.ReadUInt32();
                StreamEntryInfo[] entriesInfo = new StreamEntryInfo[entryCount];

                uint entryBlockStartOffset = 0;
                uint entryBlockEndOffset = 0;

                for (int e = 0; e < entryCount; e++)
                {
                    entriesInfo[e].Offset = br.ReadUInt32();
                    entriesInfo[e].Size = br.ReadUInt32();

                    if (entriesInfo[e].IsNull)
                    {
                        continue;
                    }

                    if (entryBlockStartOffset == 0) entryBlockStartOffset = entriesInfo[e].Offset;
                    entryBlockEndOffset = entriesInfo[e].Offset + entriesInfo[e].Size;
                }

                // Calculate the entry block alignment

                uint alignment = MIN_ENTRY_BLOCK_ALIGNMENT_SIZE;
                uint endInfoBlockOffset = (uint)afsStream.Position + ATTRIBUTE_INFO_SIZE;
                while (endInfoBlockOffset + alignment < entryBlockStartOffset) alignment <<= 1;
                EntryBlockAlignment = alignment;

                // Find where attribute info is located

                AttributesInfoType = AttributesInfoType.NoAttributes;

                uint attributeDataOffset = br.ReadUInt32();
                uint attributeDataSize = br.ReadUInt32();

                bool isAttributeInfoValid = IsAttributeInfoValid(attributeDataOffset, attributeDataSize, (uint)afsStream.Length, entryBlockEndOffset);

                if (isAttributeInfoValid)
                {
                    AttributesInfoType = AttributesInfoType.InfoAtBeginning;
                }
                else
                {
                    afsStream.Position = entryBlockStartOffset - ATTRIBUTE_INFO_SIZE;
                    attributeDataOffset = br.ReadUInt32();
                    attributeDataSize = br.ReadUInt32();

                    isAttributeInfoValid = IsAttributeInfoValid(attributeDataOffset, attributeDataSize, (uint)afsStream.Length, entryBlockEndOffset);

                    if (isAttributeInfoValid)
                    {
                        AttributesInfoType = AttributesInfoType.InfoAtEnd;
                    }
                }

                // Read attribute data if there is any

                if (ContainsAttributes)
                {
                    afsStream.Position = attributeDataOffset;

                    for (int e = 0; e < entryCount; e++)
                    {
                        if (entriesInfo[e].IsNull)
                        {
                            // It's a null entry, so ignore attribute data

                            afsStream.Position += ATTRIBUTE_ELEMENT_SIZE;

                            continue;
                        }
                        else
                        {
                            // It's a valid entry, so read attribute data

                            byte[] name = new byte[MAX_ENTRY_NAME_LENGTH];
                            afsStream.Read(name, 0, name.Length);

                            entriesInfo[e].Name = Utils.GetStringFromBytes(name);

                            try
                            {
                                entriesInfo[e].LastWriteTime = new DateTime(br.ReadUInt16(), br.ReadUInt16(), br.ReadUInt16(), br.ReadUInt16(), br.ReadUInt16(), br.ReadUInt16());
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                entriesInfo[e].LastWriteTime = default;
                                NotifyProgress?.Invoke(NotificationType.Warning, "Invalid date/time. Ignoring.");
                            }

                            entriesInfo[e].UnknownAttribute = br.ReadUInt32();
                        }
                    }
                }
                else
                {
                    for (int e = 0; e < entryCount; e++)
                    {
                        entriesInfo[e].Name = $"{e:00000000}";
                    }
                }

                // After gathering all necessary info, create the entries.

                for (int e = 0; e < entryCount; e++)
                {
                    Entry entry;
                    if (entriesInfo[e].IsNull) entry = new NullEntry();
                    else entry = new StreamEntry(afsStream, entriesInfo[e]);
                    Internal_AddEntry(entry);
                }

                UpdateSanitizedEntriesNames();
            }
        }

        private void Internal_AddEntry(Entry entry)
        {
            entries.Add(entry);

            DataEntry dataEntry = entry as DataEntry;
            if (dataEntry != null)
            {
                dataEntry.NameChanged += UpdateSanitizedEntriesNames;
            }
        }

        private void Internal_RemoveEntry(Entry entry)
        {
            DataEntry dataEntry = entry as DataEntry;
            if (dataEntry != null)
            {
                dataEntry.NameChanged -= UpdateSanitizedEntriesNames;
            }

            entries.Remove(entry);
        }

        private bool IsAttributeInfoValid(uint attributesOffset, uint attributesSize, uint afsFileSize, uint dataBlockEndOffset)
        {
            // If zeroes are found, info is not valid.
            if (attributesOffset == 0) return false;
            if (attributesSize == 0) return false;

            // Check if this info makes sense, as there are times where random
            // data can be found instead of attribute offset and size.
            if (attributesSize > afsFileSize - dataBlockEndOffset) return false;
            if (attributesSize < EntryCount * ATTRIBUTE_ELEMENT_SIZE) return false;
            if (attributesOffset < dataBlockEndOffset) return false;
            if (attributesOffset > afsFileSize - attributesSize) return false;

            // If the above conditions are not met, it looks like it's valid attribute data
            return true;
        }

        private void CheckDisposed()
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().Name);
        }

        #region Name sanitization

        private readonly Dictionary<string, uint> duplicates;

        private void UpdateSanitizedEntriesNames()
        {
            // There can be multiple files with the same name, so keep track of duplicates

            duplicates.Clear();

            for (int e = 0; e < EntryCount; e++)
            {
                if (entries[e] is NullEntry) continue;

                DataEntry dataEntry = entries[e] as DataEntry;

                string sanitizedName = SanitizeName(dataEntry.Name);

                bool found = duplicates.TryGetValue(sanitizedName, out uint duplicateCount);

                if (found) duplicates[sanitizedName] = ++duplicateCount;
                else duplicates.Add(sanitizedName, 0);

                if (duplicateCount > 0)
                {
                    string nameWithoutExtension = Path.ChangeExtension(sanitizedName, null);
                    string nameDuplicate = $" ({duplicateCount})";
                    string nameExtension = Path.GetExtension(sanitizedName);

                    sanitizedName = nameWithoutExtension + nameDuplicate + nameExtension;
                }

                dataEntry.SanitizedName = sanitizedName;
            }
        }

        private static string SanitizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                // The game "Winback 2: Project Poseidon" has attributes with empty file names.
                // Give the files a dummy name for them to extract properly.
                return DUMMY_ENTRY_NAME_FOR_BLANK_NAME;
            }

            // There are some cases where instead of a file name, an AFS file will store a path, like in Soul Calibur 2 or Crimson Tears.
            // Let's make sure there aren't any invalid characters in the path so the OS doesn't complain.

            string sanitizedName = name;

            for (int ipc = 0; ipc < invalidPathChars.Length; ipc++)
            {
                sanitizedName = sanitizedName.Replace(invalidPathChars[ipc], string.Empty);
            }

            // Also remove any ":" in case there are drive letters in the path (like, again, in Soul Calibur 2)

            sanitizedName = sanitizedName.Replace(":", string.Empty);

            return sanitizedName;
        }

        #endregion

        #region Statics

        private static readonly string[] invalidPathChars;
        private static readonly string[] invalidFileNameChars;

        static AFS()
        {
            char[] pChars = Path.GetInvalidPathChars();
            char[] fChars = Path.GetInvalidFileNameChars();

            invalidPathChars = new string[pChars.Length];
            for (int ipc = 0; ipc < pChars.Length; ipc++)
            {
                invalidPathChars[ipc] = pChars[ipc].ToString();
            }

            invalidFileNameChars = new string[fChars.Length];
            for (int ifc = 0; ifc < fChars.Length; ifc++)
            {
                invalidFileNameChars[ifc] = fChars[ifc].ToString();
            }
        }

        /// <summary>
        /// Get the version of AFSLib.
        /// </summary>
        public static Version GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        #endregion

        #region Deprecated

        /// <summary>
        /// Adds a new entry from a file.
        /// </summary>
        /// <param name="entryName">The name of the entry.</param>
        /// <param name="fileNamePath">Path to the file that will be added.</param>
        [Obsolete("This method is deprecated since version 2.0.0, please use AddEntryFromFile(string, string) instead.")]
        public void AddEntry(string entryName, string fileNamePath)
        {
            AddEntryFromFile(fileNamePath, entryName);
        }

        /// <summary>
        /// Adds a new entry from a stream.
        /// </summary>
        /// <param name="entryName">The name of the entry.</param>
        /// <param name="entryStream">Stream that contains the file that will be added.</param>
        [Obsolete("This method is deprecated since version 2.0.0, please use AddEntryFromStream(Stream, string) instead.")]
        public void AddEntry(string entryName, Stream entryStream)
        {
            AddEntryFromStream(entryStream, entryName);
        }

        /// <summary>
        /// Extracts one entry to a file.
        /// </summary>
        /// <param name="entry">The entry to extract.</param>
        /// <param name="outputFilePath">The path to the file where the entry will be saved. If it doesn't exist, it will be created.</param>
        [Obsolete("This method is deprecated since version 2.0.0, please use ExtractEntryToFile(Entry, string) instead.")]
        public void ExtractEntry(Entry entry, string outputFilePath)
        {
            ExtractEntryToFile(entry, outputFilePath);
        }

        /// <summary>
        /// Extracts all the entries from the AFS object.
        /// </summary>
        /// <param name="outputDirectory">The directory where the entries will be saved. If it doesn't exist, it will be created.</param>
        [Obsolete("This method is deprecated since version 2.0.0, please use ExtractAllEntriesToDirectory(string) instead.")]
        public void ExtractAllEntries(string outputDirectory)
        {
            ExtractAllEntriesToDirectory(outputDirectory);
        }

        #endregion
    }
}