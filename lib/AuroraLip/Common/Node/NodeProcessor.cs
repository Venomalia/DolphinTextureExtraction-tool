using AuroraLib.Compression.Interfaces;

namespace AuroraLib.Common.Node
{
    public static class NodeProcessor
    {
        public static void Expand(this FileNode file, ICompressionDecoder decoder)
        {
            MemoryPoolStream dataDecompress = new();
            try
            {
                decoder.Decompress(file.Data, dataDecompress);
                file.Data.Dispose();
                file.Data = dataDecompress;
            }
            catch (Exception)
            {
                dataDecompress.Dispose();
                throw;
            }
        }
    }
}
