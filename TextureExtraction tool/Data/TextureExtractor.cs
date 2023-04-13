﻿using AuroraLib.Common;
using AuroraLib.Texture;
using AuroraLib.Texture.Formats;
using Hack.io;
using System.Drawing;
using System.Text;

namespace DolphinTextureExtraction
{
    public class TextureExtractor : ScanBase
    {

        private new ExtractorResult Result => (ExtractorResult)base.Result;

        public class ExtractorOptions : Options
        {
            /// <summary>
            /// Should Mipmaps files be extracted?
            /// </summary>
            public bool Mips = false;

            /// <summary>
            /// use Arbitrary Mipmap Detection.
            /// </summary>
            public bool ArbitraryMipmapDetection = true;

            /// <summary>
            /// Extracts all raw images that are found
            /// </summary>
            public bool Raw = false;

            /// <summary>
            /// Tries to Imitate dolphin mipmap detection.
            /// </summary>
            public bool DolphinMipDetection = true;

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
                if (bool.TryParse(Config.Get("DolphinMipDetection"), out value)) DolphinMipDetection = value;
                if (bool.TryParse(Config.Get("ArbitraryMipmapDetection"), out value)) ArbitraryMipmapDetection = value;
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
            public List<int> Hash = new List<int>();

            public List<FormatInfo> UnsupportedFormatType = new List<FormatInfo>();

            public List<FormatInfo> UnknownFormatType = new List<FormatInfo>();

            public string GetExtractionSize() => MinExtractionRate + MinExtractionRate / 10 >= MaxExtractionRate ? $"{(MinExtractionRate + MaxExtractionRate) / 2}%" : $"{MinExtractionRate}% - {MaxExtractionRate}%";

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Extracted textures: {Extracted}");
                sb.AppendLine($"Unsupported files: {Unsupported}");
                if (Unsupported != 0) sb.AppendLine($"Unsupported files Typs: {string.Join(", ", UnsupportedFormatType.Select(x => x.GetFullDescription()))}");
                sb.AppendLine($"Unknown files: {Unknown}");
                if (UnknownFormatType.Count != 0) sb.AppendLine($"Unknown files Typs: {string.Join(", ", UnknownFormatType.Select(x => x.GetFullType()))}");
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

        private static readonly Dictionary<int, int> ThreadIndices = new Dictionary<int, int>();
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

            return Result;
        }

        #endregion

        #region Scan

