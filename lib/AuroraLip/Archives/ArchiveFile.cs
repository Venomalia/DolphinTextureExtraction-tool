using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AuroraLip.Archives
{
    /*
    * Super Hackio Incorporated
    * "Copyright © Super Hackio Incorporated 2020-2021"
    * https://github.com/SuperHackio/Hack.io
    */

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
