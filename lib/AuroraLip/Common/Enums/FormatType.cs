namespace AuroraLib.Common
{
    /// <summary>
    /// The format type specifies the intended use of a file format.
    /// </summary>
    public enum FormatType
    {
        Unknown = default,
        Archive,
        Texture,
        Audio,
        Model,
        Collision,
        Video,
        Text,
        Font,
        Layout,
        Animation,
        Skript,
        Parameter,
        Executable,
        Effect,
        Shader,
        Rom,
        Iso,
        Else,
        Dummy
    }
}
