using AuroraLib.Archives;
using AuroraLib.Archives.Formats.Nintendo;
using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Compression.Interfaces;
using AuroraLib.Core.Extensions;
using AuroraLib.Core.Interfaces;
using DolphinTextureExtraction.Scans.Helper;
using DolphinTextureExtraction.Scans.Options;
using DolphinTextureExtraction.Scans.Results;
using System.Runtime.CompilerServices;

namespace DolphinTextureExtraction.Scans
{
    public abstract class ScanBase
    {
        private readonly string ScanPath;

        private readonly string SaveDirectory;

        internal readonly ScanLogger Log;

        protected readonly ScanOptions Option;

        protected ScanResults Result = new();

        protected ScanBase(string scanDirectory, string saveDirectory, ScanOptions options, string logDirectory = null)
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

            Result.IsCompleted = false;
            if (Directory.Exists(ScanPath))
            {
                DirectoryInfo root = new (ScanPath);
                using DirectoryNode node = new(root);
                starttime = DateTime.Now;
                var files = node.GetAllValuesOf<FileNode>();
                Result.WorkeLength = files.Sum(fi => fi.Size);
                Option.ProgressUpdate(Result);
                Scan(node, 0);
            }
            else if (File.Exists(ScanPath))
            {
                FileInfo file = new(ScanPath);
                Result.WorkeLength = file.Length;
                Option.ProgressUpdate(Result);

                using FileNode node = new(file);
                ScanObjekt objekt = new(node, 0);
                Scan(objekt);

                Result.ProgressLength += file.Length;
            }
            else
            {
                Result.IsCompleted = true;
                throw new ArgumentException($"{ScanPath}: does not exist!");
            }
            Result.IsCompleted = true;
            Option.ProgressUpdate(Result);
            Result.TotalTime = DateTime.Now.Subtract(starttime);

            Log.WriteFoot(Result);
            Log.Dispose();
            GC.Collect();
            return Result;
        }

        private void ScanInitialize(DirectoryInfo directory, List<FileInfo> fileInfos)
        {
            foreach (DirectoryInfo subdirectory in directory.GetDirectories())
                ScanInitialize(subdirectory, fileInfos);

            foreach (FileInfo file in directory.GetFiles())
                fileInfos.Add(file);
        }

        #region Scan

        protected abstract void Scan(ScanObjekt so);

        protected void Scan(DirectoryNode directory, int deep)
        {
            double ArchLength = 0;

#if DEBUG
            if (Option.Parallel.MaxDegreeOfParallelism == 1)
            {
                foreach (var file in directory.GetAllValuesOf<FileNode>().ToArray())
                    ArchLength += Scan(file, deep);

                lock (Result)
                    Result.ProgressLength -= ArchLength;
                directory.Dispose();
                return;
            }
#endif

            Parallel.ForEach(directory.GetAllValuesOf<FileNode>().ToArray(), Option.GetSubParallel(deep),(file) =>
                {
                    ArchLength += Scan(file, deep);
                });

            lock (Result)
                Result.ProgressLength -= ArchLength;
            directory.Dispose();
        }


        private double Scan(FileNode file, int deep)
        {
            if (!file.Data.CanRead)
                return 0;

            double Length = file.Data.Length;
            try
            {
                ScanObjekt objekt = new(file, deep);
                Scan(objekt);
                lock (Result)
                    Result.ProgressLength += Length;
            }
            catch (Exception t)
            {
                lock (Result)
                {
                    Log.WriteEX(t, file.GetFullPath());
                    Result.ProgressLength += Length;
                }
            }

            Option.ProgressUpdate(Result);
            return Length;
        }

        #endregion

        #region Helper
        /// <summary>
        /// Writes a Steam to a new file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="destFileName"></param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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
            {
                if (fileName.SequenceEqual(blacklist[i]))
                    return;
            }

