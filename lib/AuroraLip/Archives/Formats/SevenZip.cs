using AuroraLib.Common;
using SevenZipExtractor;

namespace AuroraLib.Archives.Formats
{
    public class SevenZip : Archive, IFileAccess
    {
        public bool CanRead => _CanRead;

        private static bool _CanRead = false;

        public bool CanWrite => false;

        public static string LibraryFilePath
        {
            get => libraryPath;
            set
            {
                _CanRead = File.Exists(value);
                libraryPath = value;
            }
        }
        private static string libraryPath;

        public SevenZip() { }

        public SevenZip(string filename) : base(filename) { }

        public SevenZip(Stream stream, string fullpath = null) : base(stream, fullpath) { }

        static SevenZip()
        {
            Initialize7zipDLL(IntPtr.Size == 4);
            if (!_CanRead)
            {
                Events.NotificationEvent?.Invoke(NotificationType.Warning, "7-zip DLL could not be found!");
            }
        }

        private static void Initialize7zipDLL(bool Is32Bit)
        {
            string text = Is32Bit ? "x86" : "x64";

            if (!_CanRead) LibraryFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, text, "7z.dll");
            if (!_CanRead) LibraryFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "7z-" + text + ".dll");
            if (!_CanRead) LibraryFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "7z-" + text + ".dll");
            if (!_CanRead) LibraryFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", text, "7z.dll");
            if (!_CanRead) LibraryFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip", "7z.dll");

            if (!_CanRead && !Is32Bit)
                Initialize7zipDLL(true);
        }

        public bool IsMatch(Stream stream, in string extension = "")
        {
            try
            {
                using (SevenZipExtractor.ArchiveFile archiveFile = new SevenZipExtractor.ArchiveFile(new SubStream(stream, stream.Length), null, libraryPath)) { }
                return true;
            }
            catch (Exception) { }
            return false;
        }

        protected override void Read(Stream ArchiveFile)
        {
            Root = new ArchiveDirectory() { OwnerArchive = this };

            //this protects our Stream from being closed by 7zip
            SubStream stream = new SubStream(ArchiveFile, ArchiveFile.Length);

            using (SevenZipExtractor.ArchiveFile archiveFile = new SevenZipExtractor.ArchiveFile(stream, null, libraryPath))
            {
                foreach (Entry entry in archiveFile.Entries)
                {
                    // extract to stream
                    MemoryStream memoryStream = new MemoryStream();
                    entry.Extract(memoryStream);
                    Root.AddArchiveFile(memoryStream, entry.FileName);
                }
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }
    }
}
