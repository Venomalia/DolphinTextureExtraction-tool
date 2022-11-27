using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AuroraLip.Common
{
    public static partial class FormatDictionary
    {
        public static bool TryGetValue(string key, out FormatInfo info)
        {
            if (Header.TryGetValue(key, out info))
                return true;

            try
            {
                info = Master.First(x => x.Extension == key);
                return true;
            }
            catch (System.Exception) { }

            return false;
        }

        public static FormatInfo GetValue(string key)
        {
            if (TryGetValue(key, out FormatInfo info)) return info;
            throw new KeyNotFoundException(key);
        }

        /// <summary>
        /// Identifies the file format
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static FormatInfo Identify(this Stream stream, string extension = "")
        {
            foreach (var item in Master)
            {
                if (item.IsMatch.Invoke(stream, extension))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    return item;
                }
                stream.Seek(0, SeekOrigin.Begin);
            }

            return new FormatInfo(stream, extension);
        }

        static FormatDictionary()
        {
            foreach (FormatInfo file in Master)
            {
                if (file.Header != null)
                {
                    try
                    {
                        file.Class = Reflection.FileAccess.GetMagic(file.Header.Magic);
                        file.IsMatch = Reflection.FileAccess.GetInstance(file.Class).IsMatch;
                    }
                    catch (System.Exception) { }
                    if (file.Header.Magic.Length > 1)
                        Header.Add(file.Header.Magic, file);

                }
            }
        }

        public static readonly Dictionary<string, FormatInfo> Header = new Dictionary<string, FormatInfo>();

    }
}
