using AuroraLip.Common;
using AuroraLip.Compression;
using AuroraLip.Compression.Formats;
using Hack.io;
using Hack.io.BMD;
using Hack.io.BTI;
using Hack.io.J3D;
using Hack.io.RARC;
using Hack.io.TPL;
using Hack.io.U8;
using Hack.io.Util;
using LibCPK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Hack.io.J3D.JUtility;

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

        }

        public class Result
        {
            public TimeSpan TotalTime { get; internal set; }

            public double ExtractionRate => Math.Round((double)100 / (ExtractedSize + SkippedSize) * ExtractedSize);

            public int Extracted => Hash.Count();

            public int Unknown { get; internal set; } = 0;

            public int Unsupported { get; internal set; } = 0;

            public string LogFullPath { get; internal set; }

            internal long ExtractedSize = 0;

            internal long SkippedSize = 0;

            /// <summary>
            /// List of hash values of the extracted textures
            /// </summary>
            public List<ulong> Hash = new List<ulong>();

            public List<FileTypInfo> UnsupportedFileTyp = new List<FileTypInfo>();

            public List<FileTypInfo> UnknownFileTyp = new List<FileTypInfo>();
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

        public static Result StartScan(string meindirectory, string savedirectory, Options options = null)
        {
            TextureExtractor Extractor = new TextureExtractor(meindirectory, savedirectory, options);
            return Extractor.StartScan();
        }

        public Result StartScan()
        {
            DateTime starttime = DateTime.Now;

            Scan(new DirectoryInfo(ScanDirectory));

            result.TotalTime = DateTime.Now.Subtract(starttime);
            Log.WriteFoot(result);
            Log.Dispose();

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

            foreach (FileInfo file in directory.GetFiles())
            {
                Scan(file);
            }
        }

        private void Scan(FileInfo file)
        {
            Stream stream = new FileStream(file.FullName, FileMode.Open);
            FileTypInfo filetype = GetFiletype(stream, file.Extension);

            string subdirectory = GetDirectoryWithoutExtension(file.FullName.Replace(ScanDirectory + Path.DirectorySeparatorChar, ""));

            try
            {
                switch (filetype.Typ)
                {
                    case FileTyp.Unknown:
                        if (options.Force)
                        {
                            if (TryBTI(stream, subdirectory)) break;
                        }

                        Log.Write(FileAction.Unknown, subdirectory + file.Extension + $" ~{Math.Round((double)file.Length / 1048576, 2)}mb",
                            filetype.Header.Magic.Length > 3 ?
                            $"Magic:[{filetype.Header.Magic}] Bytes:[{string.Join(",", filetype.Header.Bytes)}] Offset:{filetype.Header.Offset}" :
                            $"Bytes32:[{string.Join(",", new Header(stream, 32).Bytes)}]");
                        result.Unknown++;
                        //Exclude files that are too small, for calculation purposes only half the size
                        if (file.Length > 130) result.SkippedSize += file.Length / 2;
                        if (filetype.Header.MagicASKI.Length < 3 || filetype.Header.MagicASKI.Length > 8) filetype = new FileTypInfo(filetype.Extension,FileTyp.Unknown);
                        if (!result.UnknownFileTyp.Contains(filetype)) result.UnknownFileTyp.Add(filetype);
                        break;
                    case FileTyp.Texture:
                        if (options.Raw)
                        {
                            Save(stream, Path.ChangeExtension(GetFullSaveDirectory(Path.Combine("Raw", subdirectory)), filetype.Extension));
                        }
                        goto case FileTyp.Archive;
                    case FileTyp.Archive:
                        if (filetype.Header == null)
                        {
                            switch (filetype.Extension.ToLower())
                            {
                                case "bti":
                                    Save(new BTI(stream), subdirectory);
                                    result.ExtractedSize += stream.Length;
                                    break;
                                case "lz":

                                    byte[] bytes = stream.ToArray();
                                    if (Compression.TryToDecompress(bytes, out byte[] test, out ICompression algorithm))
                                    {
                                        Scan(new MemoryStream(test), subdirectory);
                                        break;
                                    }

                                    if (Compression.TryToFindMatch(in bytes, out algorithm))
                                    {
                                        Log.Write(FileAction.Unsupported, subdirectory + file.Extension + $" ~{Math.Round((double)file.Length / 1048576, 2)}mb", $"Description: {filetype.GetFullDescription()} Algorithm:{algorithm.GetType().Name}?");
                                        if (!result.UnsupportedFileTyp.Contains(filetype)) result.UnsupportedFileTyp.Add(filetype);
                                        result.Unsupported++;
                                        result.SkippedSize += file.Length;
                                    }
                                    else
                                    {
                                        goto default;
                                    }
                                    break;
                                default:
                                    Log.Write(FileAction.Unsupported, subdirectory + file.Extension + $" ~{Math.Round((double)file.Length / 1048576, 2)}mb", $"Description: {filetype.GetFullDescription()}");
                                    if (!result.UnsupportedFileTyp.Contains(filetype)) result.UnsupportedFileTyp.Add(filetype);
                                    result.Unsupported++;
                                    result.SkippedSize += file.Length;
                                    break;
                            }
                        }
                        else
                        {
                            switch (filetype.Header.MagicASKI)
                            {
                                case "Yaz0":
                                    Scan(Compression<YAZ0>.Decompress(stream), subdirectory);
                                    break;
                                case "Yay0":
                                case "YAY0":
                                    Scan(Compression<YAY0>.Decompress(stream), subdirectory);
                                    break;
                                case "CLZ":
                                    Scan(Compression<CLZ>.Decompress(stream), GetDirectoryWithoutExtension(subdirectory));
                                    break;
                                case " 0": //TPL
                                    Save(new TPL(stream), subdirectory);
                                    result.ExtractedSize += stream.Length;
                                    break;
                                case "J3D2bdl4":
                                    BDL bdlmodel = new BDL(stream);
                                    foreach (var item in bdlmodel.Textures.Textures)
                                    {
                                        Save(item, subdirectory);
                                    }
                                    result.ExtractedSize += stream.Length;
                                    break;
                                case "J3D2bmd3":
                                    BMD bmdmodel = new BMD(stream);
                                    foreach (var item in bmdmodel.Textures.Textures)
                                    {
                                        Save(item, subdirectory);
                                    }
                                    result.ExtractedSize += stream.Length;
                                    break;
                                case "CPK ":
                                    stream.Close();
                                    scanCPK(file.FullName, subdirectory);
                                    break;
                                case "U8-":
                                    Scan(new U8(stream, file.FullName), subdirectory);
                                    break;
                                case RARC.Magic:
                                    Scan(new RARC(stream, file.FullName), subdirectory);
                                    break;
                                default:
                                    Log.Write(FileAction.Unsupported, subdirectory + file.Extension + $" ~{Math.Round((double)file.Length / 1048576, 2)}mb", $"Description: {filetype.GetFullDescription()}");
                                    if (!result.UnsupportedFileTyp.Contains(filetype)) result.UnsupportedFileTyp.Add(filetype);
                                    result.Unsupported++;
                                    result.SkippedSize += file.Length;
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception t)
            {
                Log.WriteEX(t, subdirectory + file.Extension);
                result.Unsupported++;
                result.SkippedSize += file.Length;
            }
            stream.Close();
        }

        private void Scan(MemoryStream stream, string subdirectory, string Extension = "")
        {
            FileTypInfo filetype = GetFiletype(stream, Extension);

            try
            {
                switch (filetype.Typ)
                {
                    case FileTyp.Unknown:
                        if (options.Force)
                        {
                            if (TryBTI(stream, subdirectory)) break;
                        }

                        Log.Write(FileAction.Unknown, subdirectory + Extension + $" ~{Math.Round((double)stream.Length / 1048576, 2)}mb",
                            filetype.Header.Magic.Length > 3 ?
                            $"Magic:[{filetype.Header.Magic}] Bytes:[{string.Join(",", filetype.Header.Bytes)}] Offset:{filetype.Header.Offset}" :
                            $"Bytes32:[{string.Join(",", new Header(stream, 32).Bytes)}]");

                        if (filetype.Header.MagicASKI.Length < 3 || filetype.Header.MagicASKI.Length > 8) filetype = new FileTypInfo(filetype.Extension, FileTyp.Unknown);
                        if (!result.UnknownFileTyp.Contains(filetype)) result.UnknownFileTyp.Add(filetype);
                        result.Unknown++;
                        break;
                    case FileTyp.Texture:
                        if (options.Raw)
                        {
                            Save(stream, Path.ChangeExtension(GetFullSaveDirectory(Path.Combine("Raw", subdirectory)), filetype.Extension));
                        }
                        goto case FileTyp.Archive;
                    case FileTyp.Archive:
                        if (filetype.Header == null)
                        {
                            switch (filetype.Extension.ToLower())
                            {
                                case "bti":
                                    Save(new BTI(stream), subdirectory);
                                    result.ExtractedSize += stream.Length;
                                    break;
                                default:
                                    Log.Write(FileAction.Unsupported, subdirectory + Extension + $" ~{Math.Round((double)stream.Length / 1048576, 2)}mb", $"Description: {filetype.GetFullDescription()}");
                                    if (!result.UnsupportedFileTyp.Contains(filetype)) result.UnsupportedFileTyp.Add(filetype);
                                    result.Unsupported++;
                                    result.SkippedSize += stream.Length;
                                    break;
                            }
                        }
                        else
                        {
                            switch (filetype.Header.MagicASKI)
                            {
                                case "Yaz0":
                                    Scan(Compression<YAZ0>.Decompress(stream), subdirectory);
                                    break;
                                case "Yay0":
                                case "YAY0":
                                    Scan(Compression<YAY0>.Decompress(stream), subdirectory);
                                    break;
                                case "CLZ":
                                    Scan(Compression<CLZ>.Decompress(stream), GetDirectoryWithoutExtension(subdirectory));
                                    break;
                                case " 0": //TPL
                                    Save(new TPL(stream), subdirectory);
                                    result.ExtractedSize += stream.Length;
                                    break;
                                case "J3D2bdl4":
                                    BDL bdlmodel = new BDL(stream);
                                    foreach (var item in bdlmodel.Textures.Textures)
                                    {
                                        Save(item, subdirectory);
                                    }
                                    result.ExtractedSize += stream.Length;
                                    break;
                                case "J3D2bmd3":
                                    BMD bmdmodel = new BMD(stream);
                                    foreach (var item in bmdmodel.Textures.Textures)
                                    {
                                        Save(item, subdirectory);
                                    }
                                    result.ExtractedSize += stream.Length;
                                    break;
                                case "U8-":
                                    Scan(new U8(stream), subdirectory);
                                    break;
                                case RARC.Magic:
                                    Scan(new RARC(stream), subdirectory);
                                    break;
                                default:
                                    Log.Write(FileAction.Unsupported, subdirectory + Extension + $" ~{Math.Round((double)stream.Length / 1048576, 2)}mb", $"Description: {filetype.GetFullDescription()}");
                                    if (!result.UnsupportedFileTyp.Contains(filetype)) result.UnsupportedFileTyp.Add(filetype);
                                    result.Unsupported++;
                                    result.SkippedSize += stream.Length;
                                    break;
                            }
                        }
                        break;
                }

            }
            catch (Exception t)
            {
                Log.WriteEX(t, subdirectory + Extension);
                result.Unsupported++;
                result.SkippedSize += stream.Length;
            }
            stream.Close();
        }

        #endregion

        #region Archive

        private void Scan(Archive archiv, string subdirectory)
        {
            foreach (var item in archiv.Root.Items)
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

        private void Scan(ArchiveDirectory archivdirectory, string subdirectory)
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

        private void Scan(ArchiveFile file, string subdirectory)
        {
            Scan((MemoryStream)file, Path.Combine(subdirectory, Path.GetFileNameWithoutExtension(file.Name)), file.Extension.ToLower());
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
                    if (entries.FileType == "FILE")
                    {
                        if (Dictionary.Extension.TryGetValue(Path.GetExtension(entries.FileName.ToString()).ToLower(), out FileTypInfo filetype))
                        {
                            switch (filetype.Typ)
                            {
                                case FileTyp.Unknown:
                                    break;
                                case FileTyp.Archive:
                                case FileTyp.Texture:
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
                    }
                }
            }
            finally
            {
                CPKReader.Close();
            }
        }

        private bool CpkDecompressEntrie(CPK CpkContent, BinaryReader CPKReader, FileEntry entrie, out byte[] chunk)
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
        private void Save(JUTTexture texture, string subdirectory)
        {
            foreach (JUTTexture.TexEntry tex in texture)
            {
                ulong Hash = HashDepot.XXHash.Hash64(tex.RowDate);
                //Skip duplicate textures
                if (result.Hash.Contains(Hash))
                {
                    continue;
                }
                result.Hash.Add(Hash);

                Log.Write(FileAction.Extract, subdirectory, $"Hash:{Hash.ToString("x").PadLeft(16, '0')} Size:{tex[0].Width}x{tex[0].Height} Format:{tex.Format} mips:{tex.Count != 1}{(tex.Count != 1 ? $" {tex.Count}" : "")}");

                //Extract the main texture and mips
                for (int i = 0; i < tex.Count; i++)
                {
                    string filename = $"tex1_{tex[0].Width}x{tex[0].Height}_";
                    if (tex.Count > 1) filename += "m_";
                    filename += $"{Hash.ToString("x").PadLeft(16, '0')}";
                    //Skip Palette Data hash
                    if (JUtility.IsPaletteFormat(tex.Format)) filename += "_$";
                    filename += $"_{((int)tex.Format)}";
                    if (i != 0) filename += $"_mip{i}";
                    filename += ".png";

                    string path = GetFullSaveDirectory(subdirectory);
                    Directory.CreateDirectory(path);
                    tex[i].Save(Path.Combine(path, filename), System.Drawing.Imaging.ImageFormat.Png);

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
        /// Try to read a file as bit
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="subdirectory"></param>
        /// <returns></returns>
        private bool TryBTI(Stream stream, string subdirectory)
        {
            if (Enum.IsDefined(typeof(GXImageFormat), (byte)stream.ReadByte()) && Enum.IsDefined(typeof(JUTTransparency), (byte)stream.ReadByte()))
            {
                ushort ImageWidth = BitConverter.ToUInt16(stream.ReadReverse(0, 2), 0);
                ushort ImageHeight = BitConverter.ToUInt16(stream.ReadReverse(0, 2), 0);

                if (ImageWidth > 4 && ImageHeight > 4 && ImageWidth < 1024 && ImageHeight < 1024)
                {
                    stream.Position -= 6;
                    try
                    {
                        Save(new BTI(stream), subdirectory);
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
        private string GetFullSaveDirectory(string directory)
        {
            return Path.Combine(SaveDirectory, directory);
        }

        private static string GetDirectoryWithoutExtension(string directory)
        {
            return Path.Combine(Path.GetDirectoryName(directory), Path.GetFileNameWithoutExtension(directory));
        }

        private static FileTypInfo GetFiletype(Stream stream, string Extension = "")
        {
            Header header = new Header(stream);

            FileTypInfo filetype = Dictionary.Master.FirstOrDefault(x => x.Header?.Equals(header) == true);
            if (filetype != null)
            {
                return filetype;
            }

            if (Dictionary.Header.TryGetValue(header.Magic, out filetype) || Dictionary.Header.TryGetValue(header.MagicASKI, out filetype) || header.MagicASKI.Length > 4 && Dictionary.Header.TryGetValue(header.MagicASKI.Substring(0, 4), out filetype))
            {
                return filetype;
            }

            if (Dictionary.Extension.TryGetValue(Extension.ToLower(), out filetype))
            {
                if (filetype.Header == null)
                {
                    return filetype;
                }
            }

            return new FileTypInfo(Extension, header, FileTyp.Unknown);
        }
        #endregion


    }
}
