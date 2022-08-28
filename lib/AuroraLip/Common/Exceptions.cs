using System;
using System.Runtime.Serialization;

namespace AuroraLip.Common
{
    public class InvalidIdentifierException : Exception
    {
        public string ExpectedIdentifier { get; set; }

        public InvalidIdentifierException() { }

        public InvalidIdentifierException(string ExpectedIdentifier) : base($"Expected \"{ExpectedIdentifier}\"")
        {
            this.ExpectedIdentifier = ExpectedIdentifier;
        }
    }

    public class PaletteException : Exception
    {
        public string ExpectedIdentifier { get; set; }

        public PaletteException() { }

        public PaletteException(string message) : base(message) { }

        public PaletteException(string message, Exception innerException) : base(message, innerException) { }

        protected PaletteException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
