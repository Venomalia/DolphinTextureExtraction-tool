using AuroraLib.Core.Exceptions;
using OpenTK.Mathematics;

//Heavily based on the SuperBMD Library.
namespace Hack.io
{
    public partial class BMD
    {
        public class VTX1
        {
            public VertexData Attributes { get; set; } = new VertexData();
            public SortedDictionary<GXVertexAttribute, Tuple<GXDataType, byte>> StorageFormats { get; set; } = new SortedDictionary<GXVertexAttribute, Tuple<GXDataType, byte>>();

            private static readonly string Magic = "VTX1";

            public VTX1(Stream stream, int VertexCount)
            {
                long ChunkStart = stream.Position;
                if (!stream.ReadString(4).Equals(Magic))
                    throw new InvalidIdentifierException(Magic);

                int ChunkSize = stream.ReadInt32(Endian.Big);
                stream.Position += 0x04;
                int[] attribDataOffsets = new int[13]
                {
                    stream.ReadInt32(Endian.Big),
                    stream.ReadInt32(Endian.Big),
                    stream.ReadInt32(Endian.Big),
                    stream.ReadInt32(Endian.Big),
                    stream.ReadInt32(Endian.Big),
                    stream.ReadInt32(Endian.Big),
                    stream.ReadInt32(Endian.Big),
                    stream.ReadInt32(Endian.Big),
                    stream.ReadInt32(Endian.Big),
                    stream.ReadInt32(Endian.Big),
                    stream.ReadInt32(Endian.Big),
                    stream.ReadInt32(Endian.Big),
                    stream.ReadInt32(Endian.Big)
                };
                GXVertexAttribute attrib = (GXVertexAttribute)stream.ReadInt32(Endian.Big);

                while (attrib != GXVertexAttribute.Null)
                {
                    GXComponentCount componentCount = (GXComponentCount)stream.ReadInt32(Endian.Big);
                    GXDataType componentType = (GXDataType)stream.ReadInt32(Endian.Big);
                    byte fractionalBitCount = (byte)stream.ReadByte();
                    StorageFormats.Add(attrib, new Tuple<GXDataType, byte>(componentType, fractionalBitCount));

                    stream.Position += 0x03;
                    long curPos = stream.Position;

                    int attribOffset = GetAttributeDataOffset(attribDataOffsets, ChunkSize, attrib, VertexCount, out int attribDataSize);
                    int attribCount = GetAttributeDataCount(attribDataSize, attrib, componentType, componentCount);
                    Attributes.SetAttributeData(attrib, LoadAttributeData(stream, (int)(ChunkStart + attribOffset), attribCount, fractionalBitCount, attrib, componentType, componentCount));

                    stream.Position = curPos;
                    attrib = (GXVertexAttribute)stream.ReadInt32(Endian.Big);
                }
                stream.Position = ChunkStart + ChunkSize;
            }

            public object LoadAttributeData(Stream stream, int offset, int count, byte frac, GXVertexAttribute attribute, GXDataType dataType, GXComponentCount compCount)
            {
                stream.Seek(offset, SeekOrigin.Begin);
                object final = null;

                switch (attribute)
                {
                    case GXVertexAttribute.Position:
                        switch (compCount)
                        {
                            case GXComponentCount.Position_XY:
                                final = LoadVec2Data(stream, frac, count, dataType);
                                break;
                            case GXComponentCount.Position_XYZ:
                                final = LoadVec3Data(stream, frac, count, dataType);
                                break;
                        }
                        break;
                    case GXVertexAttribute.Normal:
                        switch (compCount)
                        {
                            case GXComponentCount.Normal_XYZ:
                                final = LoadVec3Data(stream, frac, count, dataType);
                                break;
                            case GXComponentCount.Normal_NBT:
                                break;
                            case GXComponentCount.Normal_NBT3:
                                break;
                        }
                        break;
                    case GXVertexAttribute.Color0:
                    case GXVertexAttribute.Color1:
                        final = LoadColorData(stream, count, dataType);
                        break;
                    case GXVertexAttribute.Tex0:
                    case GXVertexAttribute.Tex1:
                    case GXVertexAttribute.Tex2:
                    case GXVertexAttribute.Tex3:
                    case GXVertexAttribute.Tex4:
                    case GXVertexAttribute.Tex5:
                    case GXVertexAttribute.Tex6:
                    case GXVertexAttribute.Tex7:
                        switch (compCount)
                        {
                            case GXComponentCount.TexCoord_S:
                                final = LoadSingleFloat(stream, frac, count, dataType);
                                break;
                            case GXComponentCount.TexCoord_ST:
                                final = LoadVec2Data(stream, frac, count, dataType);
                                break;
                        }
                        break;
                }

                return final;
            }

            private List<float> LoadSingleFloat(Stream stream, byte frac, int count, GXDataType dataType)
            {
                List<float> floatList = new List<float>();

                for (int i = 0; i < count; i++)
                {
                    switch (dataType)
                    {
                        case GXDataType.Unsigned8:
                            byte compu81 = (byte)stream.ReadByte();
                            float compu81Float = (float)compu81 / (float)(1 << frac);
                            floatList.Add(compu81Float);
                            break;
                        case GXDataType.Signed8:
                            sbyte comps81 = (sbyte)stream.ReadByte();
                            float comps81Float = (float)comps81 / (float)(1 << frac);
                            floatList.Add(comps81Float);
                            break;
                        case GXDataType.Unsigned16:
                            ushort compu161 = stream.ReadUInt16(Endian.Big);
                            float compu161Float = (float)compu161 / (float)(1 << frac);
                            floatList.Add(compu161Float);
                            break;
                        case GXDataType.Signed16:
                            short comps161 = stream.ReadInt16(Endian.Big);
                            float comps161Float = (float)comps161 / (float)(1 << frac);
                            floatList.Add(comps161Float);
                            break;
                        case GXDataType.Float32:
                            floatList.Add(stream.ReadSingle(Endian.Big));
                            break;
                    }
                }

                return floatList;
            }

