using System.IO;

namespace AFSLib
{
    /// <summary>
    /// Class that represents an entry with data referenced from a file.
    /// </summary>
    public sealed class FileEntry : DataEntry
    {
        private readonly FileInfo fileInfo;

        internal FileEntry(string fileNamePath, string entryName)
        {
            fileInfo = new FileInfo(fileNamePath);

            Name = entryName;
            Size = (uint)fileInfo.Length;
            LastWriteTime = fileInfo.LastWriteTime;
            UnknownAttribute = (uint)fileInfo.Length;
        }

        internal override Stream GetStream()
        {
            return fileInfo.OpenRead();
        }
    }
}