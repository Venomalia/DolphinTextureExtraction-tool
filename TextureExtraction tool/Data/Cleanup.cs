using System.IO;
using System.Threading.Tasks;

namespace DolphinTextureExtraction_tool
{
    static class Cleanup
    {
        public static ParallelOptions ParallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 4 };

        public static void Default(DirectoryInfo directory)
        {
            int Directories = directory.GetDirectories().Length;

            Parallel.ForEach(directory.GetDirectories(), ParallelOptions, (DirectoryInfo subdirectory) =>
            {
                if (Directories == 1)
                {
                    subdirectory.Merge(directory.FullName);
                    Default(directory);
                }
                else
                {
                    if (subdirectory.Exists)
                        Default(subdirectory);
                }
            });

            if (directory.GetFilesCount() <= Directories * 1.5)
            {
                foreach (DirectoryInfo subdirectory in directory.GetDirectories())
                {
                    if (subdirectory.Exists)
                        subdirectory.Merge(directory.FullName);
                }
            }

        }

        private static int GetFilesCount(this DirectoryInfo directory)
        {
            int Textures = directory.GetFiles().Length;
            foreach (DirectoryInfo subdirectory in directory.GetDirectories())
            {
                Textures += subdirectory.GetFilesCount();
            }
            return Textures;
        }

        public static void Merge(this DirectoryInfo directory, string destDirName)
        {
            foreach (DirectoryInfo subdirectory in directory.GetDirectories())
            {
                string subpath = Path.Combine(destDirName, subdirectory.Name);
                if (Directory.Exists(subpath))
                    subdirectory.Merge(subpath);
                else
                    subdirectory.MoveTo(subpath);
            }

            foreach (FileInfo file in directory.GetFiles())
            {
                file.MoveTo(Path.Combine(destDirName, file.Name));
            }
            directory.Delete();
        }
    }
}
