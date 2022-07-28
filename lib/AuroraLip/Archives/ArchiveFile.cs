using AuroraLip.Common;
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
    public class ArchiveFile : ArchiveObject
    {

        /// <summary>
        /// The extension of this file
        /// </summary>
        public string Extension => Path.GetExtension(Name);

        /// <summary>
        /// The Actual Data for the file
        /// </summary>
        public Stream FileData { get; set; }

        /// <summary>
        /// Empty file
        /// </summary>
        public ArchiveFile() { }

        /// <summary>
        /// Load a File's Data based on a path
        /// </summary>
        /// <param name="Filepath"></param>
        public ArchiveFile(string Filepath, FileMode fileMode = FileMode.Open, FileAccess fileAccess = FileAccess.Write)
        {
            Name = new FileInfo(Filepath).Name;
            FileData = new FileStream(Filepath, fileMode, fileAccess);
        }
        /// <summary>
        /// Create a File from a MemoryStream
        /// </summary>
        /// <param name="name">The name of the file</param>
        /// <param name="stream">The Memory Stream to use</param>
        public ArchiveFile(string name, Stream stream)
        {
            Name = name;
            FileData = stream;
        }

        /// <summary>
        /// Saves this file to the Computer's Disk
        /// </summary>
        /// <param name="Filepath">The full path to save to</param>
        public void Save(string Filepath)
            => File.WriteAllBytes(Filepath, FileData.ToArray());

        public static bool operator ==(ArchiveFile left, ArchiveFile right)
            => left.Equals(right);

        public static bool operator !=(ArchiveFile left, ArchiveFile right)
            => !left.Equals(right);
        
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
                   EqualityComparer<Stream>.Default.Equals(FileData, file.FileData);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = -138733157;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Extension);
            hashCode = hashCode * -1521134295 + EqualityComparer<Stream>.Default.GetHashCode(FileData);
            return hashCode;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Name} [0x{FileData.Length:X8}]";

        //=====================================================================

        /// <summary>
        /// Cast a File to a Stream
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator Stream(ArchiveFile x) => x.FileData;

        private bool disposedValue;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposedValue)
            {
                if (disposing)
                    FileData.Dispose();

                disposedValue = true;
            }
        }
    }
}
