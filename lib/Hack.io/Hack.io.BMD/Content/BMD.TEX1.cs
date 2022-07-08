using AuroraLip.Common;
using AuroraLip.Texture.Formats;
using AuroraLip.Texture.J3D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

//Heavily based on the SuperBMD Library.
namespace Hack.io.BMD
{
    public partial class BMD
    {
        public class TEX1
        {
            public List<BTI> Textures { get; private set; } = new List<BTI>();

            private static readonly string Magic = "TEX1";

            /// <summary>
            /// Get texture by Index
            /// </summary>
            /// <param name="Index">Texture ID</param>
            /// <returns></returns>
            public BTI this[int Index]
            {
                get
                {
                    if (Textures != null && Textures.Count > Index)
                        return Textures[Index];
                    throw new ArgumentException("TEX1[] (GET)", "Index");
                }
                set
                {
                    if (Textures == null)
                        Textures = new List<BTI>();

                    if (!(value is BTI || value is null) || Index > Textures.Count || Index < 0 || (value is null && Index == Textures.Count))
                        throw new ArgumentException("TEX1[] (SET)", "Index");
                    
                    if (value is null)
                        Textures.RemoveAt(Index);
                    else if (Index == Textures.Count)
                        Textures.Add(value);
                    else
                        Textures[Index] = value;
                }
            }
            /// <summary>
            /// Get Texture by FileName
            /// </summary>
            /// <param name="TextureName">Texture FileName</param>
            /// <returns></returns>
            public BTI this[string TextureName]
            {
                get
                {
                    if (Textures == null)
                    {
                        Console.WriteLine("There are no textures currently loaded.");
                        return null;
                    }

                    if (Textures.Count == 0)
                    {
                        Console.WriteLine("There are no textures currently loaded.");
                        return null;
                    }

                    foreach (BTI tex in Textures)
                    {
                        if (tex.FileName.Equals(TextureName))
                            return tex;
                    }

                    Console.Write($"No texture with the name { TextureName } was found.");
                    return null;
                }

                set
                {
                    if (Textures == null)
                        Textures = new List<BTI>();

                    if (!(value is BTI || value is null))
                        return;

                    for (int i = 0; i < Textures.Count; i++)
                    {
                        if (Textures[i].FileName.Equals(TextureName))
                        {
                            if (value is null)
                                Textures.RemoveAt(i);
                            else
                                Textures[i] = value;
                            return;
                        }
                    }
                    if (!(value is null))
                        Textures.Add(value);
                }
            }
            /// <summary>
            /// Gets the total amount of textures in this section
            /// </summary>
            public int Count => Textures.Count;

            public TEX1(Stream BMDFile)
            {
                int ChunkStart = (int)BMDFile.Position;
                if (!BMDFile.ReadString(4).Equals(Magic))
                    throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");

                int tex1Size = BitConverter.ToInt32(BMDFile.ReadBigEndian(4), 0);
                short texCount = BitConverter.ToInt16(BMDFile.ReadBigEndian(2), 0);
                BMDFile.Position += 0x02;

                int textureHeaderOffset = BitConverter.ToInt32(BMDFile.ReadBigEndian(4), 0);
                int textureNameTableOffset = BitConverter.ToInt32(BMDFile.ReadBigEndian(4), 0);

                List<string> names = new List<string>();

                BMDFile.Seek(ChunkStart + textureNameTableOffset, System.IO.SeekOrigin.Begin);

                short stringCount = BitConverter.ToInt16(BMDFile.ReadBigEndian(2), 0);
                BMDFile.Position += 0x02;

                for (int i = 0; i < stringCount; i++)
                {
                    BMDFile.Position += 0x02;
                    short nameOffset = BitConverter.ToInt16(BMDFile.ReadBigEndian(2), 0);
                    long saveReaderPos = BMDFile.Position;
                    BMDFile.Position = ChunkStart + textureNameTableOffset + nameOffset;

                    names.Add(BMDFile.ReadString());

                    BMDFile.Position = saveReaderPos;
                }


                BMDFile.Seek(textureHeaderOffset + ChunkStart, SeekOrigin.Begin);

                for (int i = 0; i < texCount; i++)
                {
                    BMDFile.Seek((ChunkStart + 0x20 + (0x20 * i)), SeekOrigin.Begin);

                    BTI img = new BTI(BMDFile) { FileName = names[i] };
                    Textures.Add(img);
                }
            }

