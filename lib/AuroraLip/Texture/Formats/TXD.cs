using AuroraLib.Common;
using AuroraLib.Core.Interfaces;
using RenderWareNET.Enums;
using RenderWareNET.Plugins;
using RenderWareNET.Structs;

namespace AuroraLib.Texture.Formats
{
    public class TXD : JUTTexture, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new(22);

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
        {
            RWPluginHeader mainHeader = stream.Read<RWPluginHeader>();
            RWPluginHeader structHeader = stream.Read<RWPluginHeader>();
            return mainHeader.Identifier == PluginID.TextureDictionary && mainHeader.Version == structHeader.Version && structHeader.Identifier == PluginID.Struct;
        }

        protected override void Read(Stream stream)
        {
            TextureDictionary textureDictionary = new(stream);
            foreach (TextureNative tx in textureDictionary)
            {
                if (tx.Properties.Platform != TexturePlatformID.GC)
                {
                    throw new NotSupportedException();
                }
                using MemoryStream ms = new(tx.Properties.ImageData);
                GXImageFormat format = (GXImageFormat)tx.Properties.Format;
                GXPaletteFormat palettFormat = tx.Properties.TLOTFormat switch
                {
                    FourCCType.IA8 => GXPaletteFormat.IA8,
                    FourCCType.RGB565 => GXPaletteFormat.RGB565,
                    FourCCType.RGB5A3 => GXPaletteFormat.RGB5A3,
                    _ => GXPaletteFormat.IA8,
                };
                TexEntry entry = new(ms, tx.Properties.TLOT, format, palettFormat, format.GetMaxPaletteColours(), tx.Properties.Width, tx.Properties.Height, tx.Properties.Images - 1);
                Add(entry);
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();
    }
}
