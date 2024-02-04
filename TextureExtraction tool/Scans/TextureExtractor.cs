using AuroraLib.Archives;
using AuroraLib.Archives.Formats;
using AuroraLib.Common;
using AuroraLib.Common.Interfaces;
using AuroraLib.Texture;
using AuroraLib.Texture.Formats;
using DolphinTextureExtraction.Scans.Helper;
using DolphinTextureExtraction.Scans.Options;
using DolphinTextureExtraction.Scans.Results;
using Hack.io;
using SixLabors.ImageSharp;
using System.Runtime.CompilerServices;
using System.Text;

namespace DolphinTextureExtraction.Scans
{
    public class TextureExtractor : ScanBase
    {

        private new TextureExtractorResult Result => (TextureExtractorResult)base.Result;

        private new TextureExtractorOptions Option => (TextureExtractorOptions)base.Option;

        #region Constructor StartScan

        private TextureExtractor(string meindirectory, string savedirectory) : this(meindirectory, savedirectory, new TextureExtractorOptions()) { }

        private TextureExtractor(string meindirectory, string savedirectory, TextureExtractorOptions options, string logDirectory = null) : base(meindirectory, savedirectory, options, logDirectory)
            => base.Result = new TextureExtractorResult() { LogFullPath = base.Result.LogFullPath };

        public static TextureExtractorResult StartScan(string meindirectory, string savedirectory)
            => StartScan_Async(meindirectory, savedirectory, new TextureExtractorOptions()).Result;

        public static TextureExtractorResult StartScan(string meindirectory, string savedirectory, TextureExtractorOptions options, string logDirectory = null)
            => StartScan_Async(meindirectory, savedirectory, options, logDirectory).Result;

        public static Task<TextureExtractorResult> StartScan_Async(string meindirectory, string savedirectory)
            => StartScan_Async(meindirectory, savedirectory, new TextureExtractorOptions());

        public static async Task<TextureExtractorResult> StartScan_Async(string meindirectory, string savedirectory, TextureExtractorOptions options, string logDirectory = null)
        {
            TextureExtractor Extractor = new(meindirectory, savedirectory, options, logDirectory);
#if DEBUG
            if (Extractor.Option.Parallel.MaxDegreeOfParallelism == 1)
            {
                TextureExtractorResult result = Extractor.StartScan();
                return await Task.Run(() => result);
            }
#endif
            return await Task.Run(() => Extractor.StartScan());
        }

        public new TextureExtractorResult StartScan()
        {
            base.StartScan();
            return Result;
        }

        #endregion

