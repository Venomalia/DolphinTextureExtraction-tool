using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace AuroraLip.Texture.J3D
{

    /*
    * Super Hackio Incorporated
    * "Copyright © Super Hackio Incorporated 2020-2021"
    * https://github.com/SuperHackio/Hack.io
    */

    public static partial class JUtility
    {
        public abstract partial class JUTTexture
        {
            /// <summary>
            /// A JUTTexture Entry. Contains Mipmaps
            /// </summary>
            public class TexEntry : List<Bitmap>, IDisposable
            {
                public GXImageFormat Format { get; set; } = GXImageFormat.CMPR;
                public GXPaletteFormat PaletteFormat { get; set; }
                public GXWrapMode WrapS { get; set; }
                public GXWrapMode WrapT { get; set; }
                public GXFilterMode MagnificationFilter { get; set; }
                public GXFilterMode MinificationFilter { get; set; }
                public ulong Hash { get; internal set; } = 0;
                public ulong TlutHash { get; internal set; } = 0; //not match with dolphin!
                public float MinLOD { get; set; }
                public float MaxLOD { get; set; }
                public float LODBias { get; set; }
                public bool EnableEdgeLOD { get; set; }

                public int ImageWidth => this[0].Width;
                public int ImageHeight => this[0].Height;

                public Size LargestImageSize => new Size(this[0].Width, this[0].Height);
                public Size SmallestImageSize => new Size(this[Count - 1].Width, this[Count - 1].Height);

                public TexEntry() { }

                public TexEntry(Stream Stream, in byte[] PaletteData, GXImageFormat Format, GXPaletteFormat PaletteFormat, int? PaletteCount, int ImageWidth, int ImageHeight, int Mipmap = 0)
                {
                    this.Format = Format;
                    this.PaletteFormat = PaletteFormat;
                    this.WrapS = WrapS;
                    this.WrapT = WrapT;

                    byte[] RowDate = Stream.Read(GetCalculatedDataSize(Format, ImageWidth, ImageHeight));
                    Hash = HashDepot.XXHash.Hash64(RowDate);
                    if (IsPaletteFormat(Format)) TlutHash = HashDepot.XXHash.Hash64(PaletteData);
                    this.Add(DecodeImage(RowDate, PaletteData, Format, PaletteFormat, PaletteCount, ImageWidth, ImageHeight));

                    for (int i = 1; i < Mipmap; i++)
                    {
                        this.Add(DecodeImage(Stream, PaletteData, Format, PaletteFormat, PaletteCount, ImageWidth / 2, ImageHeight / 2, i - 1));
                    }
                }

                public TexEntry(Bitmap Image, GXImageFormat ImageFormat = GXImageFormat.CMPR, GXPaletteFormat PaletteFormat = GXPaletteFormat.IA8, GXWrapMode WrapS = 0, GXWrapMode WrapT = 0, GXFilterMode MagFilter = 0, GXFilterMode MinFilter = 0, float Bias = 0.0f, bool EdgeLoD = false)
                {
                    Add(Image);
                    Format = ImageFormat;
                    this.PaletteFormat = PaletteFormat;
                    this.WrapS = WrapS;
                    this.WrapT = WrapT;
                    MagnificationFilter = MagFilter;
                    MinificationFilter = MinFilter;
                    LODBias = Bias;
                    EnableEdgeLOD = EdgeLoD;
                }

                public string GetDolphinTextureHash(int mipmap = 0, bool UseTlut = false)
                {
                    return "tex1_" + this[0].Width + 'x' + this[0].Height + '_'
                        //Has mipmaps
                        + (this.Count != 1
                            ? "m_" : string.Empty)
                        // Hash
                        + Hash.ToString("x").PadLeft(16, '0') + '_'
                        // Tlut Hash
                        + (JUtility.IsPaletteFormat(Format)
                            ? (!UseTlut || TlutHash == 0
                                ? "$" : Hash.ToString("x").PadLeft(16, '0')) + '_' : string.Empty)
                        // Format
                        + (int)Format
                        // mipmaps
                        + (mipmap != 0
                            ? "_mip" + mipmap : string.Empty);
                }

                public bool ImageEquals(TexEntry entry) => ListEx.Equals(this, entry, J3D.JUtility.CompareBitmap);
                public override bool Equals(object obj)
                {
                    return obj is TexEntry entry &&
                           Format == entry.Format &&
                           PaletteFormat == entry.PaletteFormat &&
                           WrapS == entry.WrapS &&
                           WrapT == entry.WrapT &&
                           MagnificationFilter == entry.MagnificationFilter &&
                           MinificationFilter == entry.MinificationFilter &&
                           MinLOD == entry.MinLOD &&
                           MaxLOD == entry.MaxLOD &&
                           LODBias == entry.LODBias &&
                           EnableEdgeLOD == entry.EnableEdgeLOD &&
                           ListEx.Equals(this, entry, J3D.JUtility.CompareBitmap);
                }

                public static bool operator ==(TexEntry entry1, TexEntry entry2) => entry1.Equals(entry2);

                public static bool operator !=(TexEntry entry1, TexEntry entry2) => !(entry1 == entry2);

                public override int GetHashCode()
                {
                    var hashCode = 175115414;
                    hashCode = hashCode * -1521134295 + Format.GetHashCode();
                    hashCode = hashCode * -1521134295 + PaletteFormat.GetHashCode();
                    hashCode = hashCode * -1521134295 + WrapS.GetHashCode();
                    hashCode = hashCode * -1521134295 + WrapT.GetHashCode();
                    hashCode = hashCode * -1521134295 + MagnificationFilter.GetHashCode();
                    hashCode = hashCode * -1521134295 + MinificationFilter.GetHashCode();
                    hashCode = hashCode * -1521134295 + MinLOD.GetHashCode();
                    hashCode = hashCode * -1521134295 + MaxLOD.GetHashCode();
                    hashCode = hashCode * -1521134295 + LODBias.GetHashCode();
                    hashCode = hashCode * -1521134295 + EnableEdgeLOD.GetHashCode();
                    hashCode = hashCode * -1521134295 + ListEx.GetHashCode(this);
                    return hashCode;
                }

                public void CopyTo(TexEntry destination)
                {
                    destination.Format = Format;
                    destination.PaletteFormat = PaletteFormat;
                    destination.WrapS = WrapS;
                    destination.WrapT = WrapT;
                    destination.MagnificationFilter = MagnificationFilter;
                    destination.MinificationFilter = MinificationFilter;
                    destination.MinLOD = MinLOD;
                    destination.MaxLOD = MaxLOD;
                    destination.EnableEdgeLOD = EnableEdgeLOD;
                    for (int i = 0; i < Count; i++)
                        destination.Add((Bitmap)this[i].Clone());
                }

                #region Dispose
                private bool disposedValue;
                protected virtual void Dispose(bool disposing)
                {
                    if (!disposedValue)
                    {
                        if (disposing)
                        {
                            foreach (var item in this)
                            {
                                item.Dispose();
                            }
                        }
                        disposedValue = true;
                    }
                }
                ~TexEntry()
                {
                    Dispose(disposing: false);
                }
                public void Dispose()
                {
                    Dispose(disposing: true);
                    GC.SuppressFinalize(this);
                }
                #endregion
            }
        }
    }
}