            private List<Vector2> LoadVec2Data(Stream stream, byte frac, int count, GXDataType dataType)
            {
                List<Vector2> vec2List = new List<Vector2>();

                for (int i = 0; i < count; i++)
                {
                    switch (dataType)
                    {
                        case GXDataType.Unsigned8:
                            byte compu81 = (byte)stream.ReadByte();
                            byte compu82 = (byte)stream.ReadByte();
                            float compu81Float = (float)compu81 / (float)(1 << frac);
                            float compu82Float = (float)compu82 / (float)(1 << frac);
                            vec2List.Add(new Vector2(compu81Float, compu82Float));
                            break;
                        case GXDataType.Signed8:
                            sbyte comps81 = (sbyte)stream.ReadByte();
                            sbyte comps82 = (sbyte)stream.ReadByte();
                            float comps81Float = (float)comps81 / (float)(1 << frac);
                            float comps82Float = (float)comps82 / (float)(1 << frac);
                            vec2List.Add(new Vector2(comps81Float, comps82Float));
                            break;
                        case GXDataType.Unsigned16:
                            ushort compu161 = stream.ReadUInt16(Endian.Big);
                            ushort compu162 = stream.ReadUInt16(Endian.Big);
                            float compu161Float = (float)compu161 / (float)(1 << frac);
                            float compu162Float = (float)compu162 / (float)(1 << frac);
                            vec2List.Add(new Vector2(compu161Float, compu162Float));
                            break;
                        case GXDataType.Signed16:
                            short comps161 = stream.ReadInt16(Endian.Big);
                            short comps162 = stream.ReadInt16(Endian.Big);
                            float comps161Float = (float)comps161 / (float)(1 << frac);
                            float comps162Float = (float)comps162 / (float)(1 << frac);
                            vec2List.Add(new Vector2(comps161Float, comps162Float));
                            break;
                        case GXDataType.Float32:
                            vec2List.Add(new Vector2(stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big)));
                            break;
                    }
                }

                return vec2List;
            }

            private List<Vector3> LoadVec3Data(Stream stream, byte frac, int count, GXDataType dataType)
            {
                List<Vector3> vec3List = new List<Vector3>();

                for (int i = 0; i < count; i++)
                {
                    switch (dataType)
                    {
                        case GXDataType.Unsigned8:
                            byte compu81 = (byte)stream.ReadByte();
                            byte compu82 = (byte)stream.ReadByte();
                            byte compu83 = (byte)stream.ReadByte();
                            float compu81Float = (float)compu81 / (float)(1 << frac);
                            float compu82Float = (float)compu82 / (float)(1 << frac);
                            float compu83Float = (float)compu83 / (float)(1 << frac);
                            vec3List.Add(new Vector3(compu81Float, compu82Float, compu83Float));
                            break;
                        case GXDataType.Signed8:
                            sbyte comps81 = (sbyte)stream.ReadByte();
                            sbyte comps82 = (sbyte)stream.ReadByte();
                            sbyte comps83 = (sbyte)stream.ReadByte();
                            float comps81Float = (float)comps81 / (float)(1 << frac);
                            float comps82Float = (float)comps82 / (float)(1 << frac);
                            float comps83Float = (float)comps83 / (float)(1 << frac);
                            vec3List.Add(new Vector3(comps81Float, comps82Float, comps83Float));
                            break;
                        case GXDataType.Unsigned16:
                            ushort compu161 = stream.ReadUInt16(Endian.Big);
                            ushort compu162 = stream.ReadUInt16(Endian.Big);
                            ushort compu163 = stream.ReadUInt16(Endian.Big);
                            float compu161Float = (float)compu161 / (float)(1 << frac);
                            float compu162Float = (float)compu162 / (float)(1 << frac);
                            float compu163Float = (float)compu163 / (float)(1 << frac);
                            vec3List.Add(new Vector3(compu161Float, compu162Float, compu163Float));
                            break;
                        case GXDataType.Signed16:
                            short comps161 = stream.ReadInt16(Endian.Big);
                            short comps162 = stream.ReadInt16(Endian.Big);
                            short comps163 = stream.ReadInt16(Endian.Big);
                            float comps161Float = (float)comps161 / (float)(1 << frac);
                            float comps162Float = (float)comps162 / (float)(1 << frac);
                            float comps163Float = (float)comps163 / (float)(1 << frac);
                            vec3List.Add(new Vector3(comps161Float, comps162Float, comps163Float));
                            break;
                        case GXDataType.Float32:
                            vec3List.Add(new Vector3(stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big)));
                            break;
                    }
                }

                return vec3List;
            }

