using System;
using System.IO;
using System.Threading.Tasks;

namespace DolphinTextureExtraction_tool
{
    /// <summary>
    /// Sorts and removes unnecessary folders.
    /// </summary>
    static class Cleanup
    {
        public static ParallelOptions ParallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = -1 };

        public class Option
        {

            /// <summary>
            /// 
            /// </summary>
            public Type CleanupType { get; set; } = Type.Default;

            /// <summary>
            /// 
            /// </summary>
            public int MinGroupsSize { get; set; } = 3;

            public Option()
            {
                var config = ScanBase.Options.Config;
                if (Enum.TryParse<Type>(config.Get(nameof(CleanupType)), out Type type)) CleanupType = type;
                if (Int32.TryParse(config.Get(nameof(MinGroupsSize)), out int value)) MinGroupsSize = value;
            }
        }

        public enum Type
        {
            /// <summary>
            /// No cleanup
            /// </summary>
            None,
            /// <summary>
            /// Shortens the path by deleting unnecessary folders
            /// </summary>
            Default,
            /// <summary>
            /// Move all files to a single folder
            /// </summary>
            Simple,
        }

        public static bool Start(DirectoryInfo directory, Option options = null)
        {
            options = options ?? new Option();

            try
            {
                switch (options.CleanupType)
                {
                    case Type.None:
                        break;
                    case Type.Default:
                        Default(directory, options.MinGroupsSize);
                        break;
                    case Type.Simple:
                        Simple(directory);
                        break;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static void Default(DirectoryInfo directory, int maxGroupsSize)
        {
            int Directories = directory.GetDirectories().Length;

            Parallel.ForEach(directory.GetDirectories(), ParallelOptions, (DirectoryInfo subdirectory) =>
            {
                if (Directories == 1)
                {
                    if (subdirectory.Exists)
                        subdirectory.Merge(directory.FullName);
                    Default(directory, maxGroupsSize);
                    return;
                }
                else
                {
                    if (subdirectory.Exists)
                    {
                        if (subdirectory.GetDirectories().Length == 0)
                        {
                            if (subdirectory.GetFiles().Length <= maxGroupsSize)
                                subdirectory.Merge(directory.FullName);
                        }
                        else
                            Default(subdirectory, maxGroupsSize);
                    }
                }

            });
        }

        private static void Simple(DirectoryInfo directory)
        {
            Parallel.ForEach(directory.GetDirectories(), ParallelOptions, (DirectoryInfo subdirectory) =>
            {
                Simple(directory);
                subdirectory.Merge(directory.FullName);
            });
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

        public static void Merge(this DirectoryInfo directory, in string destDirName)
        {
            foreach (DirectoryInfo subdirectory in directory.GetDirectories())
            {
                string subpath = Path.Combine(destDirName, subdirectory.Name);
                if (Directory.Exists(subpath))
                    subdirectory.Merge(destDirName);
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
