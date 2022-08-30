using AuroraLip.Common;
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
    public class TextureExtractor : ScanBase
    {
        private readonly ScanLogger Log;

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

        }

        public class ExtractorResult : Results
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

        private TextureExtractor(string meindirectory, string savedirectory) : this(meindirectory, savedirectory, new ExtractorOptions()) { }

        private TextureExtractor(string meindirectory, string savedirectory, ExtractorOptions options) : base(meindirectory, savedirectory, options)
        {
            Directory.CreateDirectory(SaveDirectory);
            base.Result = new ExtractorResult();
            Log = new ScanLogger(SaveDirectory);
            Events.NotificationEvent = Log.WriteNotification;
            Result.LogFullPath = Log.FullPath;
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

        public ExtractorResult StartScan()
        {
            DateTime starttime = DateTime.Now;

            Scan(new DirectoryInfo(ScanDirectory));

            Result.TotalTime = DateTime.Now.Subtract(starttime);
            Log.WriteFoot(Result);
            Log.Dispose();

            if (((ExtractorOptions)Option).Cleanup)
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

            return Result;
        }

        #endregion

        #region Scan

        #region main
        protected override void Scan(FileInfo file)
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
                        if (stream.Length > 130) Result.SkippedSize += stream.Length / 2;
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
                Result.Unsupported++;
                Result.UnsupportedSize += file.Length;
            }
#endif
            stream.Close();
        }

        protected override void Scan(Stream stream, string subdirectory, in string Extension = "")
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
                        if (stream.Length > 300) Result.SkippedSize += stream.Length / 50;
                        break;
                    case FormatType.Texture:
                        if (((ExtractorOptions)Option).Raw)
                        {
                            Save(stream, Path.Combine("~Raw", subdirectory), FFormat);
                        }
                        if (FFormat.Class != null && FFormat.Class.GetMember(nameof(JUTTexture)) != null)
                        {
                            using (JUTTexture Texture = (JUTTexture)Activator.CreateInstance(FFormat.Class))
                            {
                                Texture.Open(stream);
                                Save(Texture, subdirectory);
                            }
                            Result.ExtractedSize += stream.Length;
                            break;
                        }
                        goto case FormatType.Archive;
                    case FormatType.Archive:

                        if (!TryExtract(stream, subdirectory, FFormat))
                        {

                            if (FFormat.Class == null)
                                AddResultUnsupported(stream, subdirectory, Extension, FFormat);
                            else
                            {
                                switch (FFormat.Class.Name)
                                {
                                    case "BDL":
                                        BDL bdlmodel = new BDL(stream);
                                        foreach (var item in bdlmodel.Textures.Textures)
                                        {
                                            Save(item, subdirectory);
                                        }
                                        Result.ExtractedSize += stream.Length;
                                        break;
                                    case "BMD":
                                        BMD bmdmodel = new BMD(stream);
                                        foreach (var item in bmdmodel.Textures.Textures)
                                        {
                                            Save(item, subdirectory);
                                        }
                                        Result.ExtractedSize += stream.Length;
                                        break;
                                    case "TEX0":
                                        using (JUTTexture Texture = new TEX0(stream)) Save(Texture, subdirectory);
                                        Result.ExtractedSize += stream.Length;
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
                Log.WriteEX(t, subdirectory + Extension);
                if (!Result.UnsupportedFormatType.Contains(FFormat)) Result.UnsupportedFormatType.Add(FFormat);
                Result.Unsupported++;
                Result.UnsupportedSize += stream.Length;
            }

#endif
            stream.Close();
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
            finally
            {
                CPKReader.Close();
            }
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
                if (Result.Hash.Contains(tex.Hash))
                {
                    continue;
                }
                Result.Hash.Add(tex.Hash);

                Log.Write(FileAction.Extract, subdirectory, $"Hash:{tex.Hash.ToString("x").PadLeft(16, '0')} Size:{tex[0].Width}x{tex[0].Height} Format:{tex.Format} mips:{tex.Count != 1}{(tex.Count != 1 ? $" {tex.Count}" : "")}");

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
                    $"Bytes32:[{string.Join(",", stream.Read(32))}]");
            }
            else
            {
                Log.Write(FileAction.Unknown, file + $" ~{MathEx.SizeSuffix(stream.Length, 2)}",
                    $"Magic:[{FormatTypee.Header.Magic}] Bytes:[{string.Join(",", FormatTypee.Header.Bytes)}] Offset:{FormatTypee.Header.Offset}");
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