            private List<Color4> LoadColorData(Stream stream, int count, GXDataType dataType)
            {
                List<Color4> colorList = new List<Color4>();

                for (int i = 0; i < count; i++)
                {
                    switch (dataType)
                    {
                        case GXDataType.RGB565:
                            short colorShort = stream.ReadInt16(Endian.Big);
                            int r5 = (colorShort & 0xF800) >> 11;
                            int g6 = (colorShort & 0x07E0) >> 5;
                            int b5 = (colorShort & 0x001F);
                            colorList.Add(new Color4((float)r5 / 255.0f, (float)g6 / 255.0f, (float)b5 / 255.0f, 1.0f));
                            break;
                        case GXDataType.RGB8:
                            byte r8 = (byte)stream.ReadByte();
                            byte g8 = (byte)stream.ReadByte();
                            byte b8 = (byte)stream.ReadByte();
                            stream.Position++;
                            colorList.Add(new Color4((float)r8 / 255.0f, (float)g8 / 255.0f, (float)b8 / 255.0f, 1.0f));
                            break;
                        case GXDataType.RGBX8:
                            byte rx8 = (byte)stream.ReadByte();
                            byte gx8 = (byte)stream.ReadByte();
                            byte bx8 = (byte)stream.ReadByte();
                            stream.Position++;
                            colorList.Add(new Color4((float)rx8 / 255.0f, (float)gx8 / 255.0f, (float)bx8 / 255.0f, 1.0f));
                            break;
                        case GXDataType.RGBA4:
                            short colorShortA = stream.ReadInt16(Endian.Big);
                            int r4 = (colorShortA & 0xF000) >> 12;
                            int g4 = (colorShortA & 0x0F00) >> 8;
                            int b4 = (colorShortA & 0x00F0) >> 4;
                            int a4 = (colorShortA & 0x000F);
                            colorList.Add(new Color4((float)r4 / 255.0f, (float)g4 / 255.0f, (float)b4 / 255.0f, (float)a4 / 255.0f));
                            break;
                        case GXDataType.RGBA6:
                            int colorInt = stream.ReadInt32(Endian.Big);
                            int r6 = (colorInt & 0xFC0000) >> 18;
                            int ga6 = (colorInt & 0x03F000) >> 12;
                            int b6 = (colorInt & 0x000FC0) >> 6;
                            int a6 = (colorInt & 0x00003F);
                            colorList.Add(new Color4((float)r6 / 255.0f, (float)ga6 / 255.0f, (float)b6 / 255.0f, (float)a6 / 255.0f));
                            break;
                        case GXDataType.RGBA8:
                            byte ra8 = (byte)stream.ReadByte();
                            byte ga8 = (byte)stream.ReadByte();
                            byte ba8 = (byte)stream.ReadByte();
                            byte aa8 = (byte)stream.ReadByte();
                            colorList.Add(new Color4((float)ra8 / 255.0f, (float)ga8 / 255.0f, (float)ba8 / 255.0f, (float)aa8 / 255.0f));
                            break;
                    }
                }
                return colorList;
            }

            private int GetAttributeDataOffset(int[] offsets, int vtx1Size, GXVertexAttribute attribute, int VertexCount, out int size)
            {
                int offset = 0;
                size = 0;
                Vtx1OffsetIndex start = Vtx1OffsetIndex.PositionData;

                switch (attribute)
                {
                    case GXVertexAttribute.Position:
                        start = Vtx1OffsetIndex.PositionData;
                        offset = offsets[(int)Vtx1OffsetIndex.PositionData];
                        break;
                    case GXVertexAttribute.Normal:
                        start = Vtx1OffsetIndex.NormalData;
                        offset = offsets[(int)Vtx1OffsetIndex.NormalData];
                        break;
                    case GXVertexAttribute.Color0:
                        start = Vtx1OffsetIndex.Color0Data;
                        offset = offsets[(int)Vtx1OffsetIndex.Color0Data];
                        break;
                    case GXVertexAttribute.Color1:
                        start = Vtx1OffsetIndex.Color1Data;
                        offset = offsets[(int)Vtx1OffsetIndex.Color1Data];
                        break;
                    case GXVertexAttribute.Tex0:
                        start = Vtx1OffsetIndex.TexCoord0Data;
                        offset = offsets[(int)Vtx1OffsetIndex.TexCoord0Data];
                        break;
                    case GXVertexAttribute.Tex1:
                        start = Vtx1OffsetIndex.TexCoord1Data;
                        offset = offsets[(int)Vtx1OffsetIndex.TexCoord1Data];
                        break;
                    case GXVertexAttribute.Tex2:
                        start = Vtx1OffsetIndex.TexCoord2Data;
                        offset = offsets[(int)Vtx1OffsetIndex.TexCoord2Data];
                        break;
                    case GXVertexAttribute.Tex3:
                        start = Vtx1OffsetIndex.TexCoord3Data;
                        offset = offsets[(int)Vtx1OffsetIndex.TexCoord3Data];
                        break;
                    case GXVertexAttribute.Tex4:
                        start = Vtx1OffsetIndex.TexCoord4Data;
                        offset = offsets[(int)Vtx1OffsetIndex.TexCoord4Data];
                        break;
                    case GXVertexAttribute.Tex5:
                        start = Vtx1OffsetIndex.TexCoord5Data;
                        offset = offsets[(int)Vtx1OffsetIndex.TexCoord5Data];
                        break;
                    case GXVertexAttribute.Tex6:
                        start = Vtx1OffsetIndex.TexCoord6Data;
                        offset = offsets[(int)Vtx1OffsetIndex.TexCoord6Data];
                        break;
                    case GXVertexAttribute.Tex7:
                        start = Vtx1OffsetIndex.TexCoord7Data;
                        offset = offsets[(int)Vtx1OffsetIndex.TexCoord7Data];
                        break;
                    default:
                        throw new ArgumentException("attribute");
                }

                for (int i = (int)start + 1; i < 13; i++)
                {
                    if (i == 12)
                    {
                        size = vtx1Size - offset;
                        break;
                    }

                    int nextOffset = offsets[i];

                    if (nextOffset == 0)
                        continue;
                    else
                    {
                        size = nextOffset - offset;
                        break;
                    }
                }

                return offset;
            }