            public void Write(Stream writer)
            {
                long start = writer.Position;

                writer.WriteString(Magic);
                writer.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for section size
                writer.WriteBigEndian(BitConverter.GetBytes((short)Textures.Count), 0, 2);
                writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                writer.Write(new byte[4] { 0x00, 0x00, 0x00, 0x20 }, 0, 4); // Offset to the start of the texture data. Always 32
                writer.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for string table offset

                writer.AddPadding(32, Padding);

                List<string> names = new List<string>();
                Dictionary<BTI, long> WrittenImages = new Dictionary<BTI, long>();

                long ImageDataOffset = start + (writer.Position - start) + (0x20 * Textures.Count);
                for (int i = 0; i < Textures.Count; i++)
                {
                    long x = -1;
                    foreach (KeyValuePair<BTI, long> item in WrittenImages)
                    {
                        if (item.Key.ImageEquals(Textures[i]))
                        {
                            x = item.Value;
                            break;
                        }
                    }
                    if (x == -1)
                    {
                        WrittenImages.Add(Textures[i], ImageDataOffset);
                        Textures[i].Save(writer, ref ImageDataOffset);
                    }
                    else
                        Textures[i].Save(writer, ref x);
                    names.Add(Textures[i].FileName);
                }

                writer.Position = writer.Length;
                // Write texture name table offset
                int NameTableOffset = (int)(writer.Position - start);

                writer.WriteStringTable(names);
                writer.AddPadding( 32, Padding);
                // Write TEX1 size
                writer.Position = start + 4;
                writer.WriteBigEndian(BitConverter.GetBytes((int)(writer.Length - start)), 0, 4);
                writer.Position = start + 0x10;
                writer.WriteBigEndian(BitConverter.GetBytes(NameTableOffset), 0, 4);
            }

            public bool Contains(BTI Image) => Textures.Any(I => I.Equals(Image));
            public bool ContainsImage(BTI Image) => Textures.Any(I => I.ImageEquals(Image));
            public int GetTextureIndex(BTI Image)
            {
                if (Image is null)
                    throw new ArgumentException("BMD.TEX1.GetTextureIndex()", "Image");
                for (int i = 0; i < Count; i++)
                    if (Image == Textures[i])
                        return i;
                return -1;
            }

            public static List<KeyValuePair<int, BTI>?> FetchUsedTextures(TEX1 Textures, MAT3.Material Material)
            {
                List<KeyValuePair<int, BTI>?> UsedTextures = new List<KeyValuePair<int, BTI>?>();
                for (int i = 0; i < 8; i++)
                {
                    if (Material.TextureIndices[i] != -1 && Material.TextureIndices[i] < Textures.Count)
                        UsedTextures.Add(new KeyValuePair<int, BTI>(Material.TextureIndices[i], Textures[Material.TextureIndices[i]]));
                    else if (Material.TextureIndices[i] != -1)
                        UsedTextures.Add(null);
                }
                return UsedTextures;
            }

            public void UpdateTextures(MAT3 Materials)
            {
                for (int i = 0; i < Materials.Count; i++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        if (Materials[i].Textures[x] is null)
                            Materials[i].TextureIndices[x] = -1;
                        else if (Contains(Materials[i].Textures[x]))
                            Materials[i].TextureIndices[x] = GetTextureIndex(Materials[i].Textures[x]);
                        else
                        {
                            Materials[i].TextureIndices[x] = Textures.Count;
                            this[Textures.Count] = Materials[i].Textures[x];
                        }
                    }
                }
            }
        }

        //=====================================================================
    }
}
