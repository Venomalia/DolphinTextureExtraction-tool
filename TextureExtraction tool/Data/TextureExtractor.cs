using AuroraLip.Common;
using AuroraLip.Texture.Formats;
using Hack.io;
using LibCPK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AuroraLip.Texture.J3D.JUtility;

namespace DolphinTextureExtraction_tool
{
    public class TextureExtractor : ScanBase
    {

        private new ExtractorResult Result => (ExtractorResult)base.Result;

        public class ExtractorOptions : Options
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
            /// Tries to Imitate dolphin mipmap detection.
            /// </summary>
            public bool DolphinMipDetection = true;

            /// <summary>
            /// Sorts the textures and removes unnecessary folders.
            /// </summary>
            public bool Cleanup = true;

            /// <summary>
            /// is executed when a texture is extracted
            /// </summary>
            public TextureActionDelegate TextureAction;

            public delegate void TextureActionDelegate(JUTTexture.TexEntry texture, Results results, in string subdirectory);

            public ExtractorOptions()
            {
                if (!UseConfig) return;

                if (bool.TryParse(Config.Get("Mips"), out bool value)) Mips = value;
                if (bool.TryParse(Config.Get("Raw"), out value)) Raw = value;
                if (bool.TryParse(Config.Get("Force"), out value)) Force = value;
                if (bool.TryParse(Config.Get("DolphinMipDetection"), out value)) DolphinMipDetection = value;
                if (bool.TryParse(Config.Get("Cleanup"), out value)) Cleanup = value;
            }
        }

        public class ExtractorResult : Results
        {
            public int MinExtractionRate => MathEx.RoundToInt(100d / (ExtractedSize + SkippedSize + UnsupportedSize) * ExtractedSize);

            public int MaxExtractionRate => Extracted > 150 ? MathEx.RoundToInt(100d / (ExtractedSize + SkippedSize / (Extracted / 150) + UnsupportedSize) * ExtractedSize) : MinExtractionRate;

            public int Extracted => Hash.Count();

            public int Unknown { get; internal set; } = 0;

            public int Unsupported { get; internal set; } = 0;

            public int Skipped { get; internal set; } = 0;

            internal long ExtractedSize = 0;

            internal long UnsupportedSize = 0;

            internal long SkippedSize = 0;

            /// <summary>
            /// List of hash values of the extracted textures
            /// </summary>
            public List<ulong> Hash = new List<ulong>();

            public List<FormatInfo> UnsupportedFormatType = new List<FormatInfo>();

            public List<FormatInfo> UnknownFormatType = new List<FormatInfo>();

