using AFSLib;
using AuroraLip.Archives;
using AuroraLip.Common;
using AuroraLip.Compression;
using LibCPK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DolphinTextureExtraction_tool
{
    public abstract class ScanBase
    {
        protected readonly string ScanDirectory;

        protected readonly string SaveDirectory;

        internal readonly ScanLogger Log;

        protected readonly Options Option;

        protected Results Result = new Results();

        public class Options
        {
#if DEBUG
            public ParallelOptions Parallel = new ParallelOptions() { MaxDegreeOfParallelism = 1 };
#else
            public ParallelOptions Parallel = new ParallelOptions() { MaxDegreeOfParallelism = 4 };
#endif
            public Action<Results> ProgressAction;
        }


        public class Results
        {
            public int Worke { get; internal set; }

            public int Progress { get; internal set; } = 0;

            public double WorkeLength { get; internal set; }

            public double ProgressLength { get; internal set; } = 0;

            public string LogFullPath { get; internal set; }
        }

        protected ScanBase(string scanDirectory, string saveDirectory, Options options = null)
        {
            ScanDirectory = scanDirectory;
            SaveDirectory = saveDirectory;
            Directory.CreateDirectory(saveDirectory);
            Log = new ScanLogger(SaveDirectory);
            Events.NotificationEvent = Log.WriteNotification;
            Result.LogFullPath = Log.FullPath;

            if (options == null)
            {
                Option = new Options();
            }
            else
            {
                Option = options;
            }
        }

        protected void Scan(DirectoryInfo directory)
        {
            List<FileInfo> fileInfos = new List<FileInfo>();
            ScanInitialize(directory, fileInfos);
            Result.Worke = fileInfos.Count;
            Result.WorkeLength =  directory.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            Option.ProgressAction?.Invoke(Result);

            Parallel.ForEach(fileInfos, Option.Parallel, (file, localSum, i) =>
            {
                Scan(file);
                lock (Result)
                {
                    Result.Progress++;
                    Result.ProgressLength += file.Length;
                }
                Option.ProgressAction?.Invoke(Result);
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
        protected abstract void Scan(FileInfo file);

        protected abstract void Scan(Stream stream, string subdirectory, in string Extension = "");

        protected void Scan(Archive archiv, in string subdirectory)
            => Scan(archiv.Root, subdirectory);

        protected void Scan(ArchiveDirectory archivdirectory, string subdirectory)
        {
            List<ArchiveFile> fileInfos = new List<ArchiveFile>();
            ArchiveInitialize(archivdirectory, fileInfos);

            Parallel.ForEach(fileInfos, Option.Parallel, (file) =>
            {
                Scan(file, subdirectory);
            });
        }

        private void ArchiveInitialize(ArchiveDirectory archivdirectory, List<ArchiveFile> files)
        {
            foreach (var item in archivdirectory.Items)
            {
                if (item.Value is ArchiveFile file)
                {
                    files.Add(file);
                }
                if (item.Value is ArchiveDirectory directory)
                {
                    ArchiveInitialize(directory, files);
                }
            }
        }

        protected void Scan(ArchiveFile file, in string subdirectory)
        {
            Scan(file.FileData, Path.Combine(subdirectory, file.FullPath), file.Extension.ToLower());
        }
        #endregion

        #region Helper
        /// <summary>
        /// Writes a Steam to a new file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="destFileName"></param>
        protected void Save(Stream stream, string destFileName)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destFileName));
            using (FileStream file = new FileStream(destFileName, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(file);
            }
            stream.Position = 0;
        }

        protected void Save(Stream stream, string subdirectory, FormatInfo FFormat)
            => Save(stream, Path.ChangeExtension(GetFullSaveDirectory(subdirectory), FFormat.Extension));

        protected virtual bool TryExtract(Stream stream, string subdirectory, FormatInfo FFormat)
        {
            if (FFormat.Class == null)
            {
                switch (FFormat.Extension.ToLower())
                {
                    case ".arc":
                    case ".tpl":
                    case ".bti":
                    case ".lz":
                    case ".brres":
                    case ".breff":
                    case ".zlib":
                    case ".lz77":
                    case ".wtm":
                    case ".vld":
                    case ".cxd":
                    case ".cmparc":
                    case ".cmpres":
                        if (Reflection.Compression.TryToDecompress(stream, out Stream test, out _))
                        {
                            Scan(test, subdirectory);
                            return true;
                        }
                        break;
                }
            }
            else
            {
                if (FFormat.Class.IsSubclassOf(typeof(Archive)))
                {
                    using (Archive archive = (Archive)Activator.CreateInstance(FFormat.Class))
                    {
                        archive.Open(stream);
                        Scan(archive, subdirectory);

                        if (stream.Length > 104857600 * 5) //100MB*5
                            return true;

                        if (archive is IMagicIdentify identify)
                        {
                            if (stream.Search(identify.Magic))
                            {
                                List<byte[]> ident = new List<byte[]>();
                                ident.Add(identify.Magic.ToByte());
                                using (Archive Cut = new DataCutter(stream, ident))
                                {
                                    foreach (var item in Cut.Root.Items)
                                        ((ArchiveFile)item.Value).Name = ((ArchiveFile)item.Value).Extension;

                                    Scan(Cut, subdirectory);
                                }
                            }
                        }
                    }
                    return true;
                }
                if (FFormat.Class.GetInterface(nameof(ICompression)) != null)
                {
                    Scan(((ICompression)Activator.CreateInstance(FFormat.Class)).Decompress(stream), subdirectory);
                    return true;
                }
                //External classes
                switch (FFormat.Class.Name)
                {
                    case "AFS":
                        using (AFS afs = new AFS(stream))
                        {
                            foreach (Entry item in afs.Entries)
                                if (item is StreamEntry Streamitem)
                                    Scan(Streamitem.GetSubStream(), Path.Combine(subdirectory, Streamitem.SanitizedName), Path.GetExtension(Streamitem.SanitizedName));
                        }
                        break;
                }
            }
            return false;
        }

        protected virtual bool TryForce(Stream stream, string subdirectory, FormatInfo FFormat)
        {
            if (Reflection.Compression.TryToDecompress(stream, out Stream test, out _))
            {
                Scan(test, subdirectory);
                return true;
            }

            if (TryCut(stream, subdirectory, FFormat))
                return true;

            return false;
        }

        private (FormatInfo, int) badformats;
        protected bool TryCut(Stream stream, string subdirectory, FormatInfo FFormat)
        {
            try
            {
                if (badformats.Item1 == FFormat)
                    if (badformats.Item2 > 4)
                        return false;

                Archive archive = new DataCutter(stream);
                if (archive.Root.Count > 0)
                {

                    badformats = (FFormat, -1);
                    Scan(archive, subdirectory);
                    return true;
                }

                if (badformats.Item1 == FFormat)
                {
                    if (badformats.Item2 != -1)
                        badformats.Item2++;
                }
                else
                    badformats = (FFormat, 0);
            }
            catch (Exception) { }
            return false;
        }

        protected bool CpkDecompressEntrie(CPK CpkContent, BinaryReader CPKReader, LibCPK.FileEntry entrie, out byte[] chunk)
        {
            CPKReader.BaseStream.Seek((long)entrie.FileOffset, SeekOrigin.Begin);

            string isComp = Encoding.ASCII.GetString(CPKReader.ReadBytes(8));
            CPKReader.BaseStream.Seek((long)entrie.FileOffset, SeekOrigin.Begin);

            chunk = CPKReader.ReadBytes(Int32.Parse(entrie.FileSize.ToString()));

            if (isComp == "CRILAYLA")
            {
                int size = Int32.Parse(entrie.ExtractSize.ToString()) == 0 ? Int32.Parse(entrie.FileSize.ToString()) : Int32.Parse(entrie.ExtractSize.ToString());

                if (size != 0)
                {
                    chunk = CpkContent.DecompressLegacyCRI(chunk, size);
                    return true;
                }
            }
            return false;
        }

        protected string GetFullSaveDirectory(string directory)
            => Path.Combine(SaveDirectory, directory);

        protected static string GetDirectoryWithoutExtension(string directory)
            => Path.Combine(Path.GetDirectoryName(directory), Path.GetFileNameWithoutExtension(directory)).Trim();

        private List<FormatInfo> usedformats = new List<FormatInfo>();
        private readonly object Lock = new object();

        protected FormatInfo GetFormatTypee(Stream stream, string extension = "")
        {
            if (FormatDictionary.Header.TryGetValue(new HeaderInfo(stream).Magic, out FormatInfo Info))
            {
                if (Info.IsMatch.Invoke(stream, extension))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    return Info;
                }
                stream.Seek(0, SeekOrigin.Begin);
            }

            lock (Lock)
            {
                foreach (var item in usedformats)
                {
                    if (item.IsMatch.Invoke(stream, extension))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        return item;
                    }
                    stream.Seek(0, SeekOrigin.Begin);
                }
                FormatInfo info = FormatDictionary.Identify(stream, extension);
                usedformats.Add(info);

                return info;
            }
        }
        #endregion
    }
}
