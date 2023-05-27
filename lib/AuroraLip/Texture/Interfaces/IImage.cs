namespace AuroraLib.Texture.Interfaces
{
    /// <summary>
    /// Interface representing an image properties with width and height.
    /// </summary>
    public interface IImage
    {
        /// <summary>
        /// The width of the image.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The height of the image.
        /// </summary>
        int Height { get; }
    }
}
