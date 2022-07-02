using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AuroraLip.Compression
{
    public static class Compression
    {
        /// <summary>
        /// A list of all available compression algorithms Types
        /// </summary>
        public static IEnumerable<Type> AvailableTypes { get; } = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes().Where(s => typeof(ICompression).IsAssignableFrom(s) && !s.IsInterface && !s.IsAbstract));

        /// <summary>
        /// A list of all names of available decompress algorithms
        /// </summary>
        /// <returns>List of available algorithms</returns>
        public static IEnumerable<Type> GetAvailablDecompress() => AvailableTypes.Where(x => ((ICompression)Activator.CreateInstance(x)).CanDecompress);

        /// <summary>
        /// A list of all names of available compress algorithms
        /// </summary>
        /// <returns>List of available algorithms</returns>
        public static IEnumerable<Type> GetAvailablCompress() => AvailableTypes.Where(x => ((ICompression)Activator.CreateInstance(x)).CanCompress);

        /// <summary>
        /// Trying to find an algorithm that can decompress the data
        /// </summary>
        /// <param name="data">data to be decrypted</param>
        /// <param name="algorithm">Matching algorithm</param>
        /// <returns>found a match</returns>
        public static bool TryToFindMatch(in byte[] data, out ICompression algorithm)
        {
            foreach (Type item in AvailableTypes)
            {
                algorithm = (ICompression)Activator.CreateInstance(item);
                if (algorithm.IsMatch(in data)) return true;
            }
            algorithm = null;
            return false;
        }

        /// <summary>
        /// Trying to decompress the data
        /// </summary>
        /// <param name="indata"><data to be decrypted/param>
        /// <param name="outdata">Decompressed data</param>
        /// <param name="algorithm">Matching algorithm</param>
        /// <returns></returns>
        public static bool TryToDecompress(in byte[] indata, out byte[] outdata, out ICompression algorithm)
        {
            foreach (Type item in AvailableTypes)
            {
                algorithm = (ICompression)Activator.CreateInstance(item);
                if (algorithm.CanDecompress && algorithm.IsMatch(in indata))
                {
                    try
                    {
                        outdata = algorithm.Decompress(indata);
                        return true;
                    }
                    catch (Exception) { }
                }
            }

            outdata = null;
            algorithm = null;
            return false;
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
