using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AuroraLip.Archives
{
    /*
    * Super Hackio Incorporated
    * "Copyright © Super Hackio Incorporated 2020-2021"
    * https://github.com/SuperHackio/Hack.io
    */

    /// <summary>
    /// Folder contained inside the Archive. Can contain more <see cref="ArchiveDirectory"/>s if desired, as well as <see cref="ArchiveFile"/>s
    /// </summary>
    public class ArchiveDirectory : ArchiveObject
    {

        /// <summary>
        /// The contents of this directory.
        /// </summary>
        public Dictionary<string, ArchiveObject> Items { get; set; } = new Dictionary<string, ArchiveObject>();

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
        /// Import a Folder into a Archive
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
        /// Get or Set a file based on a path. When setting, if the file doesn't exist, it will be added (Along with any missing subdirectories)
        /// </summary>
        /// <param name="path">The Path to take</param>
        /// <returns></returns>
        public ArchiveObject this[string path]
        {
            get
            {
                string[] PathSplit = path.Split(Path.DirectorySeparatorChar);
                if (!ItemKeyExists(PathSplit[0]))
                    return null;
                return (PathSplit.Length > 1 && Items[PathSplit[0]] is ArchiveDirectory dir) ? dir[path.Substring(PathSplit[0].Length + 1)] : Items[PathSplit[0]];
            }
            set
            {
                string[] PathSplit = path.Split(Path.DirectorySeparatorChar);
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
                        ((ArchiveDirectory)Items[PathSplit[0]])[path.Substring(PathSplit[0].Length + 1)] = value;
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
                        dir[path.Substring(PathSplit[0].Length + 1)] = value;
                }
            }
        }
        /// <summary>
        /// Checks to see if an Item Exists based on a Path
        /// </summary>
        /// <param name="path">The path to take</param>
        /// <param name="IgnoreCase">Ignore casing</param>
        /// <returns>false if the Item isn't found</returns>
        public bool ItemExists(string path, bool IgnoreCase = false)
        {
            string[] PathSplit = path.Split(Path.DirectorySeparatorChar);
            if (PathSplit.Length > 1 && ItemKeyExists(PathSplit[0]) && Items[PathSplit[0]] is ArchiveDirectory dir)
                return dir.ItemExists(path.Substring(PathSplit[0].Length + 1), IgnoreCase);
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

            foreach (KeyValuePair<string, ArchiveObject> item in Items)
                if (item.Key.Equals(ItemName, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="AttachRootName"></param>
        /// <returns></returns>
        public string GetItemKeyFromNoCase(string path, bool AttachRootName = false)
        {
            string[] PathSplit = path.Split(Path.DirectorySeparatorChar);
            if (PathSplit.Length > 1)
            {
                string result = Items.FirstOrDefault(x => string.Equals(x.Key, PathSplit[0], StringComparison.OrdinalIgnoreCase)).Key;
                if (result == null)
                    return null;
                else
                    result = ((ArchiveDirectory)Items[result]).GetItemKeyFromNoCase(path.Substring(PathSplit[0].Length + 1), true);
                return result == null ? null : (AttachRootName ? Name + Path.DirectorySeparatorChar : "") + result;
            }
            else if (PathSplit.Length > 1)
                return null;
            else
            {
                string result = Items.FirstOrDefault(x => string.Equals(x.Key, PathSplit[0], StringComparison.OrdinalIgnoreCase)).Key;
                return result == null ? null : (AttachRootName ? Name + Path.DirectorySeparatorChar : "") + result;
            }
        }
        /// <summary>
        /// Clears all the items out of this directory
        /// </summary>
        public void Clear()
        {
            foreach (KeyValuePair<string, ArchiveObject> item in Items)
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
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetCountAndChildren()
        {
            int count = 0;
            foreach (KeyValuePair<string, ArchiveObject> item in Items)
            {
                if (item.Value is ArchiveDirectory dir)
                    count += dir.GetCountAndChildren();
                else
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Sorts the Items inside this directory using the provided string[]. This string[] MUST contain all entries inside this directory
        /// </summary>
        /// <param name="NewItemOrder"></param>
        public void SortItemsByOrder(string[] NewItemOrder)
        {
            if (NewItemOrder.Length != Items.Count)
                throw new Exception("Missing Items that exist in this Directory, but not in the provided Item Order");
            Dictionary<string, ArchiveObject> NewItems = new Dictionary<string, ArchiveObject>();
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
            foreach (KeyValuePair<string, ArchiveObject> item in Items)
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

        protected virtual ArchiveDirectory NewDirectory() => new ArchiveDirectory();

        protected virtual ArchiveDirectory NewDirectory(Archive Owner, ArchiveDirectory parent) => new ArchiveDirectory(Owner, parent);

        protected virtual ArchiveDirectory NewDirectory(string filename, Archive Owner) => new ArchiveDirectory(filename, Owner);

        public virtual void AddArchiveFile(Stream stream, string name)
        {
            ArchiveFile Sub = new ArchiveFile() { Parent = this, Name = name, FileData = stream};
            this.Items.Add(Sub.Name, Sub);
        }

        public virtual void AddArchiveFile(Stream stream, long length, long offset, string name)
        {
            ArchiveFile Sub = new ArchiveFile() { Parent = this, Name = name };
            Sub.FileData = new ArchiveFile.ArchiveFileStream(stream, length, offset) { Parent = Sub };
            this.Items.Add(Sub.Name, Sub);
        }

        public virtual void AddArchiveFile(Stream stream, long length, string name)
            => AddArchiveFile(stream, length, stream.Position, name);

        private bool disposedValue;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposedValue)
            {
                if (disposing)
                    foreach (var item in Items)
                        if (item.Value is IDisposable d)
                            d.Dispose();

                disposedValue = true;
            }
        }
    }
}
