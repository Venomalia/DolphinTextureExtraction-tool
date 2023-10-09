using AuroraLib.Archives;
using AuroraLib.Common;
using AuroraLib.Compression;
using DolphinTextureExtraction.Scans.Helper;
using DolphinTextureExtraction.Scans.Options;
using DolphinTextureExtraction.Scans.Results;

namespace DolphinTextureExtraction.Scans
{
    public abstract class ScanBase
    {
        protected readonly string ScanPath;

        protected readonly string SaveDirectory;

        internal readonly ScanLogger Log;

        protected readonly ScanOptions Option;

        protected ScanResults Result = new();

        protected ScanBase(in string scanDirectory, in string saveDirectory, ScanOptions options, string logDirectory = null)
        {
            Option = options ?? new ScanOptions();
            ScanPath = scanDirectory;
            SaveDirectory = Option.DryRun ? StringEx.ExePath : saveDirectory;
            logDirectory ??= SaveDirectory;
            Directory.CreateDirectory(SaveDirectory);

            //do we need a dummy log?
            if (logDirectory == string.Empty)
            {
                Log = new(options); // dummy
            }
            else
            {
                Directory.CreateDirectory(logDirectory);
                Log = new(logDirectory, this.GetType().Name, options);
            }

            Events.NotificationEvent = Log.WriteNotification;
            Result.LogFullPath = Log.FullPath;
        }

        public virtual async Task<ScanResults> StartScan_Async()
        {
#if DEBUG
            if (Option.Parallel.MaxDegreeOfParallelism == 1)
            {
                ScanResults result = StartScan();
                return await Task.Run(() => result);
            }
#endif
            return await Task.Run(() => StartScan());
        }

        public virtual ScanResults StartScan()
        {
            DateTime starttime = DateTime.Now;

            if (Directory.Exists(ScanPath))
                Scan(new DirectoryInfo(ScanPath));
            else if (File.Exists(ScanPath))
            {
                var file = new FileInfo(ScanPath);
                Result.Worke = 1;
                Result.WorkeLength = file.Length;
                Option.ProgressUpdate(Result);

                Stream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                Scan(new ScanObjekt(stream, file.Name.AsSpan(), 0, file.Extension));
                stream.Close();

                Result.Progress++;
                Result.ProgressLength += file.Length;
                Option.ProgressUpdate(Result);
            }
            else
            {
                throw new ArgumentException($"{ScanPath}: does not exist!");
            }

            Result.TotalTime = DateTime.Now.Subtract(starttime);

            Log.WriteFoot(Result);
            Log.Dispose();
            GC.Collect();
            return Result;
        }

        protected void Scan(DirectoryInfo directory)
        {
            List<FileInfo> fileInfos = new();
            ScanInitialize(directory, fileInfos);
            Result.Worke = fileInfos.Count;
            Result.WorkeLength = directory.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            Option.ProgressUpdate(Result);

            Parallel.ForEach(fileInfos, Option.Parallel, (file, localSum, i) =>
            {
                Scan(file);
                lock (Result)
                {
                    Result.Progress++;
                    Result.ProgressLength += file.Length;
                }
                Option.ProgressUpdate(Result);
            });
        }

        private void ScanInitialize(DirectoryInfo directory, List<FileInfo> fileInfos)
        {
            foreach (DirectoryInfo subdirectory in directory.GetDirectories())
                ScanInitialize(subdirectory, fileInfos);

            foreach (FileInfo file in directory.GetFiles())
                fileInfos.Add(file);
        }

        #region Scan
        protected virtual void Scan(FileInfo file)
        {
            Stream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var SubPath = PathX.GetRelativePath(file.FullName.AsSpan(), ScanPath.AsSpan());
            ScanObjekt objekt = new(stream, SubPath, 0, file.Extension);
            Scan(objekt);
            stream.Close();
        }

        protected abstract void Scan(ScanObjekt so);

        protected void Scan(Archive archiv, ReadOnlySpan<char> subPath, int deep)
            => Scan(archiv.Root, subPath, deep);

        protected void Scan(ArchiveDirectory archivdirectory, ReadOnlySpan<char> subPath, int deep)
        {
            List<ArchiveFile> files = new();
            List<ArchiveFile> unkFiles = new();

            ArchiveInitialize(archivdirectory, files, unkFiles);

            double ArchLength = Scan(subPath.ToString(), deep, files);
            ArchLength += Scan(subPath.ToString(), deep, unkFiles);

            lock (Result)
                Result.ProgressLength -= ArchLength;
        }