            private int GetAttributeDataCount(int size, GXVertexAttribute attribute, GXDataType dataType, GXComponentCount compCount)
            {
                int compCnt = 0;
                int compStride = 0;

                if (attribute == GXVertexAttribute.Color0 || attribute == GXVertexAttribute.Color1)
                {
                    switch (dataType)
                    {
                        case GXDataType.RGB565:
                        case GXDataType.RGBA4:
                            compCnt = 1;
                            compStride = 2;
                            break;
                        case GXDataType.RGB8:
                        case GXDataType.RGBX8:
                        case GXDataType.RGBA6:
                        case GXDataType.RGBA8:
                            compCnt = 4;
                            compStride = 1;
                            break;
                    }
                }
                else
                {
                    switch (dataType)
                    {
                        case GXDataType.Unsigned8:
                        case GXDataType.Signed8:
                            compStride = 1;
                            break;
                        case GXDataType.Unsigned16:
                        case GXDataType.Signed16:
                            compStride = 2;
                            break;
                        case GXDataType.Float32:
                            compStride = 4;
                            break;
                    }

                    switch (attribute)
                    {
                        case GXVertexAttribute.Position:
                            if (compCount == GXComponentCount.Position_XY)
                                compCnt = 2;
                            else if (compCount == GXComponentCount.Position_XYZ)
                                compCnt = 3;
                            break;
                        case GXVertexAttribute.Normal:
                            if (compCount == GXComponentCount.Normal_XYZ)
                                compCnt = 3;
                            break;
                        case GXVertexAttribute.Tex0:
                        case GXVertexAttribute.Tex1:
                        case GXVertexAttribute.Tex2:
                        case GXVertexAttribute.Tex3:
                        case GXVertexAttribute.Tex4:
                        case GXVertexAttribute.Tex5:
                        case GXVertexAttribute.Tex6:
                        case GXVertexAttribute.Tex7:
                            if (compCount == GXComponentCount.TexCoord_S)
                                compCnt = 1;
                            else if (compCount == GXComponentCount.TexCoord_ST)
                                compCnt = 2;
                            break;
                    }
                }

                return size / (compCnt * compStride);
            }

            public void Write(Stream stream)
            {
                long start = stream.Position;

                stream.WriteString("VTX1");
                stream.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for section size
                stream.Write(0x40, Endian.Big); // Offset to attribute data

                for (int i = 0; i < 13; i++) // Placeholders for attribute data offsets
                    stream.Write(new byte[4] { 0x00, 0x00, 0x00, 0x00 }, 0, 4); //I can't use the typical 0xDD here because this actually is setting the values to be "empty", it's not a placeholder

                WriteAttributeHeaders(stream);

                stream.WriteAlign(32, Padding);

                WriteAttributeData(stream, (int)start);

                long end = stream.Position;
                long length = (end - start);

                stream.Position = (int)start + 4;
                stream.Write((int)length, Endian.Big);
                stream.Position = end;
            }

            private void WriteAttributeHeaders(Stream stream)
            {
                foreach (GXVertexAttribute attrib in Enum.GetValues(typeof(GXVertexAttribute)))
                {
                    if (!Attributes.ContainsAttribute(attrib) || attrib == GXVertexAttribute.PositionMatrixIdx)
                        continue;

                    stream.Write((int)attrib, Endian.Big);

                    switch (attrib)
                    {
                        case GXVertexAttribute.PositionMatrixIdx:
                            break;
                        case GXVertexAttribute.Position:
                            stream.Write(new byte[4] { 0x00, 0x00, 0x00, 0x01 }, 0, 4);
                            stream.Write((int)StorageFormats[attrib].Item1, Endian.Big);
                            stream.WriteByte(StorageFormats[attrib].Item2);
                            stream.Write(new byte[3] { 0xFF, 0xFF, 0xFF }, 0, 3);
                            break;
                        case GXVertexAttribute.Normal:
                            stream.Write(new byte[4] { 0x00, 0x00, 0x00, 0x00 }, 0, 4);
                            stream.Write((int)StorageFormats[attrib].Item1, Endian.Big);
                            stream.WriteByte(StorageFormats[attrib].Item2);
                            stream.Write(new byte[3] { 0xFF, 0xFF, 0xFF }, 0, 3);
                            break;
                        case GXVertexAttribute.Color0:
                        case GXVertexAttribute.Color1:
                            stream.Write(new byte[4] { 0x00, 0x00, 0x00, 0x01 }, 0, 4);
                            stream.Write((int)StorageFormats[attrib].Item1, Endian.Big);
                            stream.WriteByte(StorageFormats[attrib].Item2);
                            stream.Write(new byte[3] { 0xFF, 0xFF, 0xFF }, 0, 3);
                            break;
                        case GXVertexAttribute.Tex0:
                        case GXVertexAttribute.Tex1:
                        case GXVertexAttribute.Tex2:
                        case GXVertexAttribute.Tex3:
                        case GXVertexAttribute.Tex4:
                        case GXVertexAttribute.Tex5:
                        case GXVertexAttribute.Tex6:
                        case GXVertexAttribute.Tex7:
                            stream.Write(new byte[4] { 0x00, 0x00, 0x00, 0x01 }, 0, 4);
                            stream.Write((int)StorageFormats[attrib].Item1, Endian.Big);
                            stream.WriteByte(StorageFormats[attrib].Item2);
                            stream.Write(new byte[3] { 0xFF, 0xFF, 0xFF }, 0, 3);
                            break;
                    }
                }

                stream.Write(new byte[4] { 0x00, 0x00, 0x00, 0xFF }, 0, 4);
                stream.Write(new byte[4] { 0x00, 0x00, 0x00, 0x01 }, 0, 4);
                stream.Write(new byte[4] { 0x00, 0x00, 0x00, 0x00 }, 0, 4);
                stream.Write(new byte[4] { 0x00, 0xFF, 0xFF, 0xFF }, 0, 4);
            }

