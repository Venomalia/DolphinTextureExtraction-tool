using Hack.io;
using System;
using System.IO;

namespace CLZ
{
    public class CLZ0
    {
        private const string Magic = "CLZ";

        /// <summary>
        /// Decompress a File
        /// </summary>
        /// <param name="Filename">Full path to the file</param>
        public static void Decompress(string Filename)
        {
            FileStream fileStream = new FileStream(Filename, FileMode.Open, FileAccess.Read);
            using (MemoryStream outstram = Decomp(fileStream))
            {
                fileStream.Close();
                File.WriteAllBytes(Filename, outstram.ToArray());
            } 
        }
        /// <summary>
        /// Decompress a MemoryStream
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static MemoryStream Decompress(Stream Data) => Decomp(Data);
        /// <summary>
        /// Decompress a byte[]
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] Data)
        {
            using (MemoryStream stream = Decomp(new MemoryStream(Data)))
            {
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Compress a File
        /// </summary>
        /// <param name="Filename">File to compress</param>
        /// <param name="Quick">If true, takes shorter time to compress, but is overall weaker then if disabled (resulting in larger files)</param>
        public static void Compress(string Filename, bool Quick = false) => File.WriteAllBytes(Filename, Quick ? QuickCompress(File.ReadAllBytes(Filename)) : DoCompression(File.ReadAllBytes(Filename)));
        /// <summary>
        /// Compress a MemoryStream
        /// </summary>
        /// <param name="Data">MemoryStream to compress</param>
        /// <param name="Quick">The Algorithm to use. True to use CLZ0 Fast</param>
        public static MemoryStream Compress(MemoryStream Data, bool Quick = false) => new MemoryStream(Quick ? QuickCompress(Data.ToArray()) : DoCompression(Data.ToArray()));
        /// <summary>
        /// Compress a byte[]
        /// </summary>
        /// <param name="Data">The data to compress</param>
        /// <param name="Quick">The Algorithm to use. True to use YAZ0 Fast</param>
        /// <returns></returns>
        public static byte[] Compress(byte[] Data, bool Quick = false) => Quick ? QuickCompress(Data) : DoCompression(Data);
        /// <summary>
        /// Checks a given file for CLZ0 Encoding
        /// </summary>
        /// <param name="Filename">File to check</param>
        /// <returns>true if the file is CLZ0 Encoded</returns>
        public static bool Check(string Filename)
        {
            FileStream YAZ0 = new FileStream(Filename, FileMode.Open);
            bool Check = YAZ0.ReadString(3) == Magic;
            YAZ0.Close();
            return Check;
        }
        /// <summary>
        /// Converts a Yaz0 Encoded file to a Yaz0 Decoded MemoryStream
        /// </summary>
        /// <param name="Filename">The file to decode into a MemoryStream</param>
        /// <returns>The decoded MemoryStream</returns>
        public static MemoryStream DecompressToMemoryStream(string Filename)
        {
            using (FileStream fileStream = new FileStream(Filename, FileMode.Open))
            {
                return Decomp(fileStream);
            }
        }

        private static MemoryStream Decomp(Stream infile)
        {
            MemoryStream OutStream = new MemoryStream();
            CLZ.Unpack(infile, OutStream);
            OutStream.Position = 0;
            return OutStream;
        }

        private static byte[] DoCompression(byte[] data)
        {
            throw new NotImplementedException();
        }

        private static byte[] QuickCompress(byte[] data)
        {
            throw new NotImplementedException();
        }



    }
}
