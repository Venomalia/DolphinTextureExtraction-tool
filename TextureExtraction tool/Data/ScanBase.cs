using AFSLib;
using AuroraLip.Archives;
using AuroraLip.Common;
using AuroraLip.Common.Extensions;
using AuroraLip.Compression;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DolphinTextureExtraction_tool
{
    public abstract class ScanBase
    {
        protected readonly string ScanPath;

        protected readonly string SaveDirectory;

        internal readonly ScanLogger Log;

        protected readonly Options Option;

        protected Results Result = new Results();

        public class Options
        {
            static NameValueCollection config;
            public static NameValueCollection Config => config = config ?? ConfigurationManager.AppSettings;
            static bool? useConfig = null;
            public static bool UseConfig = (bool)(useConfig = useConfig ?? Config.HasKeys() && bool.TryParse(Config.Get("UseConfig"), out bool value) && value);
#if DEBUG
            public ParallelOptions Parallel = new ParallelOptions() { MaxDegreeOfParallelism = 1 };
#else
            public ParallelOptions Parallel = new ParallelOptions() { MaxDegreeOfParallelism = 4 };
#endif
            internal ParallelOptions SubParallel => new ParallelOptions()
            {
                MaxDegreeOfParallelism = Math.Max(1, Parallel.MaxDegreeOfParallelism / 2),
                CancellationToken = Parallel.CancellationToken,
                TaskScheduler = Parallel.TaskScheduler
            };

            /// <summary>
            /// Tries to extract files from unknown files, may cause errors.
            /// </summary>
            public bool Force = false;

            public Options()
            {
                if (!UseConfig) return;
                if (bool.TryParse(Config.Get("DryRun"), out bool value)) DryRun = value;
                if (bool.TryParse(Config.Get("Force"), out value)) Force = value;
                if (int.TryParse(Config.Get("Tasks"), out int thing)) Parallel.MaxDegreeOfParallelism = thing <= 0 ? Environment.ProcessorCount : thing;
            }

            /// <summary>
            /// Don't actually extract anything.
            /// </summary>
            public bool DryRun = false;

            /// <summary>
            /// will be executed if progress was made
            /// </summary>
            public Action<Results> ProgressAction;

            private double LastProgressLength = 0;
            internal void ProgressUpdate(Results result)
            {

                if (result.Progress >= result.Worke)
                {
                    //we have to report the last progress!
                    Monitor.Enter(result);
                    try
                    {
                        LastProgressLength = 0;
                        result.ProgressLength = result.WorkeLength;
                        ProgressAction?.Invoke(result);
                    }
                    finally
                    {
                        Monitor.Exit(result);
                    }
                }
                else
                {
                    //Try to tell the Progress
                    if (!Monitor.TryEnter(result))
                        return;

                    try
                    {
                        //is there really progress to report.
                        if (result.ProgressLength < LastProgressLength)
                            return;

                        //when data has been compressed, we can achieve more than 100%... we prevent this.
                        if (result.ProgressLength > result.WorkeLength)
                            result.ProgressLength = result.WorkeLength;

                        LastProgressLength = result.ProgressLength;

                        ProgressAction?.Invoke(result);
                    }
                    finally
                    {
                        Monitor.Exit(result);
                    }
                }

            }
        }


        public class Results
        {
            /// <summary>
            /// the time the scan process has taken
            /// </summary>
            public TimeSpan TotalTime { get; internal set; }

            /// <summary>
            /// count of all files to be searched.
            /// </summary>
            public int Worke { get; internal set; }

            /// <summary>
            /// count of all files already searched.
            /// </summary>
            public int Progress { get; internal set; } = 0;

            /// <summary>
            /// Size of all files to be searched in bytes.
            /// </summary>
            public double WorkeLength { get; internal set; }

            /// <summary>
            /// Size of all already searched files in bytes.
            /// </summary>
            public double ProgressLength { get; internal set; } = 0;

            /// <summary>
            /// Full path to the log file.
            /// </summary>
            public string LogFullPath { get; internal set; }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Scan time: {TotalTime.TotalSeconds:.000}s");
                return sb.ToString();
            }
        }

        protected ScanBase(in string scanDirectory, in string saveDirectory, Options options = null)
        {
            Option = options ?? new Options();
            ScanPath = scanDirectory;
            SaveDirectory = Option.DryRun ? StringEx.ExePath : saveDirectory;
            Directory.CreateDirectory(SaveDirectory);
            Log = new ScanLogger(SaveDirectory);
            Events.NotificationEvent = Log.WriteNotification;
            Result.LogFullPath = Log.FullPath;
        }

        public virtual Results StartScan()
        {
            DateTime starttime = DateTime.Now;

            if (Directory.Exists(ScanPath))
            {
                Scan(new DirectoryInfo(ScanPath));
            }
            else if (File.Exists(ScanPath))
            {
                var file = new FileInfo(ScanPath);
                Result.Worke = 1;
                Result.WorkeLength = file.Length;
                Option.ProgressUpdate(Result);
                Scan(file);
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
            List<FileInfo> fileInfos = new List<FileInfo>();
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
            var SubPath = PathEX.GetRelativePath(file.FullName.AsSpan(), ScanPath.AsSpan());
            Scan(new ScanObjekt(stream, SubPath, 0, file.Extension));
            stream.Close();
        }

        protected abstract void Scan(ScanObjekt so);

        protected void Scan(Archive archiv, ReadOnlySpan<char> subPath, int deep)
            => Scan(archiv.Root, subPath, deep);

        protected void Scan(ArchiveDirectory archivdirectory, ReadOnlySpan<char> subPath, int deep)
        {
            List<ArchiveFile> files = new List<ArchiveFile>();
            List<ArchiveFile> unkFiles = new List<ArchiveFile>();

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
                    Scan(new ScanObjekt(file, Path.Combine(subPath, file.FullPath.Trim(Path.GetInvalidPathChars()).Trim('`', '|')).AsSpan(), deep));
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
                {
                    ArchiveInitialize(directory, files, unkFiles);
                }
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
            if (Option.DryRun) return;

            string DirectoryName = Path.GetDirectoryName(destFileName);
            //We can't create a folder if a file with the same name exists.
            if (File.Exists(DirectoryName))
                File.Move(DirectoryName, DirectoryName + "_");

            Directory.CreateDirectory(DirectoryName);
            stream.Seek(0, SeekOrigin.Begin);
            using (FileStream file = new FileStream(destFileName, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(file);
            }
            stream.Seek(0, SeekOrigin.Begin);
        }

        protected void Save(Stream stream, string subdirectory, FormatInfo FFormat)
            => Save(stream, Path.ChangeExtension(GetFullSaveDirectory(subdirectory), FFormat.Extension));

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
                        if (Reflection.Compression.TryToDecompress(so.Stream, out Stream test, out _))
                        {
                            Scan(new ScanObjekt(test, so.SubPath, so.Deep + 1, PathEX.GetExtension(PathEX.WithoutExtension(so.SubPath)).ToString()));
                            return true;
                        }
                        break;
                }
            }
            else
            {
                if (so.Format.Class.IsSubclassOf(typeof(Archive)))
                {
                    using (Archive archive = (Archive)Activator.CreateInstance(so.Format.Class))
                    {
                        // if the archive needs more files.
                        if (so.Deep == 0)
                        {
                            string subPath = so.SubPath.ToString();
                            archive.FileRequest = new Events.FileRequestDelegate(N => new FileStream(Path.Combine(Path.GetDirectoryName(Path.Combine(ScanPath, subPath)), N), FileMode.Open, FileAccess.Read, FileShare.Read));
                        }
                        else
                        {
                            ArchiveFile file = so.File;
                            archive.FileRequest = new Events.FileRequestDelegate(N => ((ArchiveFile)file.Parent[N]).FileData);
                        }
                        archive.Open(so.Stream, so.SubPath.ToString());
                        long size = archive.Root.Size;
                        //scan the archive file.
                        Scan(archive, so.SubPath, so.Deep + 1);

                        if (so.Stream.Length > 104857600 * 5) //100MB*5
                            return true;

                        //Reduces problems with multithreading
                        so.Stream.Seek(size < so.Stream.Position ? so.Stream.Position : size, SeekOrigin.Begin);

                        //checks if hidden files are present.
                        if (archive is IMagicIdentify identify)
                        {
                            if (so.Stream.Search(identify.Magic))
                            {
                                List<byte[]> ident = new List<byte[]>
                                {
                                    identify.Magic.ToByte()
                                };
                                using (Archive Cut = new DataCutter(so.Stream, ident))
                                {
                                    foreach (var item in Cut.Root.Items)
                                        ((ArchiveFile)item.Value).Name = ((ArchiveFile)item.Value).Extension;

                                    Scan(Cut, so.SubPath, so.Deep + 1);
                                }
                            }
                        }
                    }
                    return true;
                }
                if (so.Format.Class.GetInterface(nameof(ICompression)) != null)
                {
                    Stream destream = ((ICompression)Activator.CreateInstance(so.Format.Class)).Decompress(so.Stream);
                    Scan(new ScanObjekt(destream, so.SubPath, so.Deep, so.Extension));
                    return true;
                }
                //External classes
                switch (so.Format.Class.Name)
                {
                    case "AFS":
                        using (AFS afs = new AFS(so.Stream))
                        {
                            foreach (Entry item in afs.Entries)
                                if (item is StreamEntry Streamitem)
                                    Scan(new ScanObjekt(Streamitem.GetSubStream(), Path.Combine(so.SubPath.ToString(), Streamitem.SanitizedName).AsSpan(), so.Deep + 1, Path.GetExtension(Streamitem.SanitizedName)));
                        }
                        break;
                }
            }
            return false;
        }

        protected virtual bool TryForce(ScanObjekt so)
        {
            if (so.Stream.Length < 25165824) // 24 MB
                if (Reflection.Compression.TryToDecompress(so.Stream, out Stream test, out _))
                {
                    Scan(new ScanObjekt(test, so.SubPath, so.Deep, so.Extension));
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

        protected string GetFullSaveDirectory(in string directory)
            => Path.Combine(SaveDirectory, directory).TrimEnd();

        #endregion

        protected ref struct ScanObjekt
        {
            public Stream Stream { get; }
            public FormatInfo Format { get; }
            public ReadOnlySpan<char> SubPath { get; }
            public int Deep { get; }
            public string Extension { get; }
            public ArchiveFile File { get; }

            public ScanObjekt(Stream stream, ReadOnlySpan<char> subPath, int deep = 0, string extension = "")
            {
                Stream = stream;
                Extension = extension;
                Format = stream.Identify(Extension);
                SubPath = PathEX.WithoutExtension(subPath);
                Deep = deep;
                File = null;
            }

            public ScanObjekt(ArchiveFile file, ReadOnlySpan<char> subPath, int deep)
            {
                Stream = file.FileData;
                Extension = file.Extension;
                Format = Stream.Identify(Extension);
                SubPath = PathEX.WithoutExtension(subPath);
                Deep = deep;
                File = file;
            }
        }
    }
}
