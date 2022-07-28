using System.Collections.Generic;
using System.IO;

namespace AuroraLip.Common
{
    public static partial class FormatDictionary
    {
        public static bool TryGetValue(string key, out FormatInfo info)
        {
            if (Header.TryGetValue(key, out info))
                return true;

            if (Extensions.TryGetValue(key.ToLower(), out info))
                return true;

            return false;
        }

        public static FormatInfo GetValue(string key)
        {
            if (TryGetValue(key, out FormatInfo info)) return info;
            throw new KeyNotFoundException(key);
        }

        public static FormatInfo Identify(Stream stream, string extension = "")
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
                    Header.Add(file.Header.Magic, file);
                }
                else
                {
                    Extensions.Add(file.Extension.ToLower(), file);
                }
            }
        }

        private static readonly Dictionary<string, FormatInfo> Extensions = new Dictionary<string, FormatInfo>();

        private static readonly Dictionary<string, FormatInfo> Header = new Dictionary<string, FormatInfo>();

    }
}
