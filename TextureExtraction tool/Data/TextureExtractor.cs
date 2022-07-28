using AFSLib;
using AuroraLip.Archives;
using AuroraLip.Common;
using AuroraLip.Compression;
using AuroraLip.Texture.Formats;
using Hack.io.BMD;
using LibCPK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AuroraLip.Texture.J3D.JUtility;

namespace DolphinTextureExtraction_tool
{
    public class TextureExtractor
    {

        private readonly string ScanDirectory;

        private readonly string SaveDirectory;

        private readonly ScanLogger Log;

        private readonly Options options = new Options();

        private readonly Result result = new Result();

        public class Options
        {
            /// <summary>
            /// Should Mipmaps files be extracted?
            /// </summary>
            public bool Mips = true;

            /// <summary>
            /// Extracts all raw images that are found
            /// </summary>
            public bool Raw = false;

            /// <summary>
            /// Tries to extract textures from unknown files, may cause errors.
            /// </summary>
            public bool Force = false;

            /// <summary>
            /// Sorts the textures and removes unnecessary folders.
            /// </summary>
            public bool Cleanup = true;

            /// <summary>
            /// Tries to extract textures from unknown files, may cause errors.
            /// </summary>
#if DEBUG
            public ParallelOptions ParallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 1 };
#else
            public ParallelOptions ParallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 4 };
#endif

        }

        public class Result
        {
            public TimeSpan TotalTime { get; internal set; }

            public int MinExtractionRate => (int)Math.Round((double)100 / (ExtractedSize + SkippedSize + UnsupportedSize) * ExtractedSize);

            public int MaxExtractionRate
            {
                get
                {
                    if (Extracted > 150) return (int)Math.Round((double)100 / (ExtractedSize + SkippedSize / (Extracted / 150) + UnsupportedSize) * ExtractedSize);
                    return MinExtractionRate;
                }
            }

            public int Extracted => Hash.Count();

            public int Unknown { get; internal set; } = 0;

            public int Unsupported { get; internal set; } = 0;

            public int Skipped { get; internal set; } = 0;

            public string LogFullPath { get; internal set; }

            internal long ExtractedSize = 0;

            internal long UnsupportedSize = 0;

            internal long SkippedSize = 0;

            /// <summary>
            /// List of hash values of the extracted textures
            /// </summary>
            public List<ulong> Hash = new List<ulong>();

            public List<FormatInfo> UnsupportedFormatType = new List<FormatInfo>();

            public List<FormatInfo> UnknownFormatType = new List<FormatInfo>();

            public string GetExtractionSize()
            {
                if (MinExtractionRate + MinExtractionRate / 10 >= MaxExtractionRate)
                {
                    return $"{(int)(MinExtractionRate + MaxExtractionRate) / 2}%";
                }
                return $"{MinExtractionRate}% - {MaxExtractionRate}%";
            }
        }


        #region Constructor StartScan

        private TextureExtractor(string meindirectory, string savedirectory, Options options = null)
        {
            ScanDirectory = meindirectory;
            SaveDirectory = savedirectory;
            Log = new ScanLogger(SaveDirectory);
            result.LogFullPath = Log.FullPath;
            if (options != null)
            {
                this.options = options;
            }
        }

        public static Result StartScan(string meindirectory, string savedirectory, Options options = null) => StartScan_Async(meindirectory, savedirectory, options).Result;

        public static async Task<Result> StartScan_Async(string meindirectory, string savedirectory, Options options = null)
        {
            TextureExtractor Extractor = new TextureExtractor(meindirectory, savedirectory, options);
            return await Task.Run(() => Extractor.StartScan());
        }

        public Result StartScan()
        {
            DateTime starttime = DateTime.Now;

            Scan(new DirectoryInfo(ScanDirectory));

            result.TotalTime = DateTime.Now.Subtract(starttime);
            Log.WriteFoot(result);
            Log.Dispose();

            if (options.Cleanup)
            {
                try
                {
                    Console.WriteLine("Start Cleanup...");
                    Cleanup.Default(new DirectoryInfo(SaveDirectory));
                }
                catch (Exception)
                {
                    Console.WriteLine("Error! Cleanup failed");
                }
            }

            return result;
        }

        #endregion

        #region Scan

        #region main

        private void Scan(DirectoryInfo directory)
        {
            foreach (DirectoryInfo subdirectory in directory.GetDirectories())
            {
                Scan(subdirectory);
            }

            Parallel.ForEach(directory.GetFiles(), options.ParallelOptions, (FileInfo file) =>
            {
                Scan(file);
            });
        }

        private void Scan(FileInfo file)
        {
            Stream stream = new FileStream(file.FullName, FileMode.Open);
            FormatInfo FFormat = GetFormatTypee(stream, file.Extension);

            string subdirectory = GetDirectoryWithoutExtension(file.FullName.Replace(ScanDirectory + Path.DirectorySeparatorChar, ""));

#if !DEBUG
            try
            {
#endif
                switch (FFormat.Typ)
                {
                    case FormatType.Unknown:
                        if (TryForce(stream, subdirectory, FFormat))
                            break;

                        AddResultUnknown(stream, FFormat, subdirectory + file.Extension);
                        //Exclude files that are too small, for calculation purposes only half the size.
                        if (stream.Length > 130) result.SkippedSize += stream.Length / 2;
                        break;
                    case FormatType.Archive:
                    case FormatType.Texture:
                        switch (FFormat.Extension.ToLower())
                        {
                            case "cpk":
                                stream.Close();
                                scanCPK(file.FullName, subdirectory);
                                break;
                            default:
                                Scan(stream, subdirectory, file.Extension);
                                break;
                        }
                        break;
                    default:
                        break;
                }

#if !DEBUG
            }
            catch (Exception t)
            {
                Log.WriteEX(t, subdirectory + file.Extension);
                result.Unsupported++;
                result.UnsupportedSize += file.Length;
            }
#endif
            stream.Close();
        }

        private void Scan(Stream stream, string subdirectory, in string Extension = "")
        {
            FormatInfo FFormat = GetFormatTypee(stream, Extension);
            subdirectory = GetDirectoryWithoutExtension(subdirectory);

#if !DEBUG
            try
            {
#endif
                switch (FFormat.Typ)
                {
                    case FormatType.Unknown:
                        if (TryForce(stream, subdirectory, FFormat))
                            break;
                        AddResultUnknown(stream, FFormat, subdirectory + Extension);
                        if (stream.Length > 300) result.SkippedSize += stream.Length / 50;
                        break;
                    case FormatType.Texture:
                        if (options.Raw)
                        {
                            Save(stream, Path.ChangeExtension(GetFullSaveDirectory(Path.Combine("~Raw", subdirectory)), FFormat.Extension));
                        }
                        if (FFormat.Class != null && FFormat.Class.GetMember(nameof(JUTTexture)) != null)
                        {
                            using (JUTTexture Texture = (JUTTexture)Activator.CreateInstance(FFormat.Class))
                            {
                                Texture.Open(stream);
                                Save(Texture, subdirectory);
                            }
                            result.ExtractedSize += stream.Length;
                            break;
                        }
                        goto case FormatType.Archive;
                    case FormatType.Archive:

                        if (FFormat.Class == null)
                        {
                            switch (FFormat.Extension.ToLower())
                            {
                                case ".lz":
                                    if (Reflection.Compression.TryToDecompress(stream, out Stream test, out _))
                                    {
                                        Scan(test, subdirectory);
                                        break;
                                    }
                                    if (Reflection.Compression.TryToFindMatch(stream, out Type type))
                                    {
                                        Log.Write(FileAction.Unsupported, subdirectory + Extension + $" ~{MathEx.SizeSuffix(stream.Length, 2)}", $"Description: {FFormat.GetFullDescription()} Algorithm:{type.Name}?");
                                        if (!result.UnsupportedFormatType.Contains(FFormat)) result.UnsupportedFormatType.Add(FFormat);
                                        result.Unsupported++;
                                        result.UnsupportedSize += stream.Length;
                                    }
                                    else AddResultUnsupported(stream, subdirectory, Extension, FFormat);

                                    break;
                                default:
                                    AddResultUnsupported(stream, subdirectory, Extension, FFormat);
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
                                }
                                break;
                            }
                            if (FFormat.Class.GetInterface(nameof(ICompression)) != null)
                            {
                                Scan(((ICompression)Activator.CreateInstance(FFormat.Class)).Decompress(stream), subdirectory);
                                break;
                            }

                            //External classes
                            switch (FFormat.Class.Name)
                            {
                                case "BDL":
                                    BDL bdlmodel = new BDL(stream);
                                    foreach (var item in bdlmodel.Textures.Textures)
                                    {
                                        Save(item, subdirectory);
                                    }
                                    result.ExtractedSize += stream.Length;
                                    break;
                                case "BMD":
                                    BMD bmdmodel = new BMD(stream);
                                    foreach (var item in bmdmodel.Textures.Textures)
                                    {
                                        Save(item, subdirectory);
                                    }
                                    result.ExtractedSize += stream.Length;
                                    break;
                                case "AFS":
                                    using (AFS afs = new AFS(stream))
                                    {
                                        foreach (Entry item in afs.Entries)
                                            if (item is StreamEntry Streamitem)
                                                Scan(Streamitem.GetSubStream(), Path.Combine(subdirectory, Streamitem.SanitizedName), Path.GetExtension(Streamitem.SanitizedName));
                                    }
                                    break;
                                case "TEX0":
                                    using (JUTTexture Texture = new TEX0(stream)) Save(Texture, subdirectory);
                                    result.ExtractedSize += stream.Length;
                                    break;
                            }
                        }
                        break;
                }

#if !DEBUG
            }
            catch (Exception t)
            {
                Log.WriteEX(t, subdirectory + Extension);
                if (!result.UnsupportedFormatType.Contains(FFormat)) result.UnsupportedFormatType.Add(FFormat);
                result.Unsupported++;
                result.UnsupportedSize += stream.Length;
            }