            private void WriteAttributeData(Stream stream, int baseOffset)
            {
                foreach (GXVertexAttribute attrib in Enum.GetValues(typeof(GXVertexAttribute)))
                {
                    if (!Attributes.ContainsAttribute(attrib) || attrib == GXVertexAttribute.PositionMatrixIdx)
                        continue;

                    long endOffset = stream.Position;

                    switch (attrib)
                    {
                        case GXVertexAttribute.Position:
                            stream.Position = baseOffset + 0x0C;
                            stream.Write((int)(stream.Length - baseOffset), Endian.Big);
                            stream.Position = (int)endOffset;

                            foreach (Vector3 posVec in (List<Vector3>)Attributes.GetAttributeData(attrib))
                            {
                                switch (StorageFormats[attrib].Item1)
                                {
                                    case GXDataType.Unsigned8:
                                        stream.WriteByte((byte)Math.Round(posVec.X * (1 << StorageFormats[attrib].Item2)));
                                        stream.WriteByte((byte)Math.Round(posVec.Y * (1 << StorageFormats[attrib].Item2)));
                                        stream.WriteByte((byte)Math.Round(posVec.Z * (1 << StorageFormats[attrib].Item2)));
                                        break;
                                    case GXDataType.Signed8:
                                        stream.WriteByte((byte)((sbyte)Math.Round(posVec.X * (1 << StorageFormats[attrib].Item2))));
                                        stream.WriteByte((byte)((sbyte)Math.Round(posVec.Y * (1 << StorageFormats[attrib].Item2))));
                                        stream.WriteByte((byte)((sbyte)Math.Round(posVec.Z * (1 << StorageFormats[attrib].Item2))));
                                        break;
                                    case GXDataType.Unsigned16:
                                        stream.Write((ushort)Math.Round(posVec.X * (1 << StorageFormats[attrib].Item2)), Endian.Big);
                                        stream.Write((ushort)Math.Round(posVec.Y * (1 << StorageFormats[attrib].Item2)), Endian.Big);
                                        stream.Write((ushort)Math.Round(posVec.Z * (1 << StorageFormats[attrib].Item2)), Endian.Big);
                                        break;
                                    case GXDataType.Signed16:
                                        stream.Write((short)Math.Round(posVec.X * (1 << StorageFormats[attrib].Item2)), Endian.Big);
                                        stream.Write((short)Math.Round(posVec.Y * (1 << StorageFormats[attrib].Item2)), Endian.Big);
                                        stream.Write((short)Math.Round(posVec.Z * (1 << StorageFormats[attrib].Item2)), Endian.Big);
                                        break;
                                    case GXDataType.Float32:
                                        stream.Write(posVec.X, Endian.Big);
                                        stream.Write(posVec.Y, Endian.Big);
                                        stream.Write(posVec.Z, Endian.Big);
                                        break;
                                }
                            }
                            break;
                        case GXVertexAttribute.Normal:
                            stream.Position = baseOffset + 0x10;
                            stream.Write((int)(stream.Length - baseOffset), Endian.Big);
                            stream.Position = (int)endOffset;

                            foreach (Vector3 normVec in Attributes.Normals)
                            {
                                switch (StorageFormats[attrib].Item1)
                                {
                                    case GXDataType.Unsigned8:
                                        stream.WriteByte((byte)Math.Round(normVec.X * (1 << StorageFormats[attrib].Item2)));
                                        stream.WriteByte((byte)Math.Round(normVec.Y * (1 << StorageFormats[attrib].Item2)));
                                        stream.WriteByte((byte)Math.Round(normVec.Z * (1 << StorageFormats[attrib].Item2)));
                                        break;
                                    case GXDataType.Signed8:
                                        stream.WriteByte((byte)((sbyte)Math.Round(normVec.X * (1 << StorageFormats[attrib].Item2))));
                                        stream.WriteByte((byte)((sbyte)Math.Round(normVec.Y * (1 << StorageFormats[attrib].Item2))));
                                        stream.WriteByte((byte)((sbyte)Math.Round(normVec.Z * (1 << StorageFormats[attrib].Item2))));
                                        break;
                                    case GXDataType.Unsigned16:
                                        stream.Write((ushort)Math.Round(normVec.X * (1 << StorageFormats[attrib].Item2)), Endian.Big);
                                        stream.Write((ushort)Math.Round(normVec.Y * (1 << StorageFormats[attrib].Item2)), Endian.Big);
                                        stream.Write((ushort)Math.Round(normVec.Z * (1 << StorageFormats[attrib].Item2)), Endian.Big);
                                        break;
                                    case GXDataType.Signed16:
                                        stream.Write((short)Math.Round(normVec.X * (1 << StorageFormats[attrib].Item2)), Endian.Big);
                                        stream.Write((short)Math.Round(normVec.Y * (1 << StorageFormats[attrib].Item2)), Endian.Big);
                                        stream.Write((short)Math.Round(normVec.Z * (1 << StorageFormats[attrib].Item2)), Endian.Big);
                                        break;
                                    case GXDataType.Float32:
                                        stream.Write(normVec.X, Endian.Big);
                                        stream.Write(normVec.Y, Endian.Big);
                                        stream.Write(normVec.Z, Endian.Big);
                                        break;
                                }
                            }
                            break;
                        case GXVertexAttribute.Color0:
                        case GXVertexAttribute.Color1:
                            stream.Position = baseOffset + 0x18 + (int)(attrib - 11) * 4;
                            stream.Write((int)(stream.Length - baseOffset), Endian.Big);
                            stream.Position = (int)endOffset;

                            foreach (Color4 col in (List<Color4>)Attributes.GetAttributeData(attrib))
                            {
                                stream.WriteByte((byte)(col.R * 255));
                                stream.WriteByte((byte)(col.G * 255));
                                stream.WriteByte((byte)(col.B * 255));
                                stream.WriteByte((byte)(col.A * 255));
                            }
                            break;
                        case GXVertexAttribute.Tex0:
                        case GXVertexAttribute.Tex1:
                        case GXVertexAttribute.Tex2:
                        case GXVertexAttribute.Tex3:
                        case GXVertexAttribute.Tex4:
                        case GXVertexAttribute.Tex5:
                        case GXVertexAttribute.Tex6:
                        case GXVertexAttribute.Tex7:
                            stream.Position = baseOffset + 0x20 + (int)(attrib - 13) * 4;
                            stream.Write((int)(stream.Length - baseOffset), Endian.Big);
                            stream.Position = (int)endOffset;

                            foreach (Vector2 texVec in (List<Vector2>)Attributes.GetAttributeData(attrib))
                            {
                                switch (StorageFormats[attrib].Item1)
                                {
                                    case GXDataType.Unsigned8:
                                        stream.WriteByte((byte)Math.Round(texVec.X * (1 << StorageFormats[attrib].Item2)));
                                        stream.WriteByte((byte)Math.Round(texVec.Y * (1 << StorageFormats[attrib].Item2)));
                                        break;
                                    case GXDataType.Signed8:
                                        stream.WriteByte((byte)((sbyte)Math.Round(texVec.X * (1 << StorageFormats[attrib].Item2))));
                                        stream.WriteByte((byte)((sbyte)Math.Round(texVec.Y * (1 << StorageFormats[attrib].Item2))));
                                        break;
                                    case GXDataType.Unsigned16:
                                        stream.Write((ushort)Math.Round(texVec.X * (1 << StorageFormats[attrib].Item2)), Endian.Big);
                                        stream.Write((ushort)Math.Round(texVec.Y * (1 << StorageFormats[attrib].Item2)), Endian.Big);
                                        break;
                                    case GXDataType.Signed16:
                                        stream.Write((short)Math.Round(texVec.X * (1 << StorageFormats[attrib].Item2)), Endian.Big);
                                        stream.Write((short)Math.Round(texVec.Y * (1 << StorageFormats[attrib].Item2)), Endian.Big);
                                        break;
                                    case GXDataType.Float32:
                                        stream.Write(texVec.X, Endian.Big);
                                        stream.Write(texVec.Y, Endian.Big);
                                        break;
                                }
                            }
                            break;
                    }
                    stream.WriteAlign(32, Padding);
                }
            }

