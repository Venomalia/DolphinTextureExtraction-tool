using AuroraLib.Common.Struct;

namespace AuroraLib.Common
{
    public static partial class FormatDictionary
    {
        public static bool TryGetValue(string key, out FormatInfo info)
        {
            try
            {
                info = Master.First(x => x.Extension == key);
                return true;
            }
            catch (Exception) { }

            info = null;
            return false;
        }

        /// <summary>
        /// Tries to retrieve the <see cref="FormatInfo"/> associated with the specified <see cref="IIdentifier"/>.
        /// </summary>
        /// <param name="key">The identifier key.</param>
        /// <param name="info">When this method returns true, contains the <see cref="FormatInfo"/> associated with the specified <see cref="IIdentifier"/>; otherwise, null.</param>
        /// <returns>true if the lookup table contains an entry with the specified <paramref name="key"/>; otherwise, false.</returns>
        public static bool TryGetValue(IIdentifier key, out FormatInfo info)
            => IdentifierLOT.TryGetValue(key.GetHashCode(), out info);

        /// <summary>
        /// Tries to retrieve the <see cref="FormatInfo"/> associated with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The identifier key.</param>
        /// <param name="info">When this method returns <c>true</c>, contains the <see cref="FormatInfo"/> associated with the specified <paramref name="key"/>; otherwise, <c>null</c>.</param>
        /// <returns>true if the lookup table contains an entry with the specified <paramref name="key"/>; otherwise, false.</returns>
        public static bool TryGetValue(ReadOnlySpan<byte> key, out FormatInfo info)
            => IdentifierLOT.TryGetValue((int)HashDepot.XXHash.Hash32(key), out info);


        /// <summary>
        /// Identifies the file format
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static FormatInfo Identify(this Stream stream, in string extension = "")
        {
            //Test for reliable Identifier32 & Identifier64
            if (stream.Length >= 0x10)
            {
                stream.Seek(0, SeekOrigin.Begin);
                Identifier64 identifier = stream.ReadUInt64();

                lock (IdentifierLOT)
                {
                    if (IdentifierLOT.TryGetValue(identifier.GetHashCode(), out FormatInfo format) || IdentifierLOT.TryGetValue(identifier.Lower.GetHashCode(), out format))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        try
                        {
                            if (format.IsMatch.Invoke(stream, extension))
                            {
                                return format;
                            }
                        }
                        catch (Exception t)
                        {
                            Events.NotificationEvent?.Invoke(NotificationType.Warning, $"Match error in {format.Class?.Name}, {t}");
                        }
                        finally
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                        }
                    }
                }
            }

            //Test remaining identifier
            stream.Seek(0, SeekOrigin.Begin);
            foreach (var item in Formats)
            {
                try
                {
                    if (item.IsMatch.Invoke(stream, extension))
                    {
                        return item;
                    }
                }
                catch (Exception t)
                {
#if DEBUG
                    Events.NotificationEvent?.Invoke(NotificationType.Warning, $"Match error in {item.Class?.Name}, {t}");
#endif
                }
                finally
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }
            }

            //Create new identifier
            FormatInfo formatInfo = new(extension, FormatType.Unknown);
            if (stream.Length >= 0x20)
            {
                Span<byte> temp = stackalloc byte[0x10];
                stream.Read(temp);
                stream.Seek(0, SeekOrigin.Begin);

                int AsciiSize = EncodingEX.ValidSize(temp, b => b < 32 || b >= 127);
                if (AsciiSize == 3 || AsciiSize == 4)
                {
                    formatInfo.Identifier = new Identifier32(temp[..4]);
                    Add(formatInfo);
                }
                else if (AsciiSize == 7 || AsciiSize == 8)
                {
                    formatInfo.Identifier = new Identifier64(temp[..8]);
                    Add(formatInfo);
                }

            }
            return formatInfo;
        }

        public static void Add(FormatInfo formatInfo)
        {
            if (formatInfo.Identifier != null)
            {
                int key = formatInfo.Identifier.GetHashCode();
                if (!IdentifierLOT.ContainsKey(key))
                {
                    lock (IdentifierLOT)
                    {
                        IdentifierLOT.Add(key, formatInfo);
                    }
                }
                else
                {
                    Events.NotificationEvent?.Invoke(NotificationType.Warning, $"Identifier {formatInfo.Identifier} is already used!");
                }

                if (formatInfo.IdentifierOffset != 0 || formatInfo.Identifier is Identifier)
                {
                    Formats.Add(formatInfo);
                }

            }
            else
            {
                Formats.Add(formatInfo);
            }
        }

        static FormatDictionary()
        {
            foreach (FormatInfo formatInfo in Master)
            {
                Add(formatInfo);
            }
            Formats.Sort((x, y) => (y.Class != null).CompareTo(x.Class != null));
        }

        private static readonly List<FormatInfo> Formats = new();

        /// <summary>
        /// use Identifier GetHashCode
        /// </summary>
        private static readonly Dictionary<int, FormatInfo> IdentifierLOT = new();
    }
}
