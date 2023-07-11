using System.Runtime.Serialization;

namespace AuroraLib.Common
{
    public class PaletteException : Exception
    {
        public string ExpectedIdentifier { get; set; }

        public PaletteException()
        { }

        public PaletteException(string message) : base(message)
        {
        }

        public PaletteException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PaletteException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
