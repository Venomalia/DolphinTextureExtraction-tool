namespace DolphinTextureExtraction
{
    /// <summary>
    /// Sorts and removes unnecessary folders.
    /// </summary>
    static class Cleanup
    {
        public static ParallelOptions ParallelOptions = AppSettings.Parallel;

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
                var config = AppSettings.Config;
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
            options ??= new Option();

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
                            int Textures = subdirectory.GetFiles("*.png").Length;
                            if (Textures <= maxGroupsSize && Textures != 0)
                                subdirectory.Merge(directory.FullName);
                        }
                        else
                        {
                            Default(subdirectory, maxGroupsSize);
                        }
                    }
                }

            });
        }

        private static void Simple(DirectoryInfo directory)
            => Simple(directory, directory);


        private static void Simple(DirectoryInfo directory,DirectoryInfo root)
        {
            Parallel.ForEach(directory.GetDirectories(), ParallelOptions, (DirectoryInfo subdirectory) =>
            {
                Simple(subdirectory, root);

                foreach (FileInfo file in subdirectory.GetFiles())
                {
                    string filePath = Path.Combine(root.FullName, file.Name);
                    file.MoveTo(filePath);
                }
                subdirectory.Delete();
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
                string filePath = Path.Combine(destDirName, file.Name);
                file.MoveTo(filePath);
            }
            directory.Delete();
        }
    }
}
