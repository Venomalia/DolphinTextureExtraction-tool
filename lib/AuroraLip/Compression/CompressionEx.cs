using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AuroraLip.Compression
{
    public static class CompressionEx
    {
        /// <summary>
        /// Checks if the data compressed with this compression method
        /// </summary>
        /// <param name="Data"></param>
        /// <returns>"True" if it corresponds to the compression method.</returns>
        public static bool IsMatch(this ICompression algorithm, in byte[] Data)
        {
            return algorithm.IsMatch(new MemoryStream(Data));
        }

        #region Decompress
        /// <summary>
        /// Decompress a File
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="filename">Full path to the file</param>
        public static void Decompress(this ICompression algorithm, in string filename) => Decompress(algorithm, filename, filename);

        /// <summary>
        /// Decompress a file and save it to a new file
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="InFilename">Full path to the file</param>
        /// <param name="OutFilename">Full path to the new file</param>
        public static void Decompress(this ICompression algorithm, in string InFilename, in string OutFilename) => File.WriteAllBytes(OutFilename, algorithm.Decompress(File.ReadAllBytes(InFilename)));

        /// <summary>
        /// Decompress a byte[]
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="Data">Compressed data</param>
        /// <returns>Decompressed data</returns>
        public static byte[] Decompress(this ICompression algorithm, in byte[] Data) => algorithm.Decompress(in Data);

        /// <summary>
        /// Decompress a MemoryStream
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="memorystream">Compressed MemoryStream</param>
        /// <returns>Decompressed MemoryStream</returns>
        public static MemoryStream Decompress(this ICompression algorithm, MemoryStream memorystream) => new MemoryStream(algorithm.Decompress(memorystream.ToArray()));

        /// <summary>
        /// Decompress a MemoryStream
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="stream">Compressed MemoryStream</param>
        /// <returns>Decompressed MemoryStream</returns>
        public static MemoryStream Decompress(this ICompression algorithm, Stream stream) => new MemoryStream(algorithm.Decompress(stream.ToArray()));

        #endregion

        #region Compress

        /// <summary>
        /// Compress a File
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="filename">Full path to the file</param>
        public static void Compress(this ICompression algorithm, in string Filename) => Compress(algorithm, Filename, Filename);

        /// <summary>
        /// Compress a file and save it to a new file
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="InFilename">Full path to the file</param>
        /// <param name="OutFilename">Full path to the new file</param>
        public static void Compress(this ICompression algorithm, in string InFilename, in string OutFilename) => File.WriteAllBytes(OutFilename, algorithm.Compress(File.ReadAllBytes(InFilename)));

        /// <summary>
        /// Compress a byte[]
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="Data">Decompressed data</param>
        /// <returns>Compressed data</returns>
        public static byte[] Compress(this ICompression algorithm, in byte[] Data) => algorithm.Compress(in Data);

        /// <summary>
        /// Compress a MemoryStream
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="memorystream">Decompressed MemoryStream</param>
        /// <returns>Compressed MemoryStream</returns>
        public static MemoryStream Compress(this ICompression algorithm, MemoryStream memorystream) => new MemoryStream(algorithm.Compress(memorystream.ToArray()));
        #endregion

    }
}
