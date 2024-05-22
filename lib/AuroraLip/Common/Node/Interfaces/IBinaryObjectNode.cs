using AuroraLib.Core.Interfaces;

namespace AuroraLib.Common.Node.Interfaces
{
    /// <summary>
    /// Represents a node object that can be serialized and deserialized.
    /// </summary>
    public interface IBinaryObjectNode : IBinaryObject, IDisposable, IObjectName, IDataTime, IFileAccess
    {
        /// <summary>
        /// Deserializes the binary data for this node from the specified <see cref="FileNode"/>.
        /// </summary>
        /// <param name="source">The <see cref="FileNode"/> containing the binary data to deserialize.</param>
        void BinaryDeserialize(FileNode source);

        /// <summary>
        /// Serializes the binary data of this node as a <see cref="FileNode"/>.
        /// </summary>
        /// <returns>A <see cref="FileNode"/> containing the serialized binary data.</returns>
        FileNode BinarySerialize();
    }

}
