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

        public SevenZip()
        { }

        public SevenZip(string filename) : base(filename)
        {
        }

        public SevenZip(Stream stream, string fullpath = null) : base(stream, fullpath)
        {
        }

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

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
        {
            Span<byte> id = stackalloc byte[8];
            stream.Read(id);
            foreach (var item in FileSignatures)
            {
                if (id[..item.Value.Length].SequenceEqual(item.Value))
                {
                    return true;
                }
            }
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
                    MemoryPoolStream ms = new((int)entry.Size);
                    entry.Extract(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    Root.AddArchiveFile(ms, entry.FileName);
                }
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        internal static Dictionary<SevenZipFormat, byte[]> FileSignatures = new()
        {
            {
                SevenZipFormat.Rar5, new byte[8] { 82, 97, 114, 33, 26, 7, 1, 0 }
            },
            {
                SevenZipFormat.Rar, new byte[7] { 82, 97, 114, 33, 26, 7, 0 }
            },
            {
                SevenZipFormat.Vhd, new byte[8] { 99, 111, 110, 101, 99, 116, 105, 120 }
            },
            {
                SevenZipFormat.Deb, new byte[7] { 33, 60, 97, 114, 99, 104, 62 }
            },
            {
                SevenZipFormat.Dmg, new byte[7] { 120, 1, 115, 13, 98, 98, 96 }
            },
            {
                SevenZipFormat.SevenZip, new byte[6] { 55, 122, 188, 175, 39, 28 }
            },
            {
                SevenZipFormat.Tar, new byte[5] { 117, 115, 116, 97, 114 }
            },
            {
                SevenZipFormat.Iso, new byte[5] { 67, 68, 48, 48, 49 }
            },
            {
                SevenZipFormat.Cab, new byte[4] { 77, 83, 67, 70 }
            },
            {
                SevenZipFormat.Rpm, new byte[4] { 237, 171, 238, 219 }
            },
            {
                SevenZipFormat.Xar, new byte[4] { 120, 97, 114, 33 }
            },
            {
                SevenZipFormat.Chm, new byte[4] { 73, 84, 83, 70 }
            },
            {
                SevenZipFormat.BZip2, new byte[3] { 66, 90, 104 }
            },
            {
                SevenZipFormat.Flv, new byte[3] { 70, 76, 86 }
            },
            {
                SevenZipFormat.Swf, new byte[3] { 70, 87, 83 }
            },
            {
                SevenZipFormat.GZip, new byte[2] { 31, 11 }
            },
            {
                SevenZipFormat.Zip, new byte[2] { 80, 75 }
            },
            {
                SevenZipFormat.Arj, new byte[2] { 96, 234 }
            },
            {
                SevenZipFormat.Lzh, new byte[3] { 45, 108, 104 }
            },
            {
                SevenZipFormat.SquashFS, new byte[4] { 104, 115, 113, 115 }
            }
        };
    }
}