            internal void StipUnused(SHP1 Shapes)
            {
                List<SHP1.Vertex> UsedVerticies = Shapes.GetAllUsedVertices();
                SortedDictionary<uint, Vector3> NewPositions = new SortedDictionary<uint, Vector3>(), NewNormals = new SortedDictionary<uint, Vector3>();
                SortedDictionary<uint, Color4> NewColours0 = new SortedDictionary<uint, Color4>(), NewColours1 = new SortedDictionary<uint, Color4>();
                SortedDictionary<uint, Vector2> NewTexCoord0 = new SortedDictionary<uint, Vector2>(), NewTexCoord1 = new SortedDictionary<uint, Vector2>(),
                    NewTexCoord2 = new SortedDictionary<uint, Vector2>(), NewTexCoord3 = new SortedDictionary<uint, Vector2>(), NewTexCoord4 = new SortedDictionary<uint, Vector2>(),
                    NewTexCoord5 = new SortedDictionary<uint, Vector2>(), NewTexCoord6 = new SortedDictionary<uint, Vector2>(), NewTexCoord7 = new SortedDictionary<uint, Vector2>();

                bool HasPosition = Attributes.ContainsAttribute(GXVertexAttribute.Position), HasNormal = Attributes.ContainsAttribute(GXVertexAttribute.Normal),
                    HasColour0 = Attributes.ContainsAttribute(GXVertexAttribute.Color0), HasColour1 = Attributes.ContainsAttribute(GXVertexAttribute.Color1),
                    HasTex0 = Attributes.ContainsAttribute(GXVertexAttribute.Tex0), HasTex1 = Attributes.ContainsAttribute(GXVertexAttribute.Tex1),
                    HasTex2 = Attributes.ContainsAttribute(GXVertexAttribute.Tex2), HasTex3 = Attributes.ContainsAttribute(GXVertexAttribute.Tex3),
                    HasTex4 = Attributes.ContainsAttribute(GXVertexAttribute.Tex4), HasTex5 = Attributes.ContainsAttribute(GXVertexAttribute.Tex5),
                    HasTex6 = Attributes.ContainsAttribute(GXVertexAttribute.Tex6), HasTex7 = Attributes.ContainsAttribute(GXVertexAttribute.Tex7);

                for (int i = 0; i < UsedVerticies.Count; i++)
                {
                    if (HasPosition && !NewPositions.ContainsKey(UsedVerticies[i].PositionIndex))
                        NewPositions.Add(UsedVerticies[i].PositionIndex, Attributes.Positions[(int)UsedVerticies[i].PositionIndex]);

                    if (HasNormal && !NewNormals.ContainsKey(UsedVerticies[i].NormalIndex))
                        NewNormals.Add(UsedVerticies[i].NormalIndex, Attributes.Normals[(int)UsedVerticies[i].NormalIndex]);


                    if (HasColour0 && !NewColours0.ContainsKey(UsedVerticies[i].Color0Index))
                        NewColours0.Add(UsedVerticies[i].Color0Index, Attributes.Color_0[(int)UsedVerticies[i].Color0Index]);

                    if (HasColour1 && !NewColours1.ContainsKey(UsedVerticies[i].Color1Index))
                        NewColours1.Add(UsedVerticies[i].Color1Index, Attributes.Color_1[(int)UsedVerticies[i].Color1Index]);


                    if (HasTex0 && !NewTexCoord0.ContainsKey(UsedVerticies[i].TexCoord0Index))
                        NewTexCoord0.Add(UsedVerticies[i].TexCoord0Index, Attributes.TexCoord_0[(int)UsedVerticies[i].TexCoord0Index]);

                    if (HasTex1 && !NewTexCoord1.ContainsKey(UsedVerticies[i].TexCoord1Index))
                        NewTexCoord1.Add(UsedVerticies[i].TexCoord1Index, Attributes.TexCoord_1[(int)UsedVerticies[i].TexCoord1Index]);

                    if (HasTex2 && !NewTexCoord2.ContainsKey(UsedVerticies[i].TexCoord2Index))
                        NewTexCoord2.Add(UsedVerticies[i].TexCoord2Index, Attributes.TexCoord_2[(int)UsedVerticies[i].TexCoord2Index]);

                    if (HasTex3 && !NewTexCoord3.ContainsKey(UsedVerticies[i].TexCoord3Index))
                        NewTexCoord3.Add(UsedVerticies[i].TexCoord3Index, Attributes.TexCoord_3[(int)UsedVerticies[i].TexCoord3Index]);

                    if (HasTex4 && !NewTexCoord4.ContainsKey(UsedVerticies[i].TexCoord4Index))
                        NewTexCoord4.Add(UsedVerticies[i].TexCoord4Index, Attributes.TexCoord_4[(int)UsedVerticies[i].TexCoord4Index]);

                    if (HasTex5 && !NewTexCoord5.ContainsKey(UsedVerticies[i].TexCoord5Index))
                        NewTexCoord5.Add(UsedVerticies[i].TexCoord5Index, Attributes.TexCoord_5[(int)UsedVerticies[i].TexCoord5Index]);

                    if (HasTex6 && !NewTexCoord6.ContainsKey(UsedVerticies[i].TexCoord6Index))
                        NewTexCoord6.Add(UsedVerticies[i].TexCoord6Index, Attributes.TexCoord_6[(int)UsedVerticies[i].TexCoord6Index]);

                    if (HasTex7 && !NewTexCoord7.ContainsKey(UsedVerticies[i].TexCoord7Index))
                        NewTexCoord7.Add(UsedVerticies[i].TexCoord7Index, Attributes.TexCoord_7[(int)UsedVerticies[i].TexCoord7Index]);
                }

                if (HasPosition)
                    Attributes.SetAttributeData(GXVertexAttribute.Position, NewPositions.Values.ToList());

                if (HasNormal)
                    Attributes.SetAttributeData(GXVertexAttribute.Normal, NewNormals.Values.ToList());

                if (HasColour0)
                    Attributes.SetAttributeData(GXVertexAttribute.Color0, NewColours0.Values.ToList());
                if (HasColour1)
                    Attributes.SetAttributeData(GXVertexAttribute.Color1, NewColours1.Values.ToList());

                if (HasTex0)
                    Attributes.SetAttributeData(GXVertexAttribute.Tex0, NewTexCoord0.Values.ToList());
                if (HasTex1)
                    Attributes.SetAttributeData(GXVertexAttribute.Tex1, NewTexCoord1.Values.ToList());
                if (HasTex2)
                    Attributes.SetAttributeData(GXVertexAttribute.Tex2, NewTexCoord2.Values.ToList());
                if (HasTex3)
                    Attributes.SetAttributeData(GXVertexAttribute.Tex3, NewTexCoord3.Values.ToList());
                if (HasTex4)
                    Attributes.SetAttributeData(GXVertexAttribute.Tex4, NewTexCoord4.Values.ToList());
                if (HasTex5)
                    Attributes.SetAttributeData(GXVertexAttribute.Tex5, NewTexCoord5.Values.ToList());
                if (HasTex6)
                    Attributes.SetAttributeData(GXVertexAttribute.Tex6, NewTexCoord6.Values.ToList());
                if (HasTex7)
                    Attributes.SetAttributeData(GXVertexAttribute.Tex7, NewTexCoord7.Values.ToList());
            }