            public string GetExtractionSize() => MinExtractionRate + MinExtractionRate / 10 >= MaxExtractionRate ? $"{(MinExtractionRate + MaxExtractionRate) / 2}%" : $"{MinExtractionRate}% - {MaxExtractionRate}%";

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Extracted textures: {Extracted}");
                sb.AppendLine($"Unsupported files: {Unsupported}");
                if (Unsupported != 0) sb.AppendLine($"Unsupported files Typs: {string.Join(", ", UnsupportedFormatType.Select(x => x.GetFullDescription()))}".LineBreak(108, "\n                  "));
                sb.AppendLine($"Unknown files: {Unknown}");
                if (UnknownFormatType.Count != 0) sb.AppendLine($"Unknown files Typs: {string.Join(", ", UnknownFormatType.Select(x => x.Header == null || x.Header.MagicASKI.Length < 2 ? x.Extension : $"{x.Extension} \"{x.Header.MagicASKI}\""))}".LineBreak(108, "\n                  "));
                sb.AppendLine($"Extraction rate: ~ {GetExtractionSize()}");
                sb.AppendLine($"Scan time: {TotalTime.TotalSeconds:.000}s");
                return sb.ToString();
            }
        }

        #region Constructor StartScan

        private TextureExtractor(string meindirectory, string savedirectory) : this(meindirectory, savedirectory, new ExtractorOptions()) { }

        private TextureExtractor(string meindirectory, string savedirectory, ExtractorOptions options) : base(meindirectory, savedirectory, options)
        {
            base.Result = new ExtractorResult() { LogFullPath = base.Result.LogFullPath };
        }

        public static ExtractorResult StartScan(string meindirectory, string savedirectory)
            => StartScan_Async(meindirectory, savedirectory, new ExtractorOptions()).Result;

        public static ExtractorResult StartScan(string meindirectory, string savedirectory, ExtractorOptions options)
            => StartScan_Async(meindirectory, savedirectory, options).Result;

        public static Task<ExtractorResult> StartScan_Async(string meindirectory, string savedirectory)
            => StartScan_Async(meindirectory, savedirectory, new ExtractorOptions());

        public static async Task<ExtractorResult> StartScan_Async(string meindirectory, string savedirectory, ExtractorOptions options)
        {
            TextureExtractor Extractor = new TextureExtractor(meindirectory, savedirectory, options);
            return await Task.Run(() => Extractor.StartScan());
        }

        private static Dictionary<int, int> ThreadIndices = new Dictionary<int, int>();
        /// <summary>
        /// Convert a thread's id to a base 1 index, increasing in increments of 1 (Makes logs prettier)
        /// </summary>
        public static int ThreadIndex
        {
            get
            {
                int managed = Thread.CurrentThread.ManagedThreadId;
                if (ThreadIndices.TryGetValue(managed, out int id))
                    return id;
                ThreadIndices.Add(managed, id = ThreadIndices.Count + 1);
                return id;
            }
        }

        public new ExtractorResult StartScan()
        {
            // Reset Thread Indicies, no longer guaranteed to be the same threads as before
            ThreadIndices.Clear();

            base.StartScan();

            if (((ExtractorOptions)Option).Cleanup)
            {
                try
                {
                    Console.WriteLine("Start Cleanup...");
                    Cleanup.Default(new DirectoryInfo(SaveDirectory));
                    Console.WriteLine("Cleanup Completed");
                }
                catch (Exception)
                {
                    Console.WriteLine("Error! Cleanup failed");
                }
            }

            return Result;
        }

        #endregion

        #region Scan

        #region main
        protected override void Scan(Stream Stream, FormatInfo Format, ReadOnlySpan<char> SubPath, int Deep, in string OExtension = "")
        {
#if !DEBUG
            try
            {
#endif
                switch (Format.Typ)
                {
                    case FormatType.Unknown:
                        if (TryForce(Stream, SubPath.ToString(), Format))
                            break;

                        AddResultUnknown(Stream, Format, SubPath.ToString() + OExtension);

                        //Exclude files that are too small, for calculation purposes only half the size.
                        if (Deep == 0)
                        {
                            if (Stream.Length > 130) Result.SkippedSize += Stream.Length / 2;
                        }
                        else
                        {
                            if (Stream.Length > 300)
                                Result.SkippedSize += Stream.Length / 50;
                        }
                        break;
                    case FormatType.Texture:
                        if (((ExtractorOptions)Option).Raw)
                        {
                            Save(Stream, Path.Combine("~Raw", SubPath.ToString()), Format);
                        }
                        if (Format.Class != null && Format.Class.GetMember(nameof(JUTTexture)) != null)
                        {
                            using (JUTTexture Texture = (JUTTexture)Activator.CreateInstance(Format.Class))
                            {
                                Texture.Open(Stream);
                                Save(Texture, SubPath.ToString());
                            }
                            Result.ExtractedSize += Stream.Length;
                            break;
                        }
                        goto case FormatType.Archive;
                    case FormatType.Rom:
                    case FormatType.Archive:

                        if (!TryExtract(Stream, SubPath.ToString(), Format))
                        {

                            if (Format.Class == null)
                                AddResultUnsupported(Stream, SubPath.ToString(), OExtension, Format);
                            else
                            {
                                switch (Format.Class.Name)
                                {
                                    case "BDL":
                                        BDL bdlmodel = new BDL(Stream);
                                        foreach (var item in bdlmodel.Textures.Textures)
                                        {
                                            Save(item, SubPath.ToString());
                                        }
                                        Result.ExtractedSize += Stream.Length;
                                        break;
                                    case "BMD":
                                        BMD bmdmodel = new BMD(Stream);
                                        foreach (var item in bmdmodel.Textures.Textures)
                                        {
                                            Save(item, SubPath.ToString());
                                        }
                                        Result.ExtractedSize += Stream.Length;
                                        break;
                                    case "TEX0":
                                        using (JUTTexture Texture = new TEX0(Stream)) Save(Texture, SubPath.ToString());
                                        Result.ExtractedSize += Stream.Length;
                                        break;
                                }
                            }
                        }
                        break;
                }
#if !DEBUG
            }
            catch (Exception t)
            {
                Log.WriteEX(t, SubPath.ToString() + OExtension);
                if (Deep != 0 || !Result.UnsupportedFormatType.Contains(Format))
                    Result.UnsupportedFormatType.Add(Format);
                Result.Unsupported++;
                Result.UnsupportedSize += Stream.Length;
            }

#endif
        }
        #endregion

        #region CPK
        protected override void scanCPK(Stream stream, string subdirectory)
        {
            CPK CpkContent = new CPK();
            CpkContent.ReadCPK(stream, Encoding.UTF8);
            BinaryReader CPKReader = new BinaryReader(stream);

            foreach (var entries in CpkContent.fileTable)
            {
                string FullPath;
                if (entries.DirName != null)
                {
                    FullPath = Path.Combine(subdirectory, entries.DirName.ToString(), entries.FileName?.ToString());
                }
                else
                {
                    FullPath = Path.Combine(subdirectory, entries.FileName?.ToString());
                }
                try
                {
                    string Extension = Path.GetExtension(entries.FileName?.ToString()).ToLower();
                    if (Extension != "")
                    {
                        try
                        {
                            FormatInfo format = FormatDictionary.Master.First(x => x.Extension == Extension);
                            switch (format.Typ)
                            {
                                case FormatType.Archive:
                                case FormatType.Texture:
                                    break;
                                default:
                                    continue;
                            }
                        }
                        catch (Exception)
                        {
                            Log.Write(FileAction.Unknown, FullPath + $" ~{MathEx.SizeSuffix(entries.ExtractSizePos, 2)}", $"ID:{entries.ID} TOCName:{entries.TOCName}");
                            Result.Unknown++;
                            continue;
                        }
                    }

                    if (entries.FileSize == null)
                        continue;

                    if (CpkDecompressEntrie(CpkContent, CPKReader, entries, out byte[] chunk))
                    {
                        MemoryStream CpkContentStream = new MemoryStream(chunk);
                        Scan(CpkContentStream, FullPath);
                        CpkContentStream.Dispose();
                    }
                }
                catch (Exception t)
                {
                    Log.WriteEX(t, FullPath);
                    Result.Unknown++;
                }
            }
            CPKReader.Close();
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
                lock (Result.Hash)
                {
                    //Skip duplicate textures
                    if (Result.Hash.Contains(tex.Hash))
                    {
                        continue;
                    }
                    Result.Hash.Add(tex.Hash);
                }

                // Don't extract anything if performing a dry run
                if (!Option.DryRun)
                {
                    //Extract the main texture and mips
                    for (int i = 0; i < tex.Count; i++)
                    {
                        string path = GetFullSaveDirectory(subdirectory);
                        Directory.CreateDirectory(path);
                        tex[i].Save(Path.Combine(path, tex.GetDolphinTextureHash(i, false, ((ExtractorOptions)Option).DolphinMipDetection) + ".png"), System.Drawing.Imaging.ImageFormat.Png);

                        //skip mips?
                        if (!((ExtractorOptions)Option).Mips) break;
                    }
                }

                Log.Write(FileAction.Extract, Path.Combine(subdirectory, tex.GetDolphinTextureHash()) + ".png", $"mips:{tex.Count - 1} WrapS:{tex.WrapS} WrapT:{tex.WrapT} LODBias:{tex.LODBias} MinLOD:{tex.MinLOD} MaxLOD:{tex.MaxLOD}");
                ((ExtractorOptions)Option).TextureAction?.Invoke(tex, Result, subdirectory);
            }
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
        protected override bool TryForce(Stream stream, string subdirectory, FormatInfo FFormat)
        {
            if (((ExtractorOptions)Option).Force)
            {
                if (base.TryForce(stream, subdirectory, FFormat))
                    return true;

                stream.Position = 0;
                if (TryBTI(stream, subdirectory))
                    return true;
                stream.Position = 0;
            }
            else
            {
                if (FFormat.Extension.ToLower() == "")
                {
                    if (TryBTI(stream, subdirectory))
                        return true;
                    stream.Position = 0;
                }
                else if (TryExtract(stream, subdirectory, FFormat))
                    return true;
            }
            return false;
        }

        private void AddResultUnsupported(Stream stream, string subdirectory, string Extension, FormatInfo FFormat)
        {
            Log.Write(FileAction.Unsupported, subdirectory + Extension + $" ~{MathEx.SizeSuffix(stream.Length, 2)}", $"Description: {FFormat.GetFullDescription()}");
            if (!Result.UnsupportedFormatType.Contains(FFormat)) Result.UnsupportedFormatType.Add(FFormat);
            Result.Unsupported++;
            Result.UnsupportedSize += stream.Length;
        }

        private void AddResultUnknown(Stream stream, FormatInfo FormatTypee, in string file)
        {
            if (FormatTypee.Header == null || FormatTypee.Header?.Magic.Length <= 3)
            {
                Log.Write(FileAction.Unknown, file + $" ~{MathEx.SizeSuffix(stream.Length, 2)}",
                    $"Bytes32:[{BitConverter.ToString(stream.Read(32))}]");
            }
            else
            {
                Log.Write(FileAction.Unknown, file + $" ~{MathEx.SizeSuffix(stream.Length, 2)}",
                    $"Magic:[{FormatTypee.Header.Magic}] Bytes:[{BitConverter.ToString(FormatTypee.Header.Bytes)}] Offset:{FormatTypee.Header.Offset}");
            }

            if (stream.Length > 130)
            {
                if (!Result.UnknownFormatType.Contains(FormatTypee)) Result.UnknownFormatType.Add(FormatTypee);
            }
            Result.Unknown++;
        }
        #endregion


    }
}
