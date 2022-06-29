using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hack.io.Util
{
    /// <summary>
    /// The base class for Archive like systems
    /// </summary>
    public abstract class Archive
    {
        /// <summary>
        /// Filename of this Archive.
        /// <para/>Set using <see cref="Save(string)"/>;
        /// </summary>
        public string FileName { get; protected set; } = null;
        /// <summary>
        /// Get the name of the archive without the path
        /// </summary>
        public string Name { get { return FileName == null ? FileName : new FileInfo(FileName).Name; } }
        /// <summary>
        /// The Root Directory of the Archive
        /// </summary>
        public ArchiveDirectory Root { get; set; }
        /// <summary>
        /// The total amount of files inside this archive.
        /// </summary>
        public int TotalFileCount => Root?.GetCountAndChildren() ?? 0;

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
            FileName = filepath;
            FileStream fs = new FileStream(filepath, FileMode.Create);
            Save(fs);
            fs.Close();
        }
        /// <summary>
        /// Write the Archive to a Stream
        /// </summary>
        /// <param name="RARCFile"></param>
        public void Save(Stream RARCFile) => Write(RARCFile);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            int Count = Root?.GetCountAndChildren() ?? 0;
            return $"{new FileInfo(FileName).Name} - {Count} File{(Count > 1 ? "s" : "")} total";
        }
        #endregion

        #region File Functions
        /// <summary>
        /// Get or Set a file based on a path. When setting, if the file doesn't exist, it will be added (Along with any missing subdirectories). Set the file to null to delete it
        /// </summary>
        /// <param name="Path">The Path to take. Does not need the Root name to start, but cannot start with a '/'</param>
        /// <returns></returns>
        public object this[string Path]
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
        /// <param name="Path">The path to take</param>
        /// <param name="IgnoreCase">Ignore casing of the file</param>
        /// <returns>false if the Item isn't found</returns>
        public bool ItemExists(string Path, bool IgnoreCase = false)
        {
            if (Path.StartsWith(Root.Name + "/"))
                Path = Path.Substring(Root.Name.Length + 1);
            return Root.ItemExists(Path, IgnoreCase);
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
        public void ClearAll() { Root.Clear(); }
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
        #endregion

        /// <summary>
        /// Create an Archive from a Folder
        /// </summary>
        /// <param name="Folderpath">Folder to make an archive from</param>
        public void Import(string Folderpath) => Root = NewDirectory(Folderpath, this);
        /// <summary>
        /// Dump the contents of this archive to a folder
        /// </summary>
        /// <param name="FolderPath">The Path to save to. Should be a folder</param>
        /// <param name="Overwrite">If there are contents already at the chosen location, delete them?</param>
        public virtual void Export(string FolderPath, bool Overwrite = false)
        {
            FolderPath = Path.Combine(FolderPath, Root.Name);
            if (Directory.Exists(FolderPath))
            {
                if (Overwrite)
                {
                    Directory.Delete(FolderPath, true);
                    Directory.CreateDirectory(FolderPath);
                }
                else
                    throw new Exception("Target directory is occupied");
            }
            else
                Directory.CreateDirectory(FolderPath);

            Root.Export(FolderPath);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual ArchiveDirectory NewDirectory() => new ArchiveDirectory();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Owner"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected virtual ArchiveDirectory NewDirectory(Archive Owner, ArchiveDirectory parent) => new ArchiveDirectory(Owner, parent);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="Owner"></param>
        /// <returns></returns>
        protected virtual ArchiveDirectory NewDirectory(string filename, Archive Owner) => new ArchiveDirectory(filename, Owner);

    }

    /// <summary>
    /// Folder contained inside the Archive. Can contain more <see cref="ArchiveDirectory"/>s if desired, as well as <see cref="ArchiveFile"/>s
    /// </summary>
    public class ArchiveDirectory
    {
        /// <summary>
        /// The name of the Directory
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The contents of this directory.
        /// </summary>
        public Dictionary<string, object> Items { get; set; } = new Dictionary<string, object>();
        /// <summary>
        /// The parent directory (Null if non-existant)
        /// </summary>
        public ArchiveDirectory Parent { get; set; }
        /// <summary>
        /// The Archive that owns this directory
        /// </summary>
        public Archive OwnerArchive;

        /// <summary>
        /// Create a new Archive Directory
        /// </summary>
        public ArchiveDirectory() { }
        /// <summary>
        /// Create a new, child directory
        /// </summary>
        /// <param name="Owner">The Owner Archive</param>
        /// <param name="parentdir">The Parent Directory. NULL if this is the Root Directory</param>
        public ArchiveDirectory(Archive Owner, ArchiveDirectory parentdir) { OwnerArchive = Owner; Parent = parentdir; }
        /// <summary>
        /// Import a Folder into a RARCDirectory
        /// </summary>
        /// <param name="FolderPath"></param>
        /// <param name="Owner"></param>
        public ArchiveDirectory(string FolderPath, Archive Owner)
        {
            DirectoryInfo DI = new DirectoryInfo(FolderPath);
            Name = DI.Name;
            CreateFromFolder(FolderPath);
            OwnerArchive = Owner;
        }

        /// <summary>
        /// Export this Directory to a folder.
        /// </summary>
        /// <param name="FolderPath">Folder to Export to. Don't expect the files to appear here. Expect a Folder with this <see cref="Name"/> to appear</param>
        public void Export(string FolderPath)
        {
            FileInfoEx.CreateDirectoryIfNotExist(FolderPath);
            foreach (KeyValuePair<string, object> item in Items)
            {
                if (item.Value is ArchiveFile file)
                {
                    file.Save(FolderPath + "/" + file.Name);
                }
                else if (item.Value is ArchiveDirectory directory)
                {
                    string newstring = Path.Combine(FolderPath, directory.Name);
                    FileInfoEx.CreateDirectoryIfNotExist(newstring);
                    directory.Export(newstring);
                }
            }
        }
        /// <summary>
        /// Get or Set a file based on a path. When setting, if the file doesn't exist, it will be added (Along with any missing subdirectories)
        /// </summary>
        /// <param name="Path">The Path to take</param>
        /// <returns></returns>
        public object this[string Path]
        {
            get
            {
                string[] PathSplit = Path.Split('/');
                if (!ItemKeyExists(PathSplit[0]))
                    return null;
                return (PathSplit.Length > 1 && Items[PathSplit[0]] is ArchiveDirectory dir) ? dir[Path.Substring(PathSplit[0].Length + 1)] : Items[PathSplit[0]];
            }
            set
            {
                string[] PathSplit = Path.Split('/');
                if (!ItemKeyExists(PathSplit[0]) && !(value is null))
                {
                    ((dynamic)value).Parent = this;
                    if (PathSplit.Length == 1)
                    {
                        if (value is ArchiveDirectory dir)
                            dir.OwnerArchive = OwnerArchive;
                        ((dynamic)value).Parent = this;
                        Items.Add(PathSplit[0], value);

                        if (value is ArchiveFile f && string.IsNullOrEmpty(f.Name))
                        {
                            f.Name = PathSplit[0]; //If the file has no name, assign it the name defined in the path
                        }
                    }
                    else
                    {
                        ArchiveDirectory dir = NewDirectory(OwnerArchive, this);
                        dir.Name = PathSplit[0];
                        Items.Add(PathSplit[0], dir);
                        ((ArchiveDirectory)Items[PathSplit[0]])[Path.Substring(PathSplit[0].Length + 1)] = value;
                    }
                }
                else
                {
                    if (PathSplit.Length == 1)
                    {
                        if (value is null)
                        {
                            if (ItemKeyExists(PathSplit[0]))
                                Items.Remove(PathSplit[0]);
                            else
                                return;
                        }
                        else
                        {
                            ((dynamic)value).Parent = this;
                            Items[PathSplit[0]] = value;

                            if (value is ArchiveFile f && string.IsNullOrEmpty(f.Name))
                            {
                                f.Name = PathSplit[0]; //If the file has no name, assign it the name defined in the path
                            }
                        }
                    }
                    else if (Items[PathSplit[0]] is ArchiveDirectory dir)
                        dir[Path.Substring(PathSplit[0].Length + 1)] = value;
                }
            }
        }
        /// <summary>
        /// Checks to see if an Item Exists based on a Path
        /// </summary>
        /// <param name="Path">The path to take</param>
        /// <param name="IgnoreCase">Ignore casing</param>
        /// <returns>false if the Item isn't found</returns>
        public bool ItemExists(string Path, bool IgnoreCase = false)
        {
            string[] PathSplit = Path.Split('/');
            if (PathSplit.Length > 1 && ItemKeyExists(PathSplit[0]) && Items[PathSplit[0]] is ArchiveDirectory dir)
                return dir.ItemExists(Path.Substring(PathSplit[0].Length + 1), IgnoreCase);
            else if (PathSplit.Length > 1)
                return false;
            else
                return ItemKeyExists(PathSplit[0], IgnoreCase);
        }
        /// <summary>
        /// Checks to see if an item exists in this directory only
        /// </summary>
        /// <param name="ItemName">The name of the Item to look for (Case Sensitive)</param>
        /// <param name="IgnoreCase">Ignore casing</param>
        /// <returns>false if the Item doesn't exist</returns>
        public bool ItemKeyExists(string ItemName, bool IgnoreCase = false)
        {
            if (!IgnoreCase)
                return Items.ContainsKey(ItemName);

            foreach (KeyValuePair<string, object> item in Items)
                if (item.Key.Equals(ItemName, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="AttachRootName"></param>
        /// <returns></returns>
        public string GetItemKeyFromNoCase(string Path, bool AttachRootName = false)
        {
            string[] PathSplit = Path.Split('/');
            if (PathSplit.Length > 1)
            {
                string result = Items.FirstOrDefault(x => string.Equals(x.Key, PathSplit[0], StringComparison.OrdinalIgnoreCase)).Key;
                if (result == null)
                    return null;
                else
                    result = ((ArchiveDirectory)Items[result]).GetItemKeyFromNoCase(Path.Substring(PathSplit[0].Length + 1), true);
                return result == null ? null : (AttachRootName ? Name + "/" : "") + result;
            }
            else if (PathSplit.Length > 1)
                return null;
            else
            {
                string result = Items.FirstOrDefault(x => string.Equals(x.Key, PathSplit[0], StringComparison.OrdinalIgnoreCase)).Key;
                return result == null ? null : (AttachRootName ? Name + "/" : "") + result;
            }
        }
        /// <summary>
        /// Clears all the items out of this directory
        /// </summary>
        public void Clear()
        {
            foreach (KeyValuePair<string, object> item in Items)
            {
                if (item.Value is ArchiveDirectory dir)
                    dir.Clear();
            }
            Items.Clear();
        }
        /// <summary>
        /// Returns the amount of Items in this directory (Items in subdirectories not included)
        /// </summary>
        public int Count => Items.Count;
        /// <summary>
        /// The full path of this directory. Cannot be used if this .arc doesn't belong to a RARC object
        /// </summary>
        public string FullPath
        {
            get
            {
                if (OwnerArchive != null)
                {
                    StringBuilder path = new StringBuilder();
                    GetFullPath(path);
                    return path.ToString();
                }
                else
                    throw new InvalidOperationException("In order to use this, this directory must be part of a directory with a parent that is connected to a RARC object");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Path"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetCountAndChildren()
        {
            int count = 0;
            foreach (KeyValuePair<string, object> item in Items)
            {
                if (item.Value is ArchiveDirectory dir)
                    count += dir.GetCountAndChildren();
                else
                    count++;
            }
            return count;
        }
        /// <summary>
        /// Checks to see if this directory has an owner archive
        /// </summary>
        public bool HasOwnerArchive => OwnerArchive != null;
        /// <summary>
        /// Sorts the Items inside this directory using the provided string[]. This string[] MUST contain all entries inside this directory
        /// </summary>
        /// <param name="NewItemOrder"></param>
        public void SortItemsByOrder(string[] NewItemOrder)
        {
            if (NewItemOrder.Length != Items.Count)
                throw new Exception("Missing Items that exist in this Directory, but not in the provided Item Order");
            Dictionary<string, object> NewItems = new Dictionary<string, object>();
            for (int i = 0; i < NewItemOrder.Length; i++)
            {
                if (!Items.ContainsKey(NewItemOrder[i]))
                    throw new Exception("Missing Items that exist in this Directory, but not in the provided Item Order (Potentually a typo)");
                NewItems.Add(NewItemOrder[i], Items[NewItemOrder[i]]);
            }
            Items = NewItems;
        }
        /// <summary>
        /// Moves an item from it's current directory to a new directory
        /// </summary>
        /// <param name="ItemKey">The Key of the Item</param>
        /// <param name="TargetDirectory"></param>
        public void MoveItemToDirectory(string ItemKey, ArchiveDirectory TargetDirectory)
        {
            if (TargetDirectory.ItemKeyExists(ItemKey))
                throw new Exception($"There is already a file with the name {ItemKey} inside {TargetDirectory.Name}");

            TargetDirectory[ItemKey] = Items[ItemKey];
            Items.Remove(ItemKey);
        }
        /// <summary>
        /// Rename an item in the directory
        /// </summary>
        /// <param name="OldName"></param>
        /// <param name="NewName"></param>
        public void RenameItem(string OldName, string NewName)
        {
            if (ItemKeyExists(NewName))
                throw new Exception($"There is already a file with the name {NewName} inside {Name}");
            dynamic activeitem = (dynamic)Items[OldName];
            Items.Remove(OldName);
            activeitem.Name = NewName;
            Items.Add(NewName, activeitem);
        }
        /// <summary>
        /// Search the directory for files that match the regex
        /// </summary>
        /// <param name="Pattern">The regex pattern to use</param>
        /// <param name="TopLevelOnly">If true, all subdirectories will be skipped</param>
        /// <param name="IgnoreCase">Ignore the filename casing</param>
        /// <returns>List of Item Keys</returns>
        public List<string> FindItems(string Pattern, bool TopLevelOnly = false, bool IgnoreCase = false)
        {
            List<string> results = new List<string>();
            StringComparison sc = IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            RegexOptions ro = (IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None) | RegexOptions.Singleline;
            foreach (KeyValuePair<string, object> item in Items)
            {
                if (item.Value is ArchiveFile File)
                {
                    //Performance Enhancement
                    if ((Pattern.StartsWith("*") && File.Name.EndsWith(Pattern.Substring(1), sc)) || (Pattern.EndsWith("*") && File.Name.StartsWith(Pattern.Substring(Pattern.Length - 1), sc)))
                    {
                        goto Success;
                    }
                    string regexPattern = string.Concat("^", Regex.Escape(Pattern).Replace("\\*", ".*"), "$");
                    if (Regex.IsMatch(File.Name, regexPattern, ro))
                    {
                        goto Success;
                    }
                    continue;
                    Success:
                        results.Add(File.FullPath);
                }
                else if (item.Value is ArchiveDirectory Directory && !TopLevelOnly)
                    results.AddRange(Directory.FindItems(Pattern, IgnoreCase: IgnoreCase));
            }
            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Name} - {Items.Count} Item(s)";
        /// <summary>
        /// Create an ArchiveDirectory. You cannot use this function unless this directory is empty
        /// </summary>
        /// <param name="FolderPath"></param>
        /// <param name="OwnerArchive"></param>
        public void CreateFromFolder(string FolderPath, Archive OwnerArchive = null)
        {
            if (Items.Count > 0)
                throw new Exception("Cannot create a directory from a folder if Items exist");
            string[] Found = Directory.GetFiles(FolderPath, "*.*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < Found.Length; i++)
            {
                ArchiveFile temp = new ArchiveFile(Found[i]);
                Items[temp.Name] = temp;
            }

            string[] SubDirs = Directory.GetDirectories(FolderPath, "*.*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < SubDirs.Length; i++)
            {
                ArchiveDirectory temp = NewDirectory(SubDirs[i], OwnerArchive);
                Items[temp.Name] = temp;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual ArchiveDirectory NewDirectory() => new ArchiveDirectory();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Owner"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected virtual ArchiveDirectory NewDirectory(Archive Owner, ArchiveDirectory parent) => new ArchiveDirectory(Owner, parent);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="Owner"></param>
        /// <returns></returns>
        protected virtual ArchiveDirectory NewDirectory(string filename, Archive Owner) => new ArchiveDirectory(filename, Owner);
    }

    /// <summary>
    /// File contained inside the Archive
    /// </summary>
    public class ArchiveFile
    {
        /// <summary>
        /// Name of the File
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The extension of this file
        /// </summary>
        public string Extension
        {
            get
            {
                if (Name == null)
                    return null;
                string[] parts = Name.Split('.');
                if (parts.Length == 1)
                    return "";
                return "." + parts[parts.Length - 1].ToLower();
            }
        }
        /// <summary>
        /// The Actual Data for the file
        /// </summary>
        public byte[] FileData { get; set; }
        /// <summary>
        /// The parent directory (Null if non-existant)
        /// </summary>
        public ArchiveDirectory Parent { get; set; }
        /// <summary>
        /// Empty file
        /// </summary>
        public ArchiveFile() { }
        /// <summary>
        /// Load a File's Data based on a path
        /// </summary>
        /// <param name="Filepath"></param>
        public ArchiveFile(string Filepath)
        {
            Name = new FileInfo(Filepath).Name;
            FileData = File.ReadAllBytes(Filepath);
        }
        /// <summary>
        /// Create a File from a MemoryStream
        /// </summary>
        /// <param name="name">The name of the file</param>
        /// <param name="ms">The Memory Stream to use</param>
        public ArchiveFile(string name, MemoryStream ms)
        {
            Name = name;
            FileData = ms.ToArray();
        }
        /// <summary>
        /// Saves this file to the Computer's Disk
        /// </summary>
        /// <param name="Filepath">The full path to save to</param>
        public void Save(string Filepath)
        {
            File.WriteAllBytes(Filepath, FileData);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(ArchiveFile left, ArchiveFile right) => left.Equals(right);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(ArchiveFile left, ArchiveFile right) => !left.Equals(right);
        /// <summary>
        /// Compare this file to another
        /// </summary>
        /// <param name="obj">The Object to check</param>
        /// <returns>True if the files are identical</returns>
        public override bool Equals(object obj)
        {
            return obj is ArchiveFile file &&
                   Name == file.Name &&
                   Extension == file.Extension &&
                   EqualityComparer<byte[]>.Default.Equals(FileData, file.FileData);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var hashCode = -138733157;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Extension);
            hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(FileData);
            return hashCode;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Name} [0x{FileData.Length.ToString("X8")}]";

        /// <summary>
        /// The full path of this file. Cannot be used if this file doesn't belong to a RARC object somehow
        /// </summary>
        public string FullPath
        {
            get
            {
                if (Parent?.HasOwnerArchive ?? false)
                {
                    StringBuilder path = new StringBuilder();
                    GetFullPath(path);
                    return path.ToString();
                }
                else
                    throw new InvalidOperationException("In order to use this, this file must be part of a directory with a parent that is connected to a RARC object");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Path"></param>
        protected void GetFullPath(StringBuilder Path)
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

        //=====================================================================

        /// <summary>
        /// Cast a File to a MemoryStream
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator MemoryStream(ArchiveFile x) => new MemoryStream(x.FileData);
    }
}