#endif
            stream.Close();
        }

        #endregion

        #region Archive

        private void Scan(Archive archiv, string subdirectory)
        {
            ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 1 };
            if (archiv.TotalFileCount > 30)
                parallelOptions = options.ParallelOptions;

            Parallel.ForEach(archiv.Root.Items, parallelOptions, (KeyValuePair<string,object> item) =>
            {
                if (item.Value is ArchiveFile file)
                {
                    Scan(file, subdirectory);
                }
                if (item.Value is ArchiveDirectory directory)
                {
                    if (directory.Name.Length > 4)
                        Scan(directory, Path.Combine(subdirectory, directory.Name));
                    else
                        Scan(directory, subdirectory);
                }
            });
        }

        private void Scan(ArchiveDirectory archivdirectory, in string subdirectory)
        {

            foreach (var item in archivdirectory.Items)
            {
                if (item.Value is ArchiveFile file)
                {
                    Scan(file, subdirectory);
                }
                if (item.Value is ArchiveDirectory directory)
                {
                    if (directory.Name.Length > 4)
                        Scan(directory, Path.Combine(subdirectory, directory.Name));
                    else
                        Scan(directory, subdirectory);
                }
            }
        }

        private void Scan(ArchiveFile file, in string subdirectory)
        {
            Scan(file.FileData, Path.Combine(subdirectory, Path.GetFileNameWithoutExtension(file.Name)), file.Extension.ToLower());
        }

        #endregion

        #region CPK

        private void scanCPK(string file, string subdirectory)
        {
            CPK CpkContent = new CPK();
            CpkContent.ReadCPK(file, Encoding.UTF8);

            BinaryReader CPKReader = new BinaryReader(File.OpenRead(file));
            try
            {
                foreach (var entries in CpkContent.fileTable)
                {
                    {
                        try
                        {
                            FormatInfo format = FormatDictionary.Master.First(x => x.Extension == Path.GetExtension(entries.FileName.ToString()).ToLower());
                            switch (format.Typ)
                            {
                                case FormatType.Unknown:
                                    break;
                                case FormatType.Archive:
                                case FormatType.Texture:
                                    if (CpkDecompressEntrie(CpkContent, CPKReader, entries, out byte[] chunk))
                                    {
                                        MemoryStream CpkContentStream = new MemoryStream(chunk);
                                        Scan(CpkContentStream, Path.Combine(subdirectory, Path.GetFileNameWithoutExtension(entries.FileName.ToString())));
                                        CpkContentStream.Dispose();
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
            finally
            {
                CPKReader.Close();
            }
        }

        private bool CpkDecompressEntrie(CPK CpkContent, BinaryReader CPKReader, LibCPK.FileEntry entrie, out byte[] chunk)
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

        #endregion

        #endregion

        #region Extract

        /// <summary>
        /// Extract the texture
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="subdirectory"></param>
        private void Save(JUTTexture texture, in string subdirectory)
        {
            foreach (JUTTexture.TexEntry tex in texture)
            {
                //Skip duplicate textures
                if (result.Hash.Contains(tex.Hash))
                {
                    continue;
                }
                result.Hash.Add(tex.Hash);

                Log.Write(FileAction.Extract, subdirectory, $"Hash:{tex.Hash.ToString("x").PadLeft(16, '0')} Size:{tex[0].Width}x{tex[0].Height} Format:{tex.Format} mips:{tex.Count != 1}{(tex.Count != 1 ? $" {tex.Count}" : "")}");

                //Extract the main texture and mips
                for (int i = 0; i < tex.Count; i++)
                {
                    string path = GetFullSaveDirectory(subdirectory);
                    Directory.CreateDirectory(path);
                    tex[i].Save(Path.Combine(path, tex.GetDolphinTextureHash(i) + ".png"), System.Drawing.Imaging.ImageFormat.Png);

                    //skip mips?
                    if (!options.Mips) break;
                }
            }
        }

        /// <summary>
        /// Writes a Steam to a new file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="destFileName"></param>
        private void Save(Stream stream, string destFileName)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destFileName));
            using (FileStream file = new FileStream(destFileName, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(file);
            }
            stream.Position = 0;
        }

        /// <summary>
        /// Try to read a file as bti
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="subdirectory"></param>
        /// <returns></returns>
        private bool TryBTI(Stream stream, string subdirectory)
        {
            if (Enum.IsDefined(typeof(GXImageFormat), (byte)stream.ReadByte()) && Enum.IsDefined(typeof(JUTTransparency), (byte)stream.ReadByte()))
            {
                ushort ImageWidth = stream.ReadUInt16(Endian.Big);
                ushort ImageHeight = stream.ReadUInt16(Endian.Big);

                if (ImageWidth > 4 && ImageHeight > 4 && ImageWidth < 1024 && ImageHeight < 1024)
                {
                    stream.Position -= 6;
                    try
                    {
                        Save(new BTI(stream), Path.Combine("~Force", subdirectory));
                        return true;
                    }
                    catch (Exception)
                    { }
                }
            }
            return false;
        }

        #endregion

        #region Helper

        private bool TryForce(Stream stream, string subdirectory, FormatInfo FFormat)
        {
            if (options.Force)
            {
                if (Reflection.Compression.TryToDecompress(stream, out Stream test, out _))
                {
                    Scan(test, subdirectory);
                    return true;
                }

                stream.Position = 0;
                if (TryBTI(stream, subdirectory))
                    return true;
                stream.Position = 0;
            }
            else
            {
                switch (FFormat.Extension.ToLower())
                {
                    case "":
                        if (TryBTI(stream, subdirectory))
                            return true;
                        stream.Position = 0;
                        break;
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
            return false;
        }

        private void AddResultUnsupported(Stream stream, string subdirectory, string Extension, FormatInfo FFormat)
        {
            Log.Write(FileAction.Unsupported, subdirectory + Extension + $" ~{MathEx.SizeSuffix(stream.Length, 2)}", $"Description: {FFormat.GetFullDescription()}");
            if (!result.UnsupportedFormatType.Contains(FFormat)) result.UnsupportedFormatType.Add(FFormat);
            result.Unsupported++;
            result.UnsupportedSize += stream.Length;
        }

        private void AddResultUnknown(Stream stream, FormatInfo FormatTypee, in string file)
        {
            if (FormatTypee.Header == null || FormatTypee.Header?.Magic.Length <= 3)
            {
                Log.Write(FileAction.Unknown, file + $" ~{MathEx.SizeSuffix(stream.Length, 2)}",
                    $"Bytes32:[{string.Join(",", stream.Read(32))}]");
            }
            else
            {
                Log.Write(FileAction.Unknown, file + $" ~{MathEx.SizeSuffix(stream.Length, 2)}",
                    $"Magic:[{FormatTypee.Header.Magic}] Bytes:[{string.Join(",", FormatTypee.Header.Bytes)}] Offset:{FormatTypee.Header.Offset}");
            }

            if (stream.Length > 130)
            {
                if (!result.UnknownFormatType.Contains(FormatTypee)) result.UnknownFormatType.Add(FormatTypee);
            }
            result.Unknown++;
        }

        private string GetFullSaveDirectory(string directory)
        {
            return Path.Combine(SaveDirectory, directory);
        }

        private static string GetDirectoryWithoutExtension(string directory)
        {
            return Path.Combine(Path.GetDirectoryName(directory), Path.GetFileNameWithoutExtension(directory)).Trim();
        }

        private List<FormatInfo> usedformats = new List<FormatInfo>();
        private readonly object Lock = new object();
        private FormatInfo GetFormatTypee(Stream stream, string extension = "")
        {
            if (FormatDictionary.TryGetValue(new HeaderInfo(stream).Magic, out FormatInfo Info))
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