            public Dictionary<GXVertexAttribute, object> this[SHP1.Vertex IndexProvider]
            {
                get
                {
                    return Attributes.FetchDataForShapeVertex(IndexProvider);
                }
            }

            public class VertexData
            {
                private List<GXVertexAttribute> m_Attributes = new List<GXVertexAttribute>();

                public List<Vector3> Positions { get; set; } = new List<Vector3>();
                public List<Vector3> Normals { get; set; } = new List<Vector3>();
                public List<Color4> Color_0 { get; private set; } = new List<Color4>();
                public List<Color4> Color_1 { get; private set; } = new List<Color4>();
                public List<Vector2> TexCoord_0 { get; private set; } = new List<Vector2>();
                public List<Vector2> TexCoord_1 { get; private set; } = new List<Vector2>();
                public List<Vector2> TexCoord_2 { get; private set; } = new List<Vector2>();
                public List<Vector2> TexCoord_3 { get; private set; } = new List<Vector2>();
                public List<Vector2> TexCoord_4 { get; private set; } = new List<Vector2>();
                public List<Vector2> TexCoord_5 { get; private set; } = new List<Vector2>();
                public List<Vector2> TexCoord_6 { get; private set; } = new List<Vector2>();
                public List<Vector2> TexCoord_7 { get; private set; } = new List<Vector2>();

                public VertexData() { }

                public bool ContainsAttribute(GXVertexAttribute attribute) => m_Attributes.Contains(attribute);

                public object GetAttributeData(GXVertexAttribute attribute)
                {
                    if (!ContainsAttribute(attribute))
                        return null;

                    switch (attribute)
                    {
                        case GXVertexAttribute.Position:
                            return Positions;
                        case GXVertexAttribute.Normal:
                            return Normals;
                        case GXVertexAttribute.Color0:
                            return Color_0;
                        case GXVertexAttribute.Color1:
                            return Color_1;
                        case GXVertexAttribute.Tex0:
                            return TexCoord_0;
                        case GXVertexAttribute.Tex1:
                            return TexCoord_1;
                        case GXVertexAttribute.Tex2:
                            return TexCoord_2;
                        case GXVertexAttribute.Tex3:
                            return TexCoord_3;
                        case GXVertexAttribute.Tex4:
                            return TexCoord_4;
                        case GXVertexAttribute.Tex5:
                            return TexCoord_5;
                        case GXVertexAttribute.Tex6:
                            return TexCoord_6;
                        case GXVertexAttribute.Tex7:
                            return TexCoord_7;
                        default:
                            throw new ArgumentException("attribute");
                    }
                }