        #region main
        protected override void Scan(ScanObjekt so)
        {
            try
            {
                switch (so.Format.Typ)
                {
                    case FormatType.Unknown:
                        if (TryForce(so))
                            break;

                        AddResultUnknown(so.Stream, so.Format, so.SubPath.ToString() + so.Extension);

                        //Exclude files that are too small, for calculation purposes only half the size.
                        if (so.Deep == 0)
                        {
                            if (so.Stream.Length > 130) Result.SkippedSize += so.Stream.Length / 2;
                        }
                        else
                        {
                            if (so.Stream.Length > 300)
                                Result.SkippedSize += so.Stream.Length / 50;
                        }
                        break;
                    case FormatType.Texture:
                        if (((ExtractorOptions)Option).Raw)
                        {
                            Save(so.Stream, Path.Combine("~Raw", so.SubPath.ToString()), so.Format);
                        }
                        if (so.Format.Class != null && so.Format.Class.GetMember(nameof(JUTTexture)) != null)
                        {
                            using (JUTTexture Texture = (JUTTexture)Activator.CreateInstance(so.Format.Class))
                            {
                                Texture.Open(so.Stream);
                                Save(Texture, so.SubPath.ToString());
                            }
                            Result.ExtractedSize += so.Stream.Length;
                            break;
                        }
                        goto case FormatType.Archive;
                    case FormatType.Rom:
                    case FormatType.Archive:

                        if (!TryExtract(so))
                        {

                            if (so.Format.Class == null)
                                AddResultUnsupported(so.Stream, so.SubPath.ToString(), so.Extension, so.Format);
                            else
                            {
                                switch (so.Format.Class.Name)
                                {
                                    case "BDL":
                                        BDL bdlmodel = new BDL(so.Stream);
                                        foreach (var item in bdlmodel.Textures.Textures)
                                        {
                                            Save(item, so.SubPath.ToString());
                                        }
                                        Result.ExtractedSize += so.Stream.Length;
                                        break;
                                    case "BMD":
                                        BMD bmdmodel = new BMD(so.Stream);
                                        foreach (var item in bmdmodel.Textures.Textures)
                                        {
                                            Save(item, so.SubPath.ToString());
                                        }
                                        Result.ExtractedSize += so.Stream.Length;
                                        break;
                                    case "TEX0":
                                        using (JUTTexture Texture = new TEX0(so.Stream)) Save(Texture, so.SubPath.ToString());
                                        Result.ExtractedSize += so.Stream.Length;
                                        break;
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception t)
            {
                Log.WriteEX(t, so.SubPath.ToString() + so.Extension);
                if (!Result.UnsupportedFormatType.Contains(so.Format))
                    Result.UnsupportedFormatType.Add(so.Format);
                Result.Unsupported++;
                Result.UnsupportedSize += so.Stream.Length;
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
                bool? IsArbitraryMipmap = tex.Count > 1 ? ((ExtractorOptions)Option).ArbitraryMipmapDetection ? null : false : false;
                float ArbitraryMipmapValue = 0f;
                int tluts = tex.Palettes.Count == 0 ? 1 : tex.Palettes.Count;
                for (int tlut = 0; tlut < tluts; tlut++)
                {
                    ulong TlutHash = tex.GetTlutHash(tlut);

                    lock (Result.Hash)
                    {
                        int hash = tex.Hash.GetHashCode();

                        if (tex.Format.IsPaletteFormat() && TlutHash != 0)
                            hash = hash * -1521134295 + TlutHash.GetHashCode();

                        //Skip duplicate textures
                        if (Result.Hash.Contains(hash))
                        {
                            continue;
                        }
                        Result.Hash.Add(hash);
                    }

                    // Don't extract anything if performing a dry run
                    if (!Option.DryRun)
                    {
                        string SaveDirectory = GetFullSaveDirectory(subdirectory);
                        Directory.CreateDirectory(SaveDirectory);
                        Bitmap[] bitmaps = new Bitmap[tex.Count];
                        try
                        {
                            for (int i = 0; i < tex.Count; i++)
                            {
                                bitmaps[i] = tex.AsBitmap(i, tlut);
                            }

                            //Is Arbitrary Mipmap?
                            IsArbitraryMipmap ??= (ArbitraryMipmapValue = bitmaps[0].MipmapCompare(bitmaps[1..])) >= 17;

                            //Extract the main texture and mips
                            for (int i = 0; i < tex.Count; i++)
                            {
                                string path = Path.Combine(SaveDirectory, tex.GetDolphinTextureHash(i, TlutHash, ((ExtractorOptions)Option).DolphinMipDetection, IsArbitraryMipmap == true) + ".png");
                                bitmaps[i].Save(path, System.Drawing.Imaging.ImageFormat.Png);
                                //skip mips?
                                if (IsArbitraryMipmap == false && !((ExtractorOptions)Option).Mips) break;
                            }
                        }
                        catch (Exception)
                        {
                            for (int i = 0; i < tex.Count; i++)
                            {
                                bitmaps[i].Dispose();
                            }
                        }
                    }
                    Log.Write(FileAction.Extract, Path.Combine(subdirectory, tex.GetDolphinTextureHash(0, TlutHash, ((ExtractorOptions)Option).DolphinMipDetection, IsArbitraryMipmap == true)) + ".png", $"mips:{tex.Count - 1} WrapS:{tex.WrapS} WrapT:{tex.WrapT} LODBias:{tex.LODBias} MinLOD:{tex.MinLOD} MaxLOD:{tex.MaxLOD} {(tex.Count > 1 ? $"ArbMipValue:{ArbitraryMipmapValue:0.0}" : string.Empty)}");
                    ((ExtractorOptions)Option).TextureAction?.Invoke(tex, Result, subdirectory);
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
        protected override bool TryForce(ScanObjekt so)
        {
            if (((ExtractorOptions)Option).Force)
            {
                if (base.TryForce(so))
                    return true;

                so.Stream.Position = 0;
                if (TryBTI(so.Stream, so.SubPath.ToString()))
                    return true;
                so.Stream.Position = 0;
            }
            else
            {
                if (so.Format.Extension.ToLower() == "")
                {
                    if (TryBTI(so.Stream, so.SubPath.ToString()))
                        return true;
                    so.Stream.Position = 0;
                }
                else if (TryExtract(so))
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
