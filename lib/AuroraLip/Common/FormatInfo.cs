using AuroraLib.Core.Text;
using System.Runtime.CompilerServices;
using System.Text;

namespace AuroraLib.Common
{
    /// <summary>
    /// Represents information about a file format.
    /// </summary>
    public class FormatInfo : IEquatable<FormatInfo>, IEquatable<Stream>
    {

        /// <summary>
        /// Gets or sets the format type.
        /// </summary>
        public FormatType Typ { get; set; }

        /// <summary>
        /// Gets or sets the file format extension.
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the file format.
        /// </summary>
        public IIdentifier Identifier { get; set; }

        /// <summary>
        /// Gets or sets the offset of the identifier within the file.
        /// </summary>
        public int IdentifierOffset { get; set; }

        /// <summary>
        /// Gets or sets the description of the file format.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the developer of the file format.
        /// </summary>
        public string Developer { get; set; }

        /// <summary>
        /// Gets or sets the class type responsible for handling the file format.
        /// </summary>
        public Type Class
        {
            get => @class;
            set
            {
                @class = value;

                if (value != null)
                {
                    IFileAccess access = GetInstance();
                    IsMatch = access.IsMatch;
                    CanRead = access.CanRead;
                    CanWrite = access.CanWrite;
                    if (IdentifierOffset == 0 && Identifier == null && access is IHasIdentifier magic)
                    {
                        Identifier = magic.Identifier;
                    }
                }
                else
                {
                    IsMatch = Matcher;
                    CanRead = false;
                    CanWrite = false;
                }
            }
        }
        private Type @class;

        /// <summary>
        /// Gets whether the format can be read from the <see cref="Class"/> 
        /// </summary>
        public bool CanRead { get; private set; }

        /// <summary>
        /// Gets whether the format can be write from the <see cref="Class"/> 
        /// </summary>
        public bool CanWrite { get; private set; }

        /// <summary>
        /// Gets or sets the match action delegate that determines whether a given stream and extension match the format.
        /// </summary>
        public MatchAction IsMatch { get; set; }

        /// <summary>
        /// Represents a delegate that determines whether a given <paramref name="stream"/> and <paramref name="extension"/> match a specific format.
        /// </summary>
        /// <param name="stream">The stream to check.</param>
        /// <param name="extension">The extension to check.</param>
        /// <returns> true if the stream and extension match the format; otherwise, false.</returns>
        public delegate bool MatchAction(Stream stream, in string extension = "");

        /// <summary>
        /// Creates an instance of the class associated with this format.
        /// </summary>
        /// <returns>An instance of the file access class.</returns>
        public IFileAccess GetInstance() => (IFileAccess)Activator.CreateInstance(Class);

        public FormatInfo(string extension, IIdentifier identifier, int offset, FormatType typ, string description = "", string developer = "", Type type = null)
        {
            Typ = typ;
            Extension = extension;
            Description = description;
            Developer = developer;
            Identifier = identifier;
            IdentifierOffset = offset;
            Class = type;
        }

        public FormatInfo(string extension, string identifier, FormatType typ, string description = "", string developer = "", Type type = null) : this(extension, ToIdentifier(identifier), 0, typ, description, developer, type)
        { }

        public FormatInfo(string extension, int offset, byte[] identifier, FormatType typ, string description = "", string developer = "", Type type = null) : this(extension, ToIdentifier(identifier), offset, typ, description, developer, type)
        { }

        public FormatInfo(string extension, IIdentifier identifier, FormatType typ, string description = "", string developer = "", Type type = null) : this(extension, identifier, 0, typ, description, developer, type)
        { }

        public FormatInfo(string extension, FormatType typ, string description = "", string developer = "", Type type = null) : this(extension, null, 0, typ, description, developer, type)
        { }

        #region Helper
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IIdentifier ToIdentifier(in string identifier) => identifier.Length switch
        {
            4 => new Identifier32(identifier),
            8 => new Identifier64(identifier),
            _ => new Identifier(identifier),
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IIdentifier ToIdentifier(in byte[] identifier) => identifier.Length switch
        {
            4 => new Identifier32(identifier),
            8 => new Identifier64(identifier),
            _ => new Identifier(identifier),
        };

        private bool Matcher(Stream stream, in string extension = "")
        {
            if (Identifier != null)
            {
                ReadOnlySpan<byte> identifier = Identifier.AsSpan();
                if (stream.Length >= IdentifierOffset + identifier.Length)
                {
                    stream.Seek(IdentifierOffset, SeekOrigin.Begin);
                    return stream.Match(Identifier);
                }
                return false;
            }
            return extension != string.Empty && Extension.ToLower() == extension.ToLower();
        }
        #endregion

        /// <summary>
        /// Generates the full description of the format, including the developer, name, and description.
        /// </summary>
        /// <returns>A StringBuilder containing the full description of the format.</returns>
        public StringBuilder GetFullDescription()
        {
            StringBuilder sb = new();
            if (Developer != null)
            {
                sb.Append(Developer);
                sb.Append(' ');
            }

            sb.Append(GetName());
            sb.Append(' ');
            sb.Append(Description);

            return sb;
        }

        /// <summary>
        /// Retrieves the name of the format.
        /// </summary>
        /// <returns>The name of the format.</returns>
        public string GetName()
        {
            if (Identifier != null && EncodingX.ValidSize(Identifier.AsSpan()) >= 3)
            {
                return EncodingX.GetValidString(Identifier.AsSpan());
            }

            return Extension;
        }

        public string GetTypName()
        {
            if (Identifier != null && Identifier.AsSpan().Length <= 8)
            {
                return Extension == string.Empty ? Identifier.ToString() : Extension + ' ' + Identifier.ToString();
            }
            return Extension;
        }

        /// <inheritdoc />
        public virtual bool Equals(FormatInfo other)
        {
            if (Identifier != null)
            {
                return Identifier.Equals(other.Identifier);
            }
            else
            {
                return Extension.ToLower() == other.Extension.ToLower();
            }
        }

        /// <inheritdoc />
        public virtual bool Equals(Stream other) => IsMatch(other);
    }
}