            string DirectoryName = new(Path.GetDirectoryName(destFileName.AsSpan()));
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
            => Save(so.File.Data, Path.ChangeExtension(GetFullSaveDirectory(so.File.GetFullPath()), so.Format.Extension));

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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
                        if (TryDecompress(so))
                            return true;
                        break;
                }
            }
            else
            {
                if (so.Format.Class.IsSubclassOf(typeof(ArchiveNode)))
                {
                    using ArchiveNode archive = (ArchiveNode)Activator.CreateInstance(so.Format.Class);
                    archive.BinaryDeserialize(so.File);
                    long size = archive.Size;
                    //scan the archive file.
                    if (Option.Parallel.MaxDegreeOfParallelism == 1 || so.Format.Typ != FormatType.Iso || so.Format.Class.Name == nameof(GCDisk))
                    {
                        Scan(archive, so.Deep + 1);
                    }
                    else
                    {
                        // Most disk images are faster in single-tasking mode.
                        int Parallelism = Option.Parallel.MaxDegreeOfParallelism;
                        Option.Parallel.MaxDegreeOfParallelism = 1;
                        Scan(archive, so.Deep + 1);
                        Option.Parallel.MaxDegreeOfParallelism = Parallelism;
                    }

                    if (so.File.Data.Length > 104857600 * 5) //100MB*5
                        return true;

                    //Reduces problems with multithreading
                    if (size < so.File.Data.Length)
                        so.File.Data.Seek(size < so.File.Data.Position ? so.File.Data.Position : size, SeekOrigin.Begin);

                    //checks if hidden files are present.
                    if (archive is IHasIdentifier identify)
                    {
                        if (so.File.Data.Search(identify.Identifier.AsSpan().ToArray()))
                        {
                            List<byte[]> ident = new()
                                {
                                    identify.Identifier.AsSpan().ToArray(),
                                };
                            using ArchiveNode Cut = new DataCutter(so.File.Data, ident);
                            Log.WriteNotification(NotificationType.Info, $"{Cut.Count} hidden {so.Format.Class.Name} files found in\"{so.File.GetFullPath()}\".");
                            Scan(Cut, so.Deep + 1);
                        }
                    }
                    so.File.Dispose();
                    return true;
                }
                if (so.Format.Class.GetInterface(nameof(ICompressionAlgorithm)) != null)
                {
                    ICompressionAlgorithm algorithm = (ICompressionAlgorithm)Activator.CreateInstance(so.Format.Class);
                    NodeProcessor.Expand(so.File, algorithm);
                    Scan(new ScanObjekt(so.File, so.Deep + 1));
                    return true;
                }
            }
            return false;
        }

        protected virtual bool TryForce(ScanObjekt so)
        {
            if (so.File.Data.Length < 25165824) // 24 MB
            {
                if (TryDecompress(so))
                    return true;
            }

            so.File.Data.Seek(0, SeekOrigin.Begin);
            if (TryCut(so))
                return true;
            so.File.Data.Seek(0, SeekOrigin.Begin);

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        protected virtual bool TryDecompress(ScanObjekt so)
        {
            if (Reflection.Compression.TryToDecompress(so.File.Data, out List<Stream> decomStreams, out Type type))
            {
                if (decomStreams.Count == 1)
                {
                    Log.Write(FileAction.Extract, $"\"{so.File.GetFullPath()}\"", $"Decompressed with {type.Name}.");
                    if (so.File.Name.AsSpan().Count('.') > 1)
                    {
                        so.File.Name = Path.GetFileNameWithoutExtension(so.File.Name);
                    }

                    so.File.Data.Dispose();
                    so.File.Data = decomStreams[0];
                    so = new(so.File, so.Deep + 1);
                    Scan(so);
                }
                else
                {
                    Log.Write(FileAction.Extract, $"\"{so.File.GetFullPath()}\"", $"Decompressed with {type.Name}, {decomStreams.Count} file chunks.");
                    using DirectoryNode cutDir = new(so.File.Name);

                    for (int i = 0; i < decomStreams.Count; i++)
                    {
                        cutDir.Add(new FileNode(i.ToString(), decomStreams[i]));
                    }
                    if (so.File.Parent != null)
                    {
                        DirectoryNode parent = so.File.Parent;
                        parent.Remove(so.File);
                        parent.Add(cutDir);
                    }
                    Scan(cutDir, so.Deep + 1);
                }
                return true;
            }
            return false;
        }

        private (FormatInfo, int) badformats;
        protected bool TryCut(ScanObjekt so)
        {
            try
            {
                if (badformats.Item1 == so.Format && badformats.Item2 > 4)
                    return false;

                ArchiveNode archive = new DataCutter(so.File.Data);
                if (archive.Count > 0)
                {

                    badformats = (so.Format, -1);
                    Scan(archive, so.Deep + 1);
                    return true;
                }
            }
            catch (Exception t)
            {
                Log.WriteEX(t, so.File.GetFullPath() + so.Format.Extension);
            }

            if (badformats.Item1 == so.Format)
            {
                if (badformats.Item2 != -1)
                    badformats.Item2++;
            }
            else
            {
                badformats = (so.Format, 0);
            }

            return false;
        }

        protected string GetFullSaveDirectory(ReadOnlySpan<char> paht)
            => Path.Join(SaveDirectory, PathX.GetValidPath(paht.ToString()));

        protected virtual void LogResultUnknown(ScanObjekt so)
        {
            if (so.Format.Identifier == null)
            {
                so.File.Data.Seek(0, SeekOrigin.Begin);
                byte[] infoBytes = so.File.Data.Read(32 > so.File.Data.Length ? (int)so.File.Data.Length : 32);

                Log.Write(FileAction.Unknown, so.File.GetFullPath() + $" ~{PathX.AddSizeSuffix(so.File.Data.Length, 2)}",
                    $"Bytes{infoBytes.Length}:[{BitConverter.ToString(infoBytes)}]");
            }
            else
            {
                Log.Write(FileAction.Unknown, so.File.GetFullPath() + $" ~{PathX.AddSizeSuffix(so.File.Data.Length, 2)}",
                    $"Magic:[{so.Format.Identifier}] Bytes:[{string.Join(",", so.Format.Identifier.AsSpan().ToArray())}] Offset:{so.Format.IdentifierOffset}");
            }
        }


        protected virtual void LogScanObjekt(ScanObjekt so)
        {
            if (so.Format.Typ != FormatType.Unknown)
            {
                string deep = so.Deep == 0 ? string.Empty : $", Deep:{so.Deep}";
                Log.WriteNotification(NotificationType.Info, $"Scan \"{so.File.GetFullPath()}\" recognized as {so.Format.GetFullDescription()}{deep}.");
            }
        }

        #endregion


    }
}
