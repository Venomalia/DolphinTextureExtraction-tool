using System;
using System.IO;

namespace AuroraLip.Common
{
    public class FormatInfo : IEquatable<FormatInfo>, IEquatable<Stream>
    {

        /// <summary>
        /// File type
        /// </summary>
        public FormatType Typ { get; set; }

        /// <summary>
        /// Default file extension
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Header information
        /// </summary>
        public HeaderInfo Header { get; set; } = null;

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The developer of this format.
        /// </summary>
        public string Developer { get; set; }

        /// <summary>
        /// The class associated with this format
        /// </summary>
        public Type Class { get; set; } = null;

        /// <summary>
        /// Checks if the data Match with this FormatInfo.
        /// </summary>
        public MatchAction IsMatch { get; set; }

        public delegate bool MatchAction(Stream stream,in string extension = "");

        #region Constructor
        public FormatInfo(string extension = "", FormatType typ = FormatType.Unknown, string description = "", string developer = "")
        {
            Typ = typ;
            Extension = extension;
            Description = description;
            Developer = developer;
            IsMatch = Matcher;
        }

        public FormatInfo(string extension, string magic, FormatType typ, string description = "", string developer = "") : this(extension, typ, description, developer)
            => Header = new HeaderInfo(magic);

        public FormatInfo(string extension, byte[] bytes, int offset, FormatType typ, string description = "", string developer = "") : this(extension, typ, description, developer)
            => Header = new HeaderInfo(bytes, offset);

        public FormatInfo(Stream stream, string extension = "") : this(extension)
        {
            Header = new HeaderInfo(stream);
            if (Header.Bytes.Length == 0)
                Header = null;
        }

        #endregion

        private bool Matcher(Stream stream,in string extension = "")
        {
            if (Header != null)
            {
                stream.Position = Header.Offset;
                return stream.Length >= Header.Bytes.Length && stream.Read(Header.Bytes.Length).ArrayEqual(Header.Bytes);
            }
            return !(extension == "" && new HeaderInfo(stream).Bytes.Length != 0) && Extension.ToLower() == extension.ToLower();
        }

        public string GetFullDescription()
        {
            if (Header != null && Header.MagicASKI.Length > 2)
                return (Developer != "" ? Developer + ' ' : "") + Header.MagicASKI + ' ' + Description;
            else
                return (Developer != "" ? Developer + ' ' : "") + Extension + ' ' + Description;
        }

        public virtual bool Equals(FormatInfo other)
        {
            if (Header != null)
            {
                return Header.Equals(other.Header);
            }
            else return Extension.ToLower() == other.Extension.ToLower();
        }

        public virtual bool Equals(Stream other) => IsMatch(other);
    }
}
