using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraLip.Archives.Extensions
{
    public static class ArchiveEX
    {
        /// <summary>
        /// Dump the contents of this archive to a folder
        /// </summary>
        /// <param name="FolderPath">The Path to save to. Should be a folder</param>
        /// <param name="Overwrite">If there are contents already at the chosen location, delete them?</param>
        public static void Export(this Archive archive, string FolderPath, bool Overwrite = false)
        {
            FolderPath = Path.Combine(FolderPath, archive.Root.Name);
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

            archive.Root.Export(FolderPath);
        }

        /// <summary>
        /// Export this Directory to a folder.
        /// </summary>
        /// <param name="FolderPath">Folder to Export to. Don't expect the files to appear here. Expect a Folder with this <see cref="Name"/> to appear</param>
        public static void Export(this ArchiveDirectory directory,string FolderPath)
        {
            Directory.CreateDirectory(FolderPath);
            foreach (KeyValuePair<string, ArchiveObject> item in directory.Items)
            {
                if (item.Value is ArchiveFile file)
                {
                    file.Save(Path.Combine(FolderPath, file.Name));
                }
                else if (item.Value is ArchiveDirectory subdirectory)
                {
                    string newstring = Path.Combine(FolderPath, subdirectory.Name);
                    Directory.CreateDirectory(newstring);
                    subdirectory.Export(newstring);
                }
            }
        }

        /// <summary>
        /// Saves this file to the Computer's Disk
        /// </summary>
        /// <param name="Filepath">The full path to save to</param>
        public static void Save(this ArchiveFile file, string Filepath)
            => File.WriteAllBytes(Filepath, file.FileData.ToArray());
    }
}