        #region Scan
        protected override void Scan(ScanObjekt so)
        {
            try
            {
                LogScanObjekt(so);
                switch (so.Format.Typ)
                {
                    case FormatType.Unknown:
                        if (TryForce(so))
                            break;

                        LogResultUnknown(so);
                        break;
                    case FormatType.Texture:
                        if (Option.Raw)
                            Save(so.Stream, Path.Combine("~Raw", so.SubPath.ToString()), so.Format);
                        if (so.Format.Class != null && so.Format.Class.GetMember(nameof(JUTTexture)) != null)
                        {
                            using (JUTTexture Texture = (JUTTexture)Activator.CreateInstance(so.Format.Class))
                            {
                                Texture.Open(so.Stream);
                                Save(Texture, so);
                            }
                            Result.ExtractedSize += so.Stream.Length;
                            break;
                        }
                        goto case FormatType.Archive;
                    case FormatType.Iso:
                        if (so.Format.Class.Name != nameof(WAD) && so.Format.Class.IsSubclassOf(typeof(Archive)))
                        {

                            int Parallelism = Option.Parallel.MaxDegreeOfParallelism;
                            if (Option.Parallel.MaxDegreeOfParallelism != 1 && so.Format.Class.Name != nameof(GCDisk))
                            {
                                // Most disk images are faster in single-tasking mode.
                                Option.Parallel.MaxDegreeOfParallelism = 1;
                            }

                            using Archive archive = (Archive)Activator.CreateInstance(so.Format.Class);
                            archive.Open(so.Stream);

                            // For Wii games we skip the update partition.
                            const string UPDATE = "UPDATE";
                            if (archive.Root.Items.TryGetValue(UPDATE, out ArchiveObject partiton))
                            {
                                Log.WriteNotification(NotificationType.Info, $"INFO: {archive.Root.Name} update partition skipped.");
                                partiton.Dispose();
                                archive.Root.Items.Remove(UPDATE);
                            }

                            // We use the name that is embedded in the game.
                            string aPath = PathX.GetValidPath(Path.Join(Path.GetDirectoryName(so.SubPath), archive.Root.Name));
                            Scan(archive, aPath, so.Deep + 1);
                            Option.Parallel.MaxDegreeOfParallelism = Parallelism;

                            // We save the game ID as txt for dolphin
                            if (Result.Extracted != 0 && archive is IGameDetails details)
                            {
                                string identifierDir = Path.Join(aPath, "_GameID");
                                identifierDir = GetFullSaveDirectory(identifierDir);
                                Directory.CreateDirectory(identifierDir);
                                string identifierPath = Path.Join(identifierDir, $"{details.GameID.GetString().AsSpan(0, 3)}.txt");
                                File.Create(identifierPath).Close();
                            }
                            break;
                        }
                        goto case FormatType.Archive;
                    case FormatType.Archive:

                        if (!TryExtract(so))
                        {

                            if (so.Format.Class == null)
                            {
                                LogResultUnsupported(so);
                            }
                            else
                            {
                                switch (so.Format.Class.Name)
                                {
                                    case "BDL":
                                        BDL bdlmodel = new(so.Stream);
                                        foreach (var item in bdlmodel.Textures.Textures)
                                        {
                                            Save(item, so);
                                        }
                                        Result.ExtractedSize += so.Stream.Length;
                                        break;
                                    case "BMD":
                                        BMD bmdmodel = new(so.Stream);
                                        foreach (var item in bmdmodel.Textures.Textures)
                                        {
                                            Save(item, so);
                                        }
                                        Result.ExtractedSize += so.Stream.Length;
                                        break;
                                    case "TEX0":
                                        using (JUTTexture Texture = new TEX0(so.Stream)) Save(Texture, so);
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
                lock (Result)
                {
                    Log.WriteEX(t, string.Concat(so.SubPath, so.Extension));
                    Result.AddUnsupported(so);
                }
#if DEBUG
                if (so.Format.Typ == FormatType.Texture)
                {
                    string paht = Path.Combine("~Exception", so.SubPath.ToString());
                    Save(so.Stream, paht, so.Format);
                }
#endif
            }

        }
        #endregion

        #region Extract
        /// <summary>
        /// Extract the texture
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="subdirectory"></param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void Save(JUTTexture texture, ScanObjekt so)
        {
            foreach (JUTTexture.TexEntry tex in texture)
            {
                bool? IsArbitraryMipmap = tex.Count > 1 ? Option.ArbitraryMipmapDetection ? null : false : false;
                float ArbitraryMipmapValue = 0f;

                int tluts = tex.Palettes.Count == 0 ? 1 : tex.Palettes.Count;
                for (int tlut = 0; tlut < tluts; tlut++)
                {
                    string mainTextureName = string.Empty;
                    ulong tlutHash = tex.GetTlutHash(tlut);

                    // If we already have the texture we skip it
                    if (Result.AddHashIfNeeded(tex, tlutHash))
                        continue;

                    // Don't extract anything if performing a dry run
                    if (!Option.DryRun)
                    {
                        string SaveDirectory = GetFullSaveDirectory(so.SubPath);
                        Directory.CreateDirectory(SaveDirectory);


                        Image[] image = new Image[tex.Count];
                        try
                        {
                            // Combined IA8 palette texture to RGBA?
                            bool rgbaCombined = Option.CombinedRGBA && tluts == 2 && tex.PaletteFormat == GXPaletteFormat.IA8;
                            if (rgbaCombined)
                            {
                                for (int i = 0; i < tex.Count; i++)
                                {
                                    image[i] = tex.GetFullImage(i);
                                }
                                tlut++;
                            }
                            else
                            {
                                for (int i = 0; i < tex.Count; i++)
                                {
                                    image[i] = tex.GetImage(i, tlut);
                                }
                            }

                            //Is Arbitrary Mipmap?
                            IsArbitraryMipmap ??= (ArbitraryMipmapValue = image.MipmapCompare()) >= 0.18;

                            //Extract the main texture and mips
                            for (int i = 0; i < tex.Count; i++)
                            {
                                //If a combined texture we need a second TLUThash. 
                                ulong tlutHash2 = 0;
                                if (rgbaCombined)
                                {
                                    tlutHash2 = tex.GetTlutHash(tlut);
                                    Result.AddHashIfNeeded(tex, tlutHash2);
                                }

                                //Create the path and save the texture.
                                string textureName = tex.GetDolphinTextureHash(i, tlutHash, Option.DolphinMipDetection, IsArbitraryMipmap == true, tlutHash2) + ".png";
                                string path = Path.Combine(SaveDirectory, textureName);
                                image[i].SaveAsPng(path);

                                //We save the main level texture path for later
                                if (i == 0)
                                    mainTextureName = textureName;

                                //skip mips?
                                if (IsArbitraryMipmap == false && !Option.Mips) break;
                            }
                        }
                        catch (Exception t)
                        {
                            Log.WriteEX(t, string.Concat(so.SubPath, tex.ToString()));
                            Result.AddUnsupported(so);
                        }
                        finally
                        {
                            for (int i = 0; i < tex.Count; i++)
                            {
                                image[i]?.Dispose();
                            }
                        }

                        string subFilePath = Path.Join(so.SubPath, mainTextureName);
                        string texInfo = BuildTextureInfos(tex, ArbitraryMipmapValue);

                        Log.Write(FileAction.Extract, subFilePath, texInfo);
                        Option.ListPrintAction?.Invoke(Result, "Extract", subFilePath, texInfo);
                    }
                }
            }
        }

        private static string BuildTextureInfos(JUTTexture.TexEntry tex, float ArbitraryMipmapValue = 0f)
        {
            StringBuilder sb = new();
            sb.Append("Mips:");
            sb.Append(tex.Count - 1);
            sb.Append(" WrapS:");
            sb.Append(tex.WrapS);
            sb.Append(" WrapT:");
            sb.Append(tex.WrapT);
            sb.Append(" LODBias:");
            sb.Append(tex.LODBias);
            sb.Append(" MinLOD:");
            sb.Append(tex.MinLOD);
            sb.Append(" MaxLOD:");
            sb.Append(tex.MinLOD);
            if (tex.Count > 1 && ArbitraryMipmapValue > 0f)
            {
                sb.Append($" ArbMipValue:{ArbitraryMipmapValue:0.000}");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Try to read a file as bti
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="subdirectory"></param>
        /// <returns></returns>
        private bool TryBTI(Stream stream, ScanObjekt so)
        {
            if (stream.Length - stream.Position <= Unsafe.SizeOf<BTI.ImageHeader>())
                return false;
            var ImageHeader = stream.Read<BTI.ImageHeader>(Endian.Big);
            stream.Position -= Unsafe.SizeOf<BTI.ImageHeader>();
            if (
                Enum.IsDefined(ImageHeader.Format) &&
                Enum.IsDefined(ImageHeader.AlphaSetting) &&
                Enum.IsDefined(ImageHeader.PaletteFormat) &&
                Enum.IsDefined(ImageHeader.WrapS) &&
                Enum.IsDefined(ImageHeader.WrapT) &&
                Enum.IsDefined(ImageHeader.MagnificationFilter) &&
                Enum.IsDefined(ImageHeader.MinificationFilter) &&
                ImageHeader.Width > 4 && ImageHeader.Width < 1024 &&
                ImageHeader.Height > 4 && ImageHeader.Height < 1024
                )
            {
                try
                {
                    string paht = Path.Combine("~Force", so.SubPath.ToString());
                    so = new ScanObjekt(so.Stream, paht, so.Deep, so.Extension);
                    using BTI bit = new(stream);
                    Save(bit, so);
                    return true;
                }
                catch (Exception)
                { }
            }
            return false;
        }

        #endregion

        #region Helper
        protected override bool TryForce(ScanObjekt so)
        {
            if (((TextureExtractorOptions)Option).Force)
            {
                if (base.TryForce(so))
                    return true;

                so.Stream.Position = 0;
                if (TryBTI(so.Stream, so))
                    return true;
                so.Stream.Position = 0;
            }
            else
            {
                if (so.Format.Extension == "")
                {
                    if (TryBTI(so.Stream, so))
                        return true;
                    so.Stream.Position = 0;
                }
                else if (TryExtract(so))
                    return true;
            }
            return false;
        }

        private void LogResultUnsupported(ScanObjekt so)
        {
            Log.Write(FileAction.Unsupported, so.GetFullSubPath() + $" ~{PathX.AddSizeSuffix(so.Stream.Length, 2)}", $"Description: {so.Format.GetFullDescription()}");
            Result.AddUnsupported(so);
        }

        protected override void LogResultUnknown(ScanObjekt so)
        {
            base.LogResultUnknown(so);
            Result.AddUnknown(so);
        }
        #endregion


    }
}