        private double Scan(string subPath, int deep, List<ArchiveFile> fileInfos)
        {
            double ArchLength = 0;
            Parallel.ForEach(fileInfos, Option.SubParallel, (file) =>
            {
                if (file.FileData.CanRead)
                {
                    double Length = file.FileData.Length;

                    //Checks all possible illegal characters and converts them to hex
                    string path = PathX.GetValidPath(file.FullPath);
                    path = Path.Combine(subPath, path);

                    ScanObjekt objekt = new(file, path.AsSpan(), deep);

                    Scan(objekt);
                    lock (Result)
                    {
                        ArchLength += Length;
                        Result.ProgressLength += Length;
                    }
                    Option.ProgressUpdate(Result);
                }
            });
            return ArchLength;
        }

        private void ArchiveInitialize(ArchiveDirectory archivdirectory, List<ArchiveFile> files, List<ArchiveFile> unkFiles)
        {
            foreach (var item in archivdirectory.Items)
            {
                if (item.Value is ArchiveFile file)
                {
                    if (file.FileData.Identify(file.Extension).Typ == FormatType.Unknown)
                        unkFiles.Add(file);
                    else
                        files.Add(file);
                }
                if (item.Value is ArchiveDirectory directory)
                    ArchiveInitialize(directory, files, unkFiles);
            }
        }

        #endregion

