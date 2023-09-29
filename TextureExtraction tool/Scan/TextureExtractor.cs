using AuroraLib.Common;
using AuroraLib.Texture;
using AuroraLib.Texture.Formats;
using DolphinTextureExtraction.Scans.Helper;
using DolphinTextureExtraction.Scans.Options;
using DolphinTextureExtraction.Scans.Results;
using Hack.io;
using SixLabors.ImageSharp;
using System.Runtime.CompilerServices;

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
                switch (so.Format.Typ)
                {
                    case FormatType.Unknown:
                        if (TryForce(so))
                            break;

                        AddResultUnknown(so.Stream, so.Format, string.Concat(so.SubPath, so.Extension));

                        //Exclude files that are too small, for calculation purposes only half the size.
                        if (so.Deep == 0)
                        {
                            if (so.Stream.Length > 300)
                                Result.SkippedSize += so.Stream.Length >> 1;
                        }
                        else
                        {
                            if (so.Stream.Length > 512)
                                Result.SkippedSize += so.Stream.Length >> 6;
                        }
                        break;
                    case FormatType.Texture:
                        if (((TextureExtractorOptions)Option).Raw)
                            Save(so.Stream, Path.Combine("~Raw", so.SubPath.ToString()), so.Format);
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
                    case FormatType.Iso:
                    case FormatType.Archive:

                        if (!TryExtract(so))
                        {

                            if (so.Format.Class == null)
                                AddResultUnsupported(so.Stream, so.SubPath.ToString(), so.Extension.ToString(), so.Format);
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
                Log.WriteEX(t, string.Concat(so.SubPath, so.Extension));
                if (!Result.UnsupportedFormatType.Contains(so.Format))
                    Result.UnsupportedFormatType.Add(so.Format);
                Result.Unsupported++;
                Result.UnsupportedSize += so.Stream.Length;
            }

        }
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
                bool? IsArbitraryMipmap = tex.Count > 1 ? Option.ArbitraryMipmapDetection ? null : false : false;
                float ArbitraryMipmapValue = 0f;

                int tluts = tex.Palettes.Count == 0 ? 1 : tex.Palettes.Count;
                for (int tlut = 0; tlut < tluts; tlut++)
                {
                    string mainTextureHash = string.Empty;
                    ulong tlutHash = tex.GetTlutHash(tlut);

                    // If we already have the texture we skip it
                    if (TestTextureHashCode(tex, tlutHash))
                        continue;

                    // Don't extract anything if performing a dry run
                    if (!Option.DryRun)
                    {
                        string SaveDirectory = GetFullSaveDirectory(subdirectory);
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
                                    TestTextureHashCode(tex, tlutHash2);
                                }

                                //Create the path and save the texture.
                                string textureHash = tex.GetDolphinTextureHash(i, tlutHash, Option.DolphinMipDetection, IsArbitraryMipmap == true, tlutHash2);
                                string path = Path.Combine(SaveDirectory, textureHash) + ".png";
                                image[i].SaveAsPng(path);

                                //We save the main level texture path for later
                                if (i == 0)
                                    mainTextureHash = textureHash;

                                //skip mips?
                                if (IsArbitraryMipmap == false && !Option.Mips) break;
                            }
                        }
                        catch (Exception t)
                        {
                            Log.WriteEX(t, subdirectory + tex.ToString());
                            Result.Unsupported++;
                        }
                        finally
                        {
                            for (int i = 0; i < tex.Count; i++)
                            {
                                image[i]?.Dispose();
                            }
                        }
                    }
                    Log.Write(FileAction.Extract, Path.Combine(subdirectory, mainTextureHash), $"mips:{tex.Count - 1} WrapS:{tex.WrapS} WrapT:{tex.WrapT} LODBias:{tex.LODBias} MinLOD:{tex.MinLOD} MaxLOD:{tex.MaxLOD} {(tex.Count > 1 ? $"ArbMipValue:{ArbitraryMipmapValue:0.000}" : string.Empty)}");
                    Option.TextureAction?.Invoke(tex, Result, subdirectory, mainTextureHash);
                }
            }
        }

        /// <summary>
        /// Checks if a texture with a given hash code has already been added to the hash pool list and adds it if not.
        /// </summary>
        /// <param name="tex">The texture to be tested.</param>
        /// <param name="TlutHash">The TLUT hash value to consider for palette formats.</param>
        /// <returns> <c>true</c> if the texture should be skipped as a duplicate; otherwise, <c>false</c>.</returns>
        private bool TestTextureHashCode(JUTTexture.TexEntry tex, ulong TlutHash)
        {
            int hashCode = tex.Hash.GetHashCode();

            //Dolphins only recognizes files with the correct mip flag
            hashCode -= tex.MaxLOD == 0 && tex.Count == 1 ? 0 : 1;

            //If it is a palleted format add TlutHash
            if (tex.Format.IsPaletteFormat() && TlutHash != 0)
                hashCode = hashCode * -1521134295 + TlutHash.GetHashCode();

            lock (Result.Hash)
            {
                //Skip duplicate textures
                if (Result.Hash.Contains(hashCode))
                    return true;
                Result.Hash.Add(hashCode);
            }
            return false;
        }

        /// <summary>
        /// Try to read a file as bti
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="subdirectory"></param>
        /// <returns></returns>
        private bool TryBTI(Stream stream, ReadOnlySpan<char> subdirectory)
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
                    Save(new BTI(stream), Path.Combine("~Force", subdirectory.ToString()));
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
                if (TryBTI(so.Stream, so.SubPath))
                    return true;
                so.Stream.Position = 0;
            }
            else
            {
                if (so.Format.Extension == "")
                {
                    if (TryBTI(so.Stream, so.SubPath))
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
            Log.Write(FileAction.Unsupported, subdirectory + Extension + $" ~{PathX.AddSizeSuffix(stream.Length, 2)}", $"Description: {FFormat.GetFullDescription()}");
            if (!Result.UnsupportedFormatType.Contains(FFormat)) Result.UnsupportedFormatType.Add(FFormat);
            Result.Unsupported++;
            Result.UnsupportedSize += stream.Length;
        }

        protected override void AddResultUnknown(Stream stream, FormatInfo FormatTypee, in string file)
        {
            base.AddResultUnknown(stream, FormatTypee, file);

            if (stream.Length > 130)
                if (!Result.UnknownFormatType.Contains(FormatTypee)) Result.UnknownFormatType.Add(FormatTypee);
            Result.Unknown++;
        }
        #endregion


    }
}