                public void SetAttributeData(GXVertexAttribute attribute, object data)
                {
                    if (!ContainsAttribute(attribute))
                        m_Attributes.Add(attribute);

                    switch (attribute)
                    {
                        case GXVertexAttribute.Position:
                            if (data.GetType() != typeof(List<Vector3>))
                                throw new ArgumentException("position data");
                            else
                                Positions = (List<Vector3>)data;
                            break;
                        case GXVertexAttribute.Normal:
                            if (data.GetType() != typeof(List<Vector3>))
                                throw new ArgumentException("normal data");
                            else
                                Normals = (List<Vector3>)data;
                            break;
                        case GXVertexAttribute.Color0:
                            if (data.GetType() != typeof(List<Color4>))
                                throw new ArgumentException("color0 data");
                            else
                                Color_0 = (List<Color4>)data;
                            break;
                        case GXVertexAttribute.Color1:
                            if (data.GetType() != typeof(List<Color4>))
                                throw new ArgumentException("color1 data");
                            else
                                Color_1 = (List<Color4>)data;
                            break;
                        case GXVertexAttribute.Tex0:
                            if (data.GetType() != typeof(List<Vector2>))
                                throw new ArgumentException("texcoord0 data");
                            else
                                TexCoord_0 = (List<Vector2>)data;
                            break;
                        case GXVertexAttribute.Tex1:
                            if (data.GetType() != typeof(List<Vector2>))
                                throw new ArgumentException("texcoord1 data");
                            else
                                TexCoord_1 = (List<Vector2>)data;
                            break;
                        case GXVertexAttribute.Tex2:
                            if (data.GetType() != typeof(List<Vector2>))
                                throw new ArgumentException("texcoord2 data");
                            else
                                TexCoord_2 = (List<Vector2>)data;
                            break;
                        case GXVertexAttribute.Tex3:
                            if (data.GetType() != typeof(List<Vector2>))
                                throw new ArgumentException("texcoord3 data");
                            else
                                TexCoord_3 = (List<Vector2>)data;
                            break;
                        case GXVertexAttribute.Tex4:
                            if (data.GetType() != typeof(List<Vector2>))
                                throw new ArgumentException("texcoord4 data");
                            else
                                TexCoord_4 = (List<Vector2>)data;
                            break;
                        case GXVertexAttribute.Tex5:
                            if (data.GetType() != typeof(List<Vector2>))
                                throw new ArgumentException("texcoord5 data");
                            else
                                TexCoord_5 = (List<Vector2>)data;
                            break;
                        case GXVertexAttribute.Tex6:
                            if (data.GetType() != typeof(List<Vector2>))
                                throw new ArgumentException("texcoord6 data");
                            else
                                TexCoord_6 = (List<Vector2>)data;
                            break;
                        case GXVertexAttribute.Tex7:
                            if (data.GetType() != typeof(List<Vector2>))
                                throw new ArgumentException("texcoord7 data");
                            else
                                TexCoord_7 = (List<Vector2>)data;
                            break;
                    }
                }

                public void SetAttributesFromList(List<GXVertexAttribute> attributes)
                {
                    m_Attributes = new List<GXVertexAttribute>(attributes);
                }

                internal Dictionary<GXVertexAttribute, object> FetchDataForShapeVertex(SHP1.Vertex Source)
                {
                    Dictionary<GXVertexAttribute, object> Values = new Dictionary<GXVertexAttribute, object>();
                    foreach (GXVertexAttribute Attribute in m_Attributes)
                    {
                        switch (Attribute)
                        {
                            case GXVertexAttribute.Position:
                                Values.Add(Attribute, Positions[(int)Source.PositionIndex]);
                                break;
                            case GXVertexAttribute.Normal:
                                Values.Add(Attribute, Normals[(int)Source.NormalIndex]);
                                break;
                            case GXVertexAttribute.Color0:
                                Values.Add(Attribute, Color_0[(int)Source.Color0Index]);
                                break;
                            case GXVertexAttribute.Color1:
                                Values.Add(Attribute, Color_1[(int)Source.Color1Index]);
                                break;
                            case GXVertexAttribute.Tex0:
                                Values.Add(Attribute, TexCoord_0[(int)Source.TexCoord0Index]);
                                break;
                            case GXVertexAttribute.Tex1:
                                Values.Add(Attribute, TexCoord_1[(int)Source.TexCoord1Index]);
                                break;
                            case GXVertexAttribute.Tex2:
                                Values.Add(Attribute, TexCoord_2[(int)Source.TexCoord2Index]);
                                break;
                            case GXVertexAttribute.Tex3:
                                Values.Add(Attribute, TexCoord_3[(int)Source.TexCoord3Index]);
                                break;
                            case GXVertexAttribute.Tex4:
                                Values.Add(Attribute, TexCoord_4[(int)Source.TexCoord4Index]);
                                break;
                            case GXVertexAttribute.Tex5:
                                Values.Add(Attribute, TexCoord_5[(int)Source.TexCoord5Index]);
                                break;
                            case GXVertexAttribute.Tex6:
                                Values.Add(Attribute, TexCoord_6[(int)Source.TexCoord6Index]);
                                break;
                            case GXVertexAttribute.Tex7:
                                Values.Add(Attribute, TexCoord_7[(int)Source.TexCoord7Index]);
                                break;
                            default:
                                throw new ArgumentException("attribute");
                        }
                    }
                    return Values;
                }
            }
        }

        //=====================================================================
    }
}