        #region Helper
        /// <summary>
        /// Writes a Steam to a new file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="destFileName"></param>
        protected void Save(Stream stream, in string destFileName)
        {
            // Don't save anything if performing a dry run
            if (Option.DryRun)
                return;
            // Skip files that are present
            if (File.Exists(destFileName))
                return;
            // skip files that are blacklisted.
            ReadOnlySpan<char> fileName = Path.GetFileName(destFileName.AsSpan());
            for (int i = 0; i < blacklist.Length; i++)
                if (fileName.SequenceEqual(blacklist[i]))
                    return;

            string DirectoryName = Path.GetDirectoryName(destFileName);
            //We can't create a folder if a file with the same name exists.
            if (File.Exists(DirectoryName))
                File.Move(DirectoryName, DirectoryName + "_");

            Directory.CreateDirectory(DirectoryName);
            stream.Seek(0, SeekOrigin.Begin);
            using (FileStream file = new(destFileName, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(file);
            }
            stream.Seek(0, SeekOrigin.Begin);
        }
        private static readonly string[] blacklist = new[] { "desktop.ini", "Thumbs.db", ".DS_Store" };

        protected void Save(Stream stream, ReadOnlySpan<char> subdirectory, FormatInfo FFormat)
            => Save(stream, Path.ChangeExtension(GetFullSaveDirectory(subdirectory), FFormat.Extension));

        protected void Save(ScanObjekt so)
            => Save(so.Stream, string.Concat(GetFullSaveDirectory(so.SubPath), so.Extension));

        protected virtual bool TryExtract(ScanObjekt so)
        {
            if (so.Format.Class == null)
            {
                //If we have not detected the format yet, we will try to decompress them if they have a typical extension.
                switch (so.Format.Extension.ToLower())
                {
                    case ".arc":
                    case ".tpl":
                    case ".bti":
                    case ".onz":
                    case ".lz":
                    case ".zlip":
                    case ".lzo":
                    case ".lz11":
                    case ".bin":
                    case ".zs":
                    case ".lh":
                    case ".brres":
                    case ".breff":
                    case ".zlib":
                    case ".lz77":
                    case ".prs":
                    case ".wtm":
                    case ".vld":
                    case ".cxd":
                    case ".pcs":
                    case ".cms":
                    case ".cmp":
                    case ".cmparc":
                    case ".cmpres":
                        if (Reflection.Compression.TryToDecompress(so.Stream, out Stream test, out Type type))
                        {
                            Log.Write(FileAction.Extract, $"\"{so.SubPath}{so.Extension}\"", $"Decompressed with {type.Name}");
                            Scan(new ScanObjekt(test, so.SubPath, so.Deep + 1, Path.GetExtension(so.SubPath)));
                            return true;
                        }
                        break;
                }
            }
            else
            {
                if (so.Format.Class.IsSubclassOf(typeof(Archive)))
                {
                    using Archive archive = (Archive)Activator.CreateInstance(so.Format.Class);
                    string subPath = so.SubPath.ToString();
                    // if the archive needs more files.
                    if (so.Deep == 0)
                        archive.FileRequest = new Events.FileRequestDelegate(N => new FileStream(Path.Combine(Path.GetDirectoryName(Path.Combine(ScanPath, subPath)), N), FileMode.Open, FileAccess.Read, FileShare.Read));
                    else
                    {
                        ArchiveFile file = so.File;
                        archive.FileRequest = new Events.FileRequestDelegate(N => ((ArchiveFile)file?.Parent[N]).FileData);
                    }
                    archive.Open(so.Stream, subPath);
                    long size = archive.Root.Size;
                    //scan the archive file.
                    Scan(archive, so.SubPath, so.Deep + 1);

                    if (so.Stream.Length > 104857600 * 5) //100MB*5
                        return true;

                    //Reduces problems with multithreading
                    if (size < so.Stream.Length)
                        so.Stream.Seek(size < so.Stream.Position ? so.Stream.Position : size, SeekOrigin.Begin);

                    //checks if hidden files are present.
                    if (archive is IHasIdentifier identify)
                    {
                        if (so.Stream.Search(identify.Identifier.AsSpan().ToArray()))
                        {
                            List<byte[]> ident = new()
                                {
                                    identify.Identifier.AsSpan().ToArray(),
                                };
                            using (Archive Cut = new DataCutter(so.Stream, ident))
                            {
                                Log.WriteNotification(NotificationType.Info, $"{Cut.Root.Items.Count} hidden {so.Format.Class.Name} files found in\"{so.SubPath}{so.Extension}\".");
                                foreach (var item in Cut.Root.Items)
                                    ((ArchiveFile)item.Value).Name = ((ArchiveFile)item.Value).Extension;

                                Scan(Cut, so.SubPath, so.Deep + 1);
                            }
                        }
                    }
                    return true;
                }
                if (so.Format.Class.GetInterface(nameof(ICompression)) != null)
                {
                    Stream destream = new MemoryStream(((ICompression)Activator.CreateInstance(so.Format.Class)).Decompress(so.Stream));
                    Scan(new ScanObjekt(destream, so.SubPath, so.Deep + 1, so.Extension));
                    return true;
                }
            }
            return false;
        }

        protected virtual bool TryForce(ScanObjekt so)
        {
            if (so.Stream.Length < 25165824) // 24 MB
                if (Reflection.Compression.TryToDecompress(so.Stream, out Stream test, out _))
                {
                    Scan(new ScanObjekt(test, so.SubPath, so.Deep + 1, so.Extension));
                    return true;
                }
            so.Stream.Seek(0, SeekOrigin.Begin);
            if (TryCut(so))
                return true;
            so.Stream.Seek(0, SeekOrigin.Begin);

            return false;
        }

        private (FormatInfo, int) badformats;
        protected bool TryCut(ScanObjekt so)
        {
            try
            {
                if (badformats.Item1 == so.Format)
                    if (badformats.Item2 > 4)
                        return false;

                Archive archive = new DataCutter(so.Stream);
                if (archive.Root.Count > 0)
                {

                    badformats = (so.Format, -1);
                    Scan(archive, so.SubPath, so.Deep + 1);
                    return true;
                }
            }
            catch (Exception t)
            {
                Log.WriteEX(t, so.SubPath.ToString() + so.Format.Extension);
            }

            if (badformats.Item1 == so.Format)
            {
                if (badformats.Item2 != -1)
                    badformats.Item2++;
            }
            else
                badformats = (so.Format, 0);
            return false;
        }

        protected string GetFullSaveDirectory(ReadOnlySpan<char> directory)
            => Path.Join(SaveDirectory, directory.TrimEnd());

        protected virtual void LogResultUnknown(ScanObjekt so)
        {
            if (so.Format.Identifier == null)
            {
                byte[] infoBytes = so.Stream.Read(32 > so.Stream.Length ? (int)so.Stream.Length : 32);

                Log.Write(FileAction.Unknown, so.GetFullSubPath() + $" ~{PathX.AddSizeSuffix(so.Stream.Length, 2)}",
                    $"Bytes{infoBytes.Length}:[{BitConverter.ToString(infoBytes)}]");
            }
            else
            {
                Log.Write(FileAction.Unknown, so.GetFullSubPath() + $" ~{PathX.AddSizeSuffix(so.Stream.Length, 2)}",
                    $"Magic:[{so.Format.Identifier.GetString()}] Bytes:[{string.Join(",", so.Format.Identifier.AsSpan().ToArray())}] Offset:{so.Format.IdentifierOffset}");
            }
        }


        protected virtual void LogScanObjekt(ScanObjekt so)
        {
            if (so.Format.Typ != FormatType.Unknown)
            {
                string deep = so.Deep == 0 ? string.Empty : $", Deep:{so.Deep}";
                Log.WriteNotification(NotificationType.Info, $"Scan \"{so.SubPath}{so.Extension}\" recognized as {so.Format.GetFullDescription()}{deep}.");
            }
        }

        #endregion


    }
}
