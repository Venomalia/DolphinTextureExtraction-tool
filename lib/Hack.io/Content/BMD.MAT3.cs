using AuroraLip.Common;
using AuroraLip.Texture.Formats;
using AuroraLip.Texture.J3D;
using OpenTK.Mathematics;

//Heavily based on the SuperBMD Library.
namespace Hack.io
{
    public partial class BMD
    {
        public class MAT3
        {
            #region Fields and Properties
            private List<Material> m_Materials;
            public Material this[string MaterialName]
            {
                get
                {
                    for (int i = 0; i < m_Materials.Count; i++)
                    {
                        if (m_Materials[i].Name.Equals(MaterialName))
                            return m_Materials[i];
                    }
                    return null;
                }
                set
                {
                    if (!(value is Material || value is null))
                        throw new ArgumentException("Value is not a Material!", "value");
                    for (int i = 0; i < m_Materials.Count; i++)
                    {
                        if (m_Materials[i].Name.Equals(MaterialName))
                        {
                            if (value is null)
                                m_Materials.RemoveAt(i);
                            else
                                m_Materials[i] = value;
                            return;
                        }
                    }
                    if (!(value is null))
                        m_Materials.Add(value);
                }
            }
            public Material this[int Index]
            {
                get
                {
                    return m_Materials[Index];
                }
                set
                {
                    if (!(value is Material || value is null))
                        throw new ArgumentException("Value is not a Material!", "value");

                    m_Materials[Index] = value;
                }
            }

            public int Count => m_Materials.Count;
            #endregion

            private static readonly string Magic = "MAT3";

            public MAT3(Stream stream)
            {
                List<int> m_RemapIndices = new List<int>();
                List<string> m_MaterialNames = new List<string>();

                List<Material.IndirectTexturing> m_IndirectTexBlock = new List<Material.IndirectTexturing>();
                List<CullMode> m_CullModeBlock = new List<CullMode>();
                List<Color4> m_MaterialColorBlock = new List<Color4>();
                List<Material.ChannelControl> m_ChannelControlBlock = new List<Material.ChannelControl>();
                List<Color4> m_AmbientColorBlock = new List<Color4>();
                List<Color4> m_LightingColorBlock = new List<Color4>();
                List<Material.TexCoordGen> m_TexCoord1GenBlock = new List<Material.TexCoordGen>();
                List<Material.TexCoordGen> m_TexCoord2GenBlock = new List<Material.TexCoordGen>();
                List<Material.TexMatrix> m_TexMatrix1Block = new List<Material.TexMatrix>();
                List<Material.TexMatrix> m_TexMatrix2Block = new List<Material.TexMatrix>();
                List<short> m_TexRemapBlock = new List<short>();
                List<Material.TevOrder> m_TevOrderBlock = new List<Material.TevOrder>();
                List<Color4> m_TevColorBlock = new List<Color4>();
                List<Color4> m_TevKonstColorBlock = new List<Color4>();
                List<Material.TevStage> m_TevStageBlock = new List<Material.TevStage>();
                List<Material.TevSwapMode> m_SwapModeBlock = new List<Material.TevSwapMode>();
                List<Material.TevSwapModeTable> m_SwapTableBlock = new List<Material.TevSwapModeTable>();
                List<Material.Fog> m_FogBlock = new List<Material.Fog>();
                List<Material.AlphaCompare> m_AlphaCompBlock = new List<Material.AlphaCompare>();
                List<Material.BlendMode> m_blendModeBlock = new List<Material.BlendMode>();
                List<Material.NBTScaleHolder> m_NBTScaleBlock = new List<Material.NBTScaleHolder>();

                List<Material.ZModeHolder> m_zModeBlock = new List<Material.ZModeHolder>();
                List<bool> m_zCompLocBlock = new List<bool>();
                List<bool> m_ditherBlock = new List<bool>();

                List<byte> NumColorChannelsBlock = new List<byte>();
                List<byte> NumTexGensBlock = new List<byte>();
                List<byte> NumTevStagesBlock = new List<byte>();


                int ChunkStart = (int)stream.Position;
                if (!stream.ReadString(4).Equals(Magic))
                    throw new InvalidIdentifierException(Magic);

                int mat3Size = stream.ReadInt32(Endian.Big);
                int matCount = stream.ReadInt16(Endian.Big);
                long matInitOffset = 0;
                stream.Position += 0x02;

                for (Mat3OffsetIndex i = 0; i <= Mat3OffsetIndex.NBTScaleData; ++i)
                {
                    int sectionOffset = stream.ReadInt32(Endian.Big);

                    if (sectionOffset == 0)
                        continue;

                    long curReaderPos = stream.Position;
                    int nextOffset = stream.ReadInt32(Endian.Big);
                    stream.Position -= 0x04;
                    int sectionSize = 0;

                    if (i == Mat3OffsetIndex.NBTScaleData)
                    {

                    }

                    if (nextOffset == 0 && i != Mat3OffsetIndex.NBTScaleData)
                    {
                        long saveReaderPos = stream.Position;

                        stream.Position += 4;

                        while (stream.ReadInt32(Endian.Big) == 0)
                            stream.Position += 0;

                        stream.Position -= 0x04;
                        nextOffset = stream.ReadInt32(Endian.Big);
                        stream.Position -= 0x04;
                        sectionSize = nextOffset - sectionOffset;

                        stream.Position = saveReaderPos;
                    }
                    else if (i == Mat3OffsetIndex.NBTScaleData)
                        sectionSize = mat3Size - sectionOffset;
                    else
                        sectionSize = nextOffset - sectionOffset;

                    stream.Position = ChunkStart + sectionOffset;
                    int count;
                    switch (i)
                    {
                        case Mat3OffsetIndex.MaterialData:
                            matInitOffset = stream.Position;
                            break;
                        case Mat3OffsetIndex.IndexData:
                            m_RemapIndices = new List<int>();

                            for (int index = 0; index < matCount; index++)
                                m_RemapIndices.Add(stream.ReadInt16(Endian.Big));

                            break;
                        case Mat3OffsetIndex.NameTable:
                            m_MaterialNames = new List<string>();

                            stream.Position = ChunkStart + sectionOffset;

                            short stringCount = stream.ReadInt16(Endian.Big);
                            stream.Position += 0x02;

                            for (int y = 0; y < stringCount; y++)
                            {
                                stream.Position += 0x02;
                                short nameOffset = stream.ReadInt16(Endian.Big);
                                long saveReaderPos = stream.Position;
                                stream.Position = ChunkStart + sectionOffset + nameOffset;

                                m_MaterialNames.Add(stream.ReadString());

                                stream.Position = saveReaderPos;
                            }
                            break;
                        case Mat3OffsetIndex.IndirectData:
                            m_IndirectTexBlock = new List<Material.IndirectTexturing>();
                            count = sectionSize / 312;

                            for (int y = 0; y < count; y++)
                                m_IndirectTexBlock.Add(new Material.IndirectTexturing(stream));
                            break;
                        case Mat3OffsetIndex.CullMode:
                            m_CullModeBlock = new List<CullMode>();
                            count = sectionSize / 4;

                            for (int y = 0; y < count; y++)
                                m_CullModeBlock.Add((CullMode)stream.ReadInt32(Endian.Big));
                            break;
                        case Mat3OffsetIndex.MaterialColor:
                            m_MaterialColorBlock = ReadColours(stream, sectionOffset, sectionSize);
                            break;
                        case Mat3OffsetIndex.ColorChannelCount:
                            NumColorChannelsBlock = new List<byte>();

                            for (int chanCnt = 0; chanCnt < sectionSize; chanCnt++)
                            {
                                byte chanCntIn = (byte)stream.ReadByte();

                                if (chanCntIn < 84)
                                    NumColorChannelsBlock.Add(chanCntIn);
                            }

                            break;
                        case Mat3OffsetIndex.ColorChannelData:
                            m_ChannelControlBlock = new List<Material.ChannelControl>();
                            count = sectionSize / 8;

                            for (int y = 0; y < count; y++)
                                m_ChannelControlBlock.Add(new Material.ChannelControl(stream));
                            break;
                        case Mat3OffsetIndex.AmbientColorData:
                            m_AmbientColorBlock = ReadColours(stream, sectionOffset, sectionSize);
                            break;
                        case Mat3OffsetIndex.LightData:
                            m_LightingColorBlock = ReadColours(stream, sectionOffset, sectionSize);
                            break;
                        case Mat3OffsetIndex.TexGenCount:
                            NumTexGensBlock = new List<byte>();

                            for (int genCnt = 0; genCnt < sectionSize; genCnt++)
                            {
                                byte genCntIn = (byte)stream.ReadByte();

                                if (genCntIn < 84)
                                    NumTexGensBlock.Add(genCntIn);
                            }

                            break;
                        case Mat3OffsetIndex.TexCoordData:
                            m_TexCoord1GenBlock = ReadTexCoordGens(stream, sectionOffset, sectionSize);
                            break;
                        case Mat3OffsetIndex.TexCoord2Data:
                            m_TexCoord2GenBlock = ReadTexCoordGens(stream, sectionOffset, sectionSize);
                            break;
                        case Mat3OffsetIndex.TexMatrixData:
                            m_TexMatrix1Block = ReadTexMatrices(stream, sectionOffset, sectionSize);
                            break;
                        case Mat3OffsetIndex.TexMatrix2Data:
                            m_TexMatrix2Block = ReadTexMatrices(stream, sectionOffset, sectionSize);
                            break;
                        case Mat3OffsetIndex.TexNoData:
                            m_TexRemapBlock = new List<short>();
                            int texNoCnt = sectionSize / 2;

                            for (int texNo = 0; texNo < texNoCnt; texNo++)
                                m_TexRemapBlock.Add(stream.ReadInt16(Endian.Big));

                            break;
                        case Mat3OffsetIndex.TevOrderData:
                            m_TevOrderBlock = new List<Material.TevOrder>();
                            count = sectionSize / 4;

                            for (int y = 0; y < count; y++)
                                m_TevOrderBlock.Add(new Material.TevOrder(stream));
                            break;
                        case Mat3OffsetIndex.TevColorData:
                            m_TevColorBlock = ReadColours(stream, sectionOffset, sectionSize, true);
                            break;
                        case Mat3OffsetIndex.TevKColorData:
                            m_TevKonstColorBlock = ReadColours(stream, sectionOffset, sectionSize);
                            break;
                        case Mat3OffsetIndex.TevStageCount:
                            NumTevStagesBlock = new List<byte>();

                            for (int stgCnt = 0; stgCnt < sectionSize; stgCnt++)
                            {
                                byte stgCntIn = (byte)stream.ReadByte();

                                if (stgCntIn < 84)
                                    NumTevStagesBlock.Add(stgCntIn);
                            }

                            break;
                        case Mat3OffsetIndex.TevStageData:
                            m_TevStageBlock = new List<Material.TevStage>();
                            count = sectionSize / 20;

                            for (int y = 0; y < count; y++)
                                m_TevStageBlock.Add(new Material.TevStage(stream));
                            break;
                        case Mat3OffsetIndex.TevSwapModeData:
                            m_SwapModeBlock = new List<Material.TevSwapMode>();
                            count = sectionSize / 4;

                            for (int y = 0; y < count; y++)
                                m_SwapModeBlock.Add(new Material.TevSwapMode(stream));
                            break;
                        case Mat3OffsetIndex.TevSwapModeTable:
                            m_SwapTableBlock = new List<Material.TevSwapModeTable>();
                            count = sectionSize / 4;

                            for (int y = 0; y < count; y++)
                                m_SwapTableBlock.Add(new Material.TevSwapModeTable(stream));
                            break;
                        case Mat3OffsetIndex.FogData:
                            m_FogBlock = new List<Material.Fog>();
                            count = sectionSize / 44;

                            for (int y = 0; y < count; y++)
                                m_FogBlock.Add(new Material.Fog(stream));
                            break;
                        case Mat3OffsetIndex.AlphaCompareData:
                            m_AlphaCompBlock = new List<Material.AlphaCompare>();
                            count = sectionSize / 8;

                            for (int y = 0; y < count; y++)
                                m_AlphaCompBlock.Add(new Material.AlphaCompare(stream));
                            break;
                        case Mat3OffsetIndex.BlendData:
                            m_blendModeBlock = new List<Material.BlendMode>();
                            count = sectionSize / 4;

                            for (int y = 0; y < count; y++)
                                m_blendModeBlock.Add(new Material.BlendMode(stream));
                            break;
                        case Mat3OffsetIndex.ZModeData:
                            m_zModeBlock = new List<Material.ZModeHolder>();
                            count = sectionSize / 4;

                            for (int y = 0; y < count; y++)
                                m_zModeBlock.Add(new Material.ZModeHolder(stream));
                            break;
                        case Mat3OffsetIndex.ZCompLoc:
                            m_zCompLocBlock = new List<bool>();

                            for (int zcomp = 0; zcomp < sectionSize; zcomp++)
                            {
                                byte boolIn = (byte)stream.ReadByte();

                                if (boolIn > 1)
                                    break;

                                m_zCompLocBlock.Add(Convert.ToBoolean(boolIn));
                            }

                            break;
                        case Mat3OffsetIndex.DitherData:
                            m_ditherBlock = new List<bool>();

                            for (int dith = 0; dith < sectionSize; dith++)
                            {
                                byte boolIn = (byte)stream.ReadByte();

                                if (boolIn > 1)
                                    break;

                                m_ditherBlock.Add(Convert.ToBoolean(boolIn));
                            }

                            break;
                        case Mat3OffsetIndex.NBTScaleData:
                            m_NBTScaleBlock = new List<Material.NBTScaleHolder>();
                            count = sectionSize / 16;

                            for (int y = 0; y < count; y++)
                                m_NBTScaleBlock.Add(new Material.NBTScaleHolder(stream));
                            break;
                    }

                    stream.Position = curReaderPos;
                }

                int highestMatIndex = 0;

                for (int i = 0; i < matCount; i++)
                {
                    if (m_RemapIndices[i] > highestMatIndex)
                        highestMatIndex = m_RemapIndices[i];
                }

                stream.Position = matInitOffset;
                m_Materials = new List<Material>();
                for (int i = 0; i <= highestMatIndex; i++)
                {
                    LoadInitData(stream, m_RemapIndices[i], m_MaterialNames, m_IndirectTexBlock, m_CullModeBlock, m_MaterialColorBlock, m_ChannelControlBlock, m_AmbientColorBlock,
                        m_LightingColorBlock, m_TexCoord1GenBlock, m_TexCoord2GenBlock, m_TexMatrix1Block, m_TexMatrix2Block, m_TexRemapBlock, m_TevOrderBlock, m_TevColorBlock,
                        m_TevKonstColorBlock, m_TevStageBlock, m_SwapModeBlock, m_SwapTableBlock, m_FogBlock, m_AlphaCompBlock, m_blendModeBlock, m_NBTScaleBlock, m_zModeBlock,
                        m_zCompLocBlock, m_ditherBlock, NumColorChannelsBlock, NumTexGensBlock, NumTevStagesBlock);
                }

                stream.Seek(ChunkStart + mat3Size, SeekOrigin.Begin);

                List<Material> matCopies = new List<Material>();
                for (int i = 0; i < m_RemapIndices.Count; i++)
                {
                    Material originalMat = m_Materials[m_RemapIndices[i]];
                    Material copyMat = new Material(originalMat) { Name = m_MaterialNames[i] };
                    matCopies.Add(copyMat);
                }

                m_Materials = matCopies;
            }

            public bool Any(Func<Material, bool> predicate)
            {
                if (predicate == null)
                    throw new ArgumentException("predicate");
                foreach (Material element in m_Materials)
                    if (predicate(element))
                        return true;
                return false;
            }

            public void Sort(Comparison<Material> comparer)
            {
                m_Materials.Sort(comparer);
            }
            public static Comparison<Material> SortByName => new Comparison<Material>((x, y) => string.Compare(x.Name, y.Name, true));
            public int IndexOf(Material mat)
            {
                for (int i = 0; i < m_Materials.Count; i++)
                {
                    if (object.ReferenceEquals(mat, m_Materials[i]))
                    {
                        return i;
                    }
                }
                return -1;
            }


            public void SetTextureNames(TEX1 textures)
            {
                foreach (Material mat in m_Materials)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (mat.TextureIndices[i] == -1)
                            continue;

                        mat.Textures[i] = textures[mat.TextureIndices[i]];
                        //mat.TextureNames[i] = textures[mat.TextureIndices[i]].FileName;
                        //mat.TextureNames[i] = textures.getTextureInstanceName(mat.TextureIndices[i]);
                    }
                }
            }

            #region I/O
            private void LoadInitData(Stream stream, int matindex, List<string> m_MaterialNames, List<Material.IndirectTexturing> m_IndirectTexBlock, List<CullMode> m_CullModeBlock,
            List<Color4> m_MaterialColorBlock, List<Material.ChannelControl> m_ChannelControlBlock, List<Color4> m_AmbientColorBlock, List<Color4> m_LightingColorBlock,
            List<Material.TexCoordGen> m_TexCoord1GenBlock, List<Material.TexCoordGen> m_TexCoord2GenBlock, List<Material.TexMatrix> m_TexMatrix1Block, List<Material.TexMatrix> m_TexMatrix2Block,
            List<short> m_TexRemapBlock, List<Material.TevOrder> m_TevOrderBlock, List<Color4> m_TevColorBlock, List<Color4> m_TevKonstColorBlock, List<Material.TevStage> m_TevStageBlock,
            List<Material.TevSwapMode> m_SwapModeBlock, List<Material.TevSwapModeTable> m_SwapTableBlock, List<Material.Fog> m_FogBlock, List<Material.AlphaCompare> m_AlphaCompBlock,
            List<Material.BlendMode> m_blendModeBlock, List<Material.NBTScaleHolder> m_NBTScaleBlock, List<Material.ZModeHolder> m_zModeBlock, List<bool> m_zCompLocBlock,
            List<bool> m_ditherBlock, List<byte> NumColorChannelsBlock, List<byte> NumTexGensBlock, List<byte> NumTevStagesBlock)
            {
                Material mat = new Material
                {
                    Name = m_MaterialNames[matindex],
                    Flag = (byte)stream.ReadByte(),
                    CullMode = m_CullModeBlock[stream.ReadByte()],
                    LightChannelCount = NumColorChannelsBlock[stream.ReadByte()]
                };
                stream.Position += 0x02;

                if (matindex < m_IndirectTexBlock.Count)
                {
                    mat.IndTexEntry = m_IndirectTexBlock[matindex];
                }
                else
                {
                    Events.NotificationEvent?.Invoke(NotificationType.Warning, $"Material {mat.Name} referenced an out of range IndirectTexBlock index");
                }
                mat.ZCompLoc = m_zCompLocBlock[stream.ReadByte()];
                mat.ZMode = m_zModeBlock[stream.ReadByte()];

                if (m_ditherBlock == null || m_ditherBlock.Count == 0)
                    stream.Position++;
                else
                    mat.Dither = m_ditherBlock[stream.ReadByte()];

                int matColorIndex = stream.ReadInt16(Endian.Big);
                if (matColorIndex != -1)
                    mat.MaterialColors[0] = m_MaterialColorBlock[matColorIndex];
                matColorIndex = stream.ReadInt16(Endian.Big);
                if (matColorIndex != -1)
                    mat.MaterialColors[1] = m_MaterialColorBlock[matColorIndex];

                for (int i = 0; i < 4; i++)
                {
                    int chanIndex = stream.ReadInt16(Endian.Big);
                    if (chanIndex == -1)
                        continue;
                    else if (chanIndex < m_ChannelControlBlock.Count)
                    {
                        mat.ChannelControls[i] = m_ChannelControlBlock[chanIndex];
                    }
                    else
                    {
                        Events.NotificationEvent?.Invoke(NotificationType.Warning, $"Material {mat.Name} i={i}, color channel index out of range: {chanIndex}");
                    }
                }
                for (int i = 0; i < 2; i++)
                {
                    int ambColorIndex = stream.ReadInt16(Endian.Big);
                    if (ambColorIndex == -1)
                        continue;
                    else if (ambColorIndex < m_AmbientColorBlock.Count)
                    {
                        mat.AmbientColors[i] = m_AmbientColorBlock[ambColorIndex];
                    }
                    else
                    {
                        Events.NotificationEvent?.Invoke(NotificationType.Warning, $"Material {mat.Name} i={i}, ambient color index out of range: {ambColorIndex}");
                    }
                }

                for (int i = 0; i < 8; i++)
                {
                    int lightIndex = stream.ReadInt16(Endian.Big);
                    if ((lightIndex == -1) || (lightIndex > m_LightingColorBlock.Count) || (m_LightingColorBlock.Count == 0))
                        continue;
                    else
                        mat.LightingColors[i] = m_LightingColorBlock[lightIndex];
                }

                for (int i = 0; i < 8; i++)
                {
                    int texGenIndex = stream.ReadInt16(Endian.Big);
                    if (texGenIndex == -1)
                        continue;
                    else if (texGenIndex < m_TexCoord1GenBlock.Count)
                        mat.TexCoord1Gens[i] = m_TexCoord1GenBlock[texGenIndex];
                    else
                        Events.NotificationEvent?.Invoke(NotificationType.Warning, $"Material {mat.Name} i={i}, TexCoord1GenBlock index out of range: {texGenIndex}");
                }

                for (int i = 0; i < 8; i++)
                {
                    int texGenIndex = stream.ReadInt16(Endian.Big);
                    if (texGenIndex == -1)
                        continue;
                    else
                        mat.PostTexCoordGens[i] = m_TexCoord2GenBlock[texGenIndex];
                }

                for (int i = 0; i < 10; i++)
                {
                    int texMatIndex = stream.ReadInt16(Endian.Big);
                    if (texMatIndex == -1)
                        continue;
                    else
                        mat.TexMatrix1[i] = m_TexMatrix1Block[texMatIndex];
                }

                for (int i = 0; i < 20; i++)
                {
                    int texMatIndex = stream.ReadInt16(Endian.Big);
                    if (texMatIndex == -1)
                        continue;
                    else if (texMatIndex < (m_TexMatrix2Block?.Count ?? 0))
                        mat.PostTexMatrix[i] = m_TexMatrix2Block[texMatIndex];
                    else
                        Events.NotificationEvent?.Invoke(NotificationType.Warning, $"Material {mat.Name}, TexMatrix2Block index out of range: {texMatIndex}");
                }

                for (int i = 0; i < 8; i++)
                {
                    int texIndex = stream.ReadInt16(Endian.Big);
                    if (texIndex == -1)
                        continue;
                    else
                        mat.TextureIndices[i] = m_TexRemapBlock[texIndex];
                }

                for (int i = 0; i < 4; i++)
                {
                    int tevKColor = stream.ReadInt16(Endian.Big);
                    if (tevKColor == -1)
                        continue;
                    else
                        mat.KonstColors[i] = m_TevKonstColorBlock[tevKColor];
                }

                for (int i = 0; i < 16; i++)
                {
                    mat.ColorSels[i] = (KonstColorSel)stream.ReadByte();
                }

                for (int i = 0; i < 16; i++)
                {
                    mat.AlphaSels[i] = (KonstAlphaSel)stream.ReadByte();
                }

                for (int i = 0; i < 16; i++)
                {
                    int tevOrderIndex = stream.ReadInt16(Endian.Big);
                    if (tevOrderIndex == -1)
                        continue;
                    else
                        mat.TevOrders[i] = m_TevOrderBlock[tevOrderIndex];
                }

                for (int i = 0; i < 4; i++)
                {
                    int tevColor = stream.ReadInt16(Endian.Big);
                    if (tevColor == -1)
                        continue;
                    else
                        mat.TevColors[i] = m_TevColorBlock[tevColor];
                }

                for (int i = 0; i < 16; i++)
                {
                    int tevStageIndex = stream.ReadInt16(Endian.Big);
                    if (tevStageIndex == -1)
                        continue;
                    else
                        mat.TevStages[i] = m_TevStageBlock[tevStageIndex];
                }

                for (int i = 0; i < 16; i++)
                {
                    int tevSwapModeIndex = stream.ReadInt16(Endian.Big);
                    if (tevSwapModeIndex == -1)
                        continue;
                    else
                        mat.SwapModes[i] = m_SwapModeBlock[tevSwapModeIndex];
                }

                for (int i = 0; i < 16; i++)
                {
                    int tevSwapModeTableIndex = stream.ReadInt16(Endian.Big);
                    if ((tevSwapModeTableIndex < 0) || (tevSwapModeTableIndex >= m_SwapTableBlock.Count))
                        continue;
                    else
                    {
                        if (tevSwapModeTableIndex >= m_SwapTableBlock.Count)
                            continue;

                        mat.SwapTables[i] = m_SwapTableBlock[tevSwapModeTableIndex];
                    }
                }

                if (m_FogBlock.Count == 0)
                    stream.Position += 0x02;
                else
                    mat.FogInfo = m_FogBlock[stream.ReadInt16(Endian.Big)];
                mat.AlphCompare = m_AlphaCompBlock[stream.ReadInt16(Endian.Big)];
                mat.BMode = m_blendModeBlock[stream.ReadInt16(Endian.Big)];

                if (m_NBTScaleBlock.Count == 0)
                    stream.Position += 0x02;
                else
                    mat.NBTScale = m_NBTScaleBlock[stream.ReadInt16(Endian.Big)];
                m_Materials.Add(mat);
            }
            private static List<Color4> ReadColours(Stream stream, int offset, int size, bool IsInt16 = false)
            {
                List<Color4> colors = new List<Color4>();
                int count = size / (IsInt16 ? 8 : 4);

                if (IsInt16)
                {
                    for (int i = 0; i < count; i++)
                    {
                        short r = stream.ReadInt16(Endian.Big);
                        short g = stream.ReadInt16(Endian.Big);
                        short b = stream.ReadInt16(Endian.Big);
                        short a = stream.ReadInt16(Endian.Big);

                        colors.Add(new Color4((float)r / 255, (float)g / 255, (float)b / 255, (float)a / 255));
                    }
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        byte r = (byte)stream.ReadByte();
                        byte g = (byte)stream.ReadByte();
                        byte b = (byte)stream.ReadByte();
                        byte a = (byte)stream.ReadByte();

                        colors.Add(new Color4((float)r / 255, (float)g / 255, (float)b / 255, (float)a / 255));
                    }
                }


                return colors;
            }
            private static List<Material.TexCoordGen> ReadTexCoordGens(Stream reader, int offset, int size)
            {
                List<Material.TexCoordGen> gens = new List<Material.TexCoordGen>();
                int count = size / 4;

                for (int i = 0; i < count; i++)
                    gens.Add(new Material.TexCoordGen(reader));

                return gens;
            }
            private static List<Material.TexMatrix> ReadTexMatrices(Stream reader, int offset, int size)
            {
                List<Material.TexMatrix> matrices = new List<Material.TexMatrix>();
                int count = size / 100;

                for (int i = 0; i < count; i++)
                    matrices.Add(new Material.TexMatrix(reader));

                return matrices;
            }

            public void Write(Stream writer)
            {
                long start = writer.Position;
                List<int> m_RemapIndices = new List<int>();
                List<string> m_MaterialNames = new List<string>();

                List<Material.IndirectTexturing> m_IndirectTexBlock = new List<Material.IndirectTexturing>();
                List<CullMode> m_CullModeBlock = new List<CullMode>() { CullMode.Back, CullMode.Front, CullMode.None };
                List<Color4> m_MaterialColorBlock = new List<Color4>();
                List<Material.ChannelControl> m_ChannelControlBlock = new List<Material.ChannelControl>();
                List<Color4> m_AmbientColorBlock = new List<Color4>();
                List<Color4> m_LightingColorBlock = new List<Color4>();
                List<Material.TexCoordGen> m_TexCoord1GenBlock = new List<Material.TexCoordGen>();
                List<Material.TexCoordGen> m_TexCoord2GenBlock = new List<Material.TexCoordGen>();
                List<Material.TexMatrix> m_TexMatrix1Block = new List<Material.TexMatrix>();
                List<Material.TexMatrix> m_TexMatrix2Block = new List<Material.TexMatrix>();
                List<short> m_TexRemapBlock = new List<short>();
                List<Material.TevOrder> m_TevOrderBlock = new List<Material.TevOrder>();
                List<Color4> m_TevColorBlock = new List<Color4>();
                List<Color4> m_TevKonstColorBlock = new List<Color4>();
                List<Material.TevStage> m_TevStageBlock = new List<Material.TevStage>();
                List<Material.TevSwapMode> m_SwapModeBlock = new List<Material.TevSwapMode>() { new Material.TevSwapMode(0, 0), new Material.TevSwapMode(0, 0) };
                List<Material.TevSwapModeTable> m_SwapTableBlock = new List<Material.TevSwapModeTable>();
                List<Material.Fog> m_FogBlock = new List<Material.Fog>();
                List<Material.AlphaCompare> m_AlphaCompBlock = new List<Material.AlphaCompare>();
                List<Material.BlendMode> m_blendModeBlock = new List<Material.BlendMode>();
                List<Material.NBTScaleHolder> m_NBTScaleBlock = new List<Material.NBTScaleHolder>();

                List<Material.ZModeHolder> m_zModeBlock = new List<Material.ZModeHolder>();
                List<bool> m_zCompLocBlock = new List<bool>() { false, true };
                List<bool> m_ditherBlock = new List<bool>() { false, true };

                List<byte> NumColorChannelsBlock = new List<byte>();
                List<byte> NumTexGensBlock = new List<byte>();
                List<byte> NumTevStagesBlock = new List<byte>();

                // Calculate what the unique materials are and update the duplicate remap indices list.
                List<Material> uniqueMaterials = new List<Material>();
                for (int i = 0; i < m_Materials.Count; i++)
                {
                    Material mat = m_Materials[i];
                    int duplicateRemapIndex = -1;
                    for (int j = 0; j < i; j++)
                    {
                        Material othermat = m_Materials[j];
                        if (mat == othermat)
                        {
                            duplicateRemapIndex = uniqueMaterials.IndexOf(othermat);
                            break;
                        }
                    }
                    if (duplicateRemapIndex >= 0)
                        m_RemapIndices.Add(duplicateRemapIndex);
                    else
                    {
                        m_RemapIndices.Add(uniqueMaterials.Count);
                        uniqueMaterials.Add(mat);
                    }

                    m_MaterialNames.Add(mat.Name);

                    m_IndirectTexBlock.Add(mat.IndTexEntry);
                    if (m_Materials[i].LightChannelCount > 2)
                        m_Materials[i].LightChannelCount = 2;
                }

                writer.Write(Magic);
                writer.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for section size
                writer.WriteBigEndian(BitConverter.GetBytes((short)m_RemapIndices.Count), 0, 2);
                writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);

                writer.Write(new byte[4] { 0x00, 0x00, 0x00, 0x84 }, 0, 4); // Offset to material init data. Always 132

                int[] Offsets = new int[29];
                for (int i = 0; i < 29; i++)
                    writer.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for Offsets

                bool[] writtenCheck = new bool[uniqueMaterials.Count];
                List<string> names = m_MaterialNames;

                for (int i = 0; i < m_RemapIndices.Count; i++)
                {
                    if (writtenCheck[m_RemapIndices[i]])
                        continue;
                    else
                    {
                        WriteMaterialInitData(writer, uniqueMaterials[m_RemapIndices[i]], ref m_CullModeBlock, ref m_MaterialColorBlock, ref m_ChannelControlBlock, ref m_AmbientColorBlock,
                        ref m_LightingColorBlock, ref m_TexCoord1GenBlock, ref m_TexCoord2GenBlock, ref m_TexMatrix1Block, ref m_TexMatrix2Block, ref m_TexRemapBlock, ref m_TevOrderBlock, ref m_TevColorBlock,
                        ref m_TevKonstColorBlock, ref m_TevStageBlock, ref m_SwapModeBlock, ref m_SwapTableBlock, ref m_FogBlock, ref m_AlphaCompBlock, ref m_blendModeBlock, ref m_NBTScaleBlock, ref m_zModeBlock,
                        ref m_zCompLocBlock, ref m_ditherBlock, ref NumColorChannelsBlock, ref NumTexGensBlock, ref NumTevStagesBlock);
                        writtenCheck[m_RemapIndices[i]] = true;
                    }
                }

                long curOffset = writer.Position;

                // Remap indices offset
                Offsets[0] = (int)(curOffset - start);

                for (int i = 0; i < m_RemapIndices.Count; i++)
                    writer.WriteBigEndian(BitConverter.GetBytes((short)m_RemapIndices[i]), 0, 2);

                writer.WritePadding(4, Padding);

                curOffset = writer.Position;

                // Name table offset
                Offsets[1] = (int)(curOffset - start);

                writer.WriteStringTable(names);
                writer.WritePadding(8, Padding);

                curOffset = writer.Position;

                // Indirect texturing offset
                Offsets[2] = (int)(curOffset - start);

                //IndirectTexturingIO.Write(writer, m_IndirectTexBlock);
                foreach (Material.IndirectTexturing ind in m_IndirectTexBlock)
                    ind.Write(writer);

                curOffset = writer.Position;

                // Cull mode offset
                Offsets[3] = (int)(curOffset - start);

                //CullModeIO.Write(writer, m_CullModeBlock);
                for (int i = 0; i < m_CullModeBlock.Count; i++)
                    writer.WriteBigEndian(BitConverter.GetBytes((int)m_CullModeBlock[i]), 0, 4);

                curOffset = writer.Position;

                // Material colors offset
                Offsets[4] = (int)(curOffset - start);

                //ColorIO.Write(writer, m_MaterialColorBlock);
                for (int i = 0; i < m_MaterialColorBlock.Count; i++)
                {
                    writer.WriteByte((byte)(m_MaterialColorBlock[i].R * 255));
                    writer.WriteByte((byte)(m_MaterialColorBlock[i].G * 255));
                    writer.WriteByte((byte)(m_MaterialColorBlock[i].B * 255));
                    writer.WriteByte((byte)(m_MaterialColorBlock[i].A * 255));
                }

                curOffset = writer.Position;

                // Color channel count offset
                Offsets[5] = (int)(curOffset - start);

                foreach (byte chanNum in NumColorChannelsBlock)
                    writer.WriteByte(chanNum);

                writer.WritePadding(4, Padding);

                curOffset = writer.Position;

                // Color channel data offset
                Offsets[6] = (int)(curOffset - start);

                //ColorChannelIO.Write(writer, m_ChannelControlBlock);
                foreach (Material.ChannelControl chan in m_ChannelControlBlock)
                    chan.Write(writer);

                writer.WritePadding(4, Padding);

                curOffset = writer.Position;

                // ambient color data offset
                Offsets[7] = (int)(curOffset - start);

                //ColorIO.Write(writer, m_AmbientColorBlock);
                for (int i = 0; i < m_AmbientColorBlock.Count; i++)
                {
                    writer.WriteByte((byte)(m_AmbientColorBlock[i].R * 255));
                    writer.WriteByte((byte)(m_AmbientColorBlock[i].G * 255));
                    writer.WriteByte((byte)(m_AmbientColorBlock[i].B * 255));
                    writer.WriteByte((byte)(m_AmbientColorBlock[i].A * 255));
                }

                curOffset = writer.Position;

                // light color data offset
                Offsets[8] = (int)(curOffset - start);

                if (m_LightingColorBlock != null)
                {
                    //ColorIO.Write(writer, m_LightingColorBlock);
                    for (int i = 0; i < m_LightingColorBlock.Count; i++)
                    {
                        writer.WriteByte((byte)(m_LightingColorBlock[i].R * 255));
                        writer.WriteByte((byte)(m_LightingColorBlock[i].G * 255));
                        writer.WriteByte((byte)(m_LightingColorBlock[i].B * 255));
                        writer.WriteByte((byte)(m_LightingColorBlock[i].A * 255));
                    }
                }

                curOffset = writer.Position;

                // tex gen count data offset
                Offsets[9] = (int)(curOffset - start);

                foreach (byte texGenCnt in NumTexGensBlock)
                    writer.WriteByte(texGenCnt);

                writer.WritePadding(4, Padding);

                curOffset = writer.Position;

                // tex coord 1 data offset
                Offsets[10] = (int)(curOffset - start);

                //TexCoordGenIO.Write(writer, m_TexCoord1GenBlock);
                foreach (Material.TexCoordGen gen in m_TexCoord1GenBlock)
                    gen.Write(writer);


                curOffset = writer.Position;

                // tex coord 2 data offset AKA PostTexGenInfoOffset
                Offsets[11] = (m_TexCoord2GenBlock == null || m_TexCoord2GenBlock.Count == 0) ? 0 : (int)(curOffset - start);

                //TexCoordGenIO.Write(writer, m_TexCoord2GenBlock);
                if (m_TexCoord2GenBlock != null && m_TexCoord2GenBlock.Count != 0)
                {
                    foreach (Material.TexCoordGen gen in m_TexCoord2GenBlock)
                        gen.Write(writer);
                }

                curOffset = writer.Position;

                // tex matrix 1 data offset
                Offsets[12] = (int)(curOffset - start);

                //TexMatrixIO.Write(writer, m_TexMatrix1Block);
                foreach (Material.TexMatrix mat in m_TexMatrix1Block)
                    mat.Write(writer);


                curOffset = writer.Position;

                // tex matrix 2 data offset
                Offsets[13] = (m_TexMatrix2Block == null || m_TexMatrix2Block.Count == 0) ? 0 : (int)(curOffset - start);

                //TexMatrixIO.Write(writer, m_TexMatrix2Block);
                if (m_TexMatrix2Block != null && m_TexMatrix2Block.Count != 0)
                {
                    foreach (Material.TexMatrix gen in m_TexMatrix2Block)
                        gen.Write(writer);
                }


                curOffset = writer.Position;

                // tex number data offset
                Offsets[14] = (int)(curOffset - start);

                foreach (int inte in m_TexRemapBlock)
                    writer.WriteBigEndian(BitConverter.GetBytes((short)inte), 0, 2);

                writer.WritePadding(4, Padding);

                curOffset = writer.Position;

                // tev order data offset
                Offsets[15] = (int)(curOffset - start);

                //TevOrderIO.Write(writer, m_TevOrderBlock);
                foreach (Material.TevOrder order in m_TevOrderBlock)
                    order.Write(writer);

                curOffset = writer.Position;

                // tev color data offset
                Offsets[16] = (int)(curOffset - start);

                //Int16ColorIO.Write(writer, m_TevColorBlock);
                for (int i = 0; i < m_TevColorBlock.Count; i++)
                {
                    writer.WriteBigEndian(BitConverter.GetBytes((short)(m_TevColorBlock[i].R * 255)), 0, 2);
                    writer.WriteBigEndian(BitConverter.GetBytes((short)(m_TevColorBlock[i].G * 255)), 0, 2);
                    writer.WriteBigEndian(BitConverter.GetBytes((short)(m_TevColorBlock[i].B * 255)), 0, 2);
                    writer.WriteBigEndian(BitConverter.GetBytes((short)(m_TevColorBlock[i].A * 255)), 0, 2);
                }

                curOffset = writer.Position;

                // tev konst color data offset
                Offsets[17] = (int)(curOffset - start);

                //ColorIO.Write(writer, m_TevKonstColorBlock);
                for (int i = 0; i < m_TevKonstColorBlock.Count; i++)
                {
                    writer.WriteByte((byte)(m_TevKonstColorBlock[i].R * 255));
                    writer.WriteByte((byte)(m_TevKonstColorBlock[i].G * 255));
                    writer.WriteByte((byte)(m_TevKonstColorBlock[i].B * 255));
                    writer.WriteByte((byte)(m_TevKonstColorBlock[i].A * 255));
                }

                curOffset = writer.Position;

                // tev stage count data offset
                Offsets[18] = (int)(curOffset - start);

                foreach (byte bt in NumTevStagesBlock)
                    writer.WriteByte(bt);

                writer.WritePadding(4, Padding);

                curOffset = writer.Position;

                // tev stage data offset
                Offsets[19] = (int)(curOffset - start);

                //TevStageIO.Write(writer, m_TevStageBlock);
                foreach (Material.TevStage stage in m_TevStageBlock)
                    stage.Write(writer);

                curOffset = writer.Position;

                // tev swap mode offset
                Offsets[20] = (int)(curOffset - start);

                //TevSwapModeIO.Write(writer, m_SwapModeBlock);
                foreach (Material.TevSwapMode mode in m_SwapModeBlock)
                    mode.Write(writer);

                curOffset = writer.Position;

                // tev swap mode table offset
                Offsets[21] = (int)(curOffset - start);

                //TevSwapModeTableIO.Write(writer, m_SwapTableBlock);
                foreach (Material.TevSwapModeTable table in m_SwapTableBlock)
                    table.Write(writer);

                curOffset = writer.Position;

                // fog data offset
                Offsets[22] = (int)(curOffset - start);

                //FogIO.Write(writer, m_FogBlock);
                foreach (Material.Fog fog in m_FogBlock)
                    fog.Write(writer);

                curOffset = writer.Position;

                // alpha compare offset
                Offsets[23] = (int)(curOffset - start);

                //AlphaCompareIO.Write(writer, m_AlphaCompBlock);
                foreach (Material.AlphaCompare comp in m_AlphaCompBlock)
                    comp.Write(writer);

                curOffset = writer.Position;

                // blend data offset
                Offsets[24] = (int)(curOffset - start);

                //BlendModeIO.Write(writer, m_blendModeBlock);
                foreach (Material.BlendMode mode in m_blendModeBlock)
                    mode.Write(writer);

                curOffset = writer.Position;

                // zmode data offset
                Offsets[25] = (int)(curOffset - start);

                //ZModeIO.Write(writer, m_zModeBlock);
                foreach (Material.ZModeHolder mode in m_zModeBlock)
                    mode.Write(writer);

                curOffset = writer.Position;

                // z comp loc data offset
                Offsets[26] = (int)(curOffset - start);

                foreach (bool bol in m_zCompLocBlock)
                    writer.WriteByte((byte)(bol ? 0x01 : 0x00));

                writer.WritePadding(4, Padding);

                curOffset = writer.Position;

                //Dither Block
                if (m_ditherBlock != null && m_ditherBlock.Count != 0)
                {
                    // dither data offset
                    Offsets[27] = (int)(curOffset - start);

                    foreach (bool bol in m_ditherBlock)
                        writer.WriteByte((byte)(bol ? 0x01 : 0x00));

                    writer.WritePadding(4, Padding);
                }

                curOffset = writer.Position;

                // NBT Scale data offset
                Offsets[28] = (int)(curOffset - start);

                //NBTScaleIO.Write(writer, m_NBTScaleBlock);
                foreach (Material.NBTScaleHolder scale in m_NBTScaleBlock)
                    scale.Write(writer);

                writer.WritePadding(32, Padding);

                writer.Position = start + 4;
                writer.WriteBigEndian(BitConverter.GetBytes((int)(writer.Length - start)), 0, 4);
                writer.Position += 0x08;
                for (int i = 0; i < 29; i++)
                    writer.WriteBigEndian(BitConverter.GetBytes(Offsets[i]), 0, 4);
                writer.Position = writer.Length;
            }
            private void WriteMaterialInitData(Stream writer, Material mat, ref List<CullMode> m_CullModeBlock,
            ref List<Color4> m_MaterialColorBlock, ref List<Material.ChannelControl> m_ChannelControlBlock, ref List<Color4> m_AmbientColorBlock, ref List<Color4> m_LightingColorBlock,
            ref List<Material.TexCoordGen> m_TexCoord1GenBlock, ref List<Material.TexCoordGen> m_TexCoord2GenBlock, ref List<Material.TexMatrix> m_TexMatrix1Block, ref List<Material.TexMatrix> m_TexMatrix2Block,
            ref List<short> m_TexRemapBlock, ref List<Material.TevOrder> m_TevOrderBlock, ref List<Color4> m_TevColorBlock, ref List<Color4> m_TevKonstColorBlock, ref List<Material.TevStage> m_TevStageBlock,
            ref List<Material.TevSwapMode> m_SwapModeBlock, ref List<Material.TevSwapModeTable> m_SwapTableBlock, ref List<Material.Fog> m_FogBlock, ref List<Material.AlphaCompare> m_AlphaCompBlock,
            ref List<Material.BlendMode> m_blendModeBlock, ref List<Material.NBTScaleHolder> m_NBTScaleBlock, ref List<Material.ZModeHolder> m_zModeBlock, ref List<bool> m_zCompLocBlock,
            ref List<bool> m_ditherBlock, ref List<byte> NumColorChannelsBlock, ref List<byte> NumTexGensBlock, ref List<byte> NumTevStagesBlock)
            {
                writer.WriteByte(mat.Flag);

                if (!m_CullModeBlock.Any(CM => CM == mat.CullMode))
                    m_CullModeBlock.Add(mat.CullMode);
                writer.WriteByte((byte)m_CullModeBlock.IndexOf(mat.CullMode));

                if (!NumColorChannelsBlock.Any(NCC => NCC == mat.LightChannelCount))
                    NumColorChannelsBlock.Add(mat.LightChannelCount);
                writer.WriteByte((byte)NumColorChannelsBlock.IndexOf(mat.LightChannelCount));

                if (!NumTexGensBlock.Any(NTG => NTG == mat.NumTexGensCount))
                    NumTexGensBlock.Add(mat.NumTexGensCount);
                writer.WriteByte((byte)NumTexGensBlock.IndexOf(mat.NumTexGensCount));

                if (!NumTevStagesBlock.Any(NTS => NTS == mat.NumTevStagesCount))
                    NumTevStagesBlock.Add(mat.NumTevStagesCount);
                writer.WriteByte((byte)NumTevStagesBlock.IndexOf(mat.NumTevStagesCount));

                if (!m_zCompLocBlock.Any(ZCL => ZCL == mat.ZCompLoc))
                    m_zCompLocBlock.Add(mat.ZCompLoc);
                writer.WriteByte((byte)m_zCompLocBlock.IndexOf(mat.ZCompLoc));

                if (!m_zModeBlock.Any(ZM => ZM == mat.ZMode))
                    m_zModeBlock.Add(mat.ZMode);
                writer.WriteByte((byte)m_zModeBlock.IndexOf(mat.ZMode));

                if (!m_ditherBlock.Any(Ditherer => Ditherer == mat.Dither))
                    m_ditherBlock.Add(mat.Dither);
                writer.WriteByte((byte)m_ditherBlock.IndexOf(mat.Dither));

                if (mat.MaterialColors[0].HasValue)
                {
                    if (!m_MaterialColorBlock.Any(MatCol => MatCol == mat.MaterialColors[0].Value))
                        m_MaterialColorBlock.Add(mat.MaterialColors[0].Value);
                    writer.WriteBigEndian(BitConverter.GetBytes((short)m_MaterialColorBlock.IndexOf(mat.MaterialColors[0].Value)), 0, 2);
                }
                else
                    writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);

                if (mat.MaterialColors[1].HasValue)
                {
                    if (!m_MaterialColorBlock.Any(MatCol => MatCol == mat.MaterialColors[1].Value))
                        m_MaterialColorBlock.Add(mat.MaterialColors[1].Value);
                    writer.WriteBigEndian(BitConverter.GetBytes((short)m_MaterialColorBlock.IndexOf(mat.MaterialColors[1].Value)), 0, 2);
                }
                else
                    writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);

                for (int i = 0; i < 4; i++)
                {
                    if (mat.ChannelControls[i] != null)
                    {
                        if (!m_ChannelControlBlock.Any(ChanCol => ChanCol == mat.ChannelControls[i].Value))
                            m_ChannelControlBlock.Add(mat.ChannelControls[i].Value);
                        writer.WriteBigEndian(BitConverter.GetBytes((short)m_ChannelControlBlock.IndexOf(mat.ChannelControls[i].Value)), 0, 2);
                    }
                    else
                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                }

                if (mat.AmbientColors[0].HasValue)
                {
                    if (!m_AmbientColorBlock.Any(AmbCol => AmbCol == mat.AmbientColors[0].Value))
                        m_AmbientColorBlock.Add(mat.AmbientColors[0].Value);
                    writer.WriteBigEndian(BitConverter.GetBytes((short)m_AmbientColorBlock.IndexOf(mat.AmbientColors[0].Value)), 0, 2);
                }
                else
                    writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);

                if (mat.AmbientColors[1].HasValue)
                {
                    if (!m_AmbientColorBlock.Any(AmbCol => AmbCol == mat.AmbientColors[1].Value))
                        m_AmbientColorBlock.Add(mat.AmbientColors[1].Value);
                    writer.WriteBigEndian(BitConverter.GetBytes((short)m_AmbientColorBlock.IndexOf(mat.AmbientColors[1].Value)), 0, 2);
                }
                else
                    writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);

                for (int i = 0; i < 8; i++)
                {
                    if (mat.LightingColors[i] != null)
                    {
                        if (!m_LightingColorBlock.Any(LightCol => LightCol == mat.LightingColors[i].Value))
                            m_LightingColorBlock.Add(mat.LightingColors[i].Value);
                        writer.WriteBigEndian(BitConverter.GetBytes((short)m_LightingColorBlock.IndexOf(mat.LightingColors[i].Value)), 0, 2);
                    }
                    else
                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                }

                for (int i = 0; i < 8; i++)
                {
                    if (mat.TexCoord1Gens[i] != null)
                    {
                        if (!m_TexCoord1GenBlock.Any(TexCoord => TexCoord == mat.TexCoord1Gens[i].Value))
                            m_TexCoord1GenBlock.Add(mat.TexCoord1Gens[i].Value);
                        writer.WriteBigEndian(BitConverter.GetBytes((short)m_TexCoord1GenBlock.IndexOf(mat.TexCoord1Gens[i].Value)), 0, 2);
                    }
                    else
                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                }

                for (int i = 0; i < 8; i++)
                {
                    if (mat.PostTexCoordGens[i] != null)
                    {
                        if (!m_TexCoord2GenBlock.Any(PostTexCoord => PostTexCoord == mat.PostTexCoordGens[i].Value))
                            m_TexCoord2GenBlock.Add(mat.PostTexCoordGens[i].Value);
                        writer.WriteBigEndian(BitConverter.GetBytes((short)m_TexCoord2GenBlock.IndexOf(mat.PostTexCoordGens[i].Value)), 0, 2);
                    }
                    else
                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                }

                for (int i = 0; i < 10; i++)
                {
                    if (mat.TexMatrix1[i] != null)
                    {
                        if (!m_TexMatrix1Block.Any(TexMat => TexMat == mat.TexMatrix1[i].Value))
                            m_TexMatrix1Block.Add(mat.TexMatrix1[i].Value);
                        writer.WriteBigEndian(BitConverter.GetBytes((short)m_TexMatrix1Block.IndexOf(mat.TexMatrix1[i].Value)), 0, 2);
                    }
                    else
                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                }

                for (int i = 0; i < 20; i++)
                {
                    if (mat.PostTexMatrix[i] != null)
                    {
                        if (!m_TexMatrix2Block.Any(PostTexMat => PostTexMat == mat.PostTexMatrix[i].Value))
                            m_TexMatrix2Block.Add(mat.PostTexMatrix[i].Value);
                        writer.WriteBigEndian(BitConverter.GetBytes((short)m_TexMatrix2Block.IndexOf(mat.PostTexMatrix[i].Value)), 0, 2);
                    }
                    else
                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                }

                for (int i = 0; i < 8; i++)
                {
                    if (mat.TextureIndices[i] != -1)
                    {
                        if (!m_TexRemapBlock.Any(TexId => TexId == (short)mat.TextureIndices[i]))
                            m_TexRemapBlock.Add((short)mat.TextureIndices[i]);
                        writer.WriteBigEndian(BitConverter.GetBytes((short)m_TexRemapBlock.IndexOf((short)mat.TextureIndices[i])), 0, 2);
                    }
                    else
                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                }

                for (int i = 0; i < 4; i++)
                {
                    if (mat.KonstColors[i] != null)
                    {
                        if (!m_TevKonstColorBlock.Any(KCol => KCol == mat.KonstColors[i].Value))
                            m_TevKonstColorBlock.Add(mat.KonstColors[i].Value);
                        writer.WriteBigEndian(BitConverter.GetBytes((short)m_TevKonstColorBlock.IndexOf(mat.KonstColors[i].Value)), 0, 2);
                    }
                    else
                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                }

                for (int i = 0; i < 16; i++)
                    writer.WriteByte((byte)mat.ColorSels[i]);

                for (int i = 0; i < 16; i++)
                    writer.WriteByte((byte)mat.AlphaSels[i]);

                for (int i = 0; i < 16; i++)
                {
                    if (mat.TevOrders[i] != null)
                    {
                        if (!m_TevOrderBlock.Any(TevOrder => TevOrder == mat.TevOrders[i].Value))
                            m_TevOrderBlock.Add(mat.TevOrders[i].Value);
                        writer.WriteBigEndian(BitConverter.GetBytes((short)m_TevOrderBlock.IndexOf(mat.TevOrders[i].Value)), 0, 2);
                    }
                    else
                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                }

                for (int i = 0; i < 4; i++)
                {
                    if (mat.TevColors[i] != null)
                    {
                        if (!m_TevColorBlock.Any(TevCol => TevCol == mat.TevColors[i].Value))
                            m_TevColorBlock.Add(mat.TevColors[i].Value);
                        writer.WriteBigEndian(BitConverter.GetBytes((short)m_TevColorBlock.IndexOf(mat.TevColors[i].Value)), 0, 2);
                    }
                    else
                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                }

                for (int i = 0; i < 16; i++)
                {
                    if (mat.TevStages[i] != null)
                    {
                        if (!m_TevStageBlock.Any(TevStg => TevStg == mat.TevStages[i].Value))
                            m_TevStageBlock.Add(mat.TevStages[i].Value);
                        writer.WriteBigEndian(BitConverter.GetBytes((short)m_TevStageBlock.IndexOf(mat.TevStages[i].Value)), 0, 2);
                    }
                    else
                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                }

                for (int i = 0; i < 16; i++)
                {
                    if (mat.SwapModes[i] != null)
                    {
                        if (!m_SwapModeBlock.Any(SwapMode => SwapMode == mat.SwapModes[i].Value))
                            m_SwapModeBlock.Add(mat.SwapModes[i].Value);
                        writer.WriteBigEndian(BitConverter.GetBytes((short)m_SwapModeBlock.IndexOf(mat.SwapModes[i].Value)), 0, 2);
                    }
                    else
                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                }

                for (int i = 0; i < 16; i++)
                {
                    if (mat.SwapTables[i] != null)
                    {
                        if (!m_SwapTableBlock.Any(SwapTable => SwapTable == mat.SwapTables[i].Value))
                            m_SwapTableBlock.Add(mat.SwapTables[i].Value);
                        writer.WriteBigEndian(BitConverter.GetBytes((short)m_SwapTableBlock.IndexOf(mat.SwapTables[i].Value)), 0, 2);
                    }
                    else
                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                }

                if (!m_FogBlock.Any(Fog => Fog == mat.FogInfo))
                    m_FogBlock.Add(mat.FogInfo);
                writer.WriteBigEndian(BitConverter.GetBytes((short)m_FogBlock.IndexOf(mat.FogInfo)), 0, 2);

                if (!m_AlphaCompBlock.Any(AlphaComp => AlphaComp == mat.AlphCompare))
                    m_AlphaCompBlock.Add(mat.AlphCompare);
                writer.WriteBigEndian(BitConverter.GetBytes((short)m_AlphaCompBlock.IndexOf(mat.AlphCompare)), 0, 2);

                if (!m_blendModeBlock.Any(Blend => Blend == mat.BMode))
                    m_blendModeBlock.Add(mat.BMode);
                writer.WriteBigEndian(BitConverter.GetBytes((short)m_blendModeBlock.IndexOf(mat.BMode)), 0, 2);

                if (!m_NBTScaleBlock.Any(NBT => NBT == mat.NBTScale))
                    m_NBTScaleBlock.Add(mat.NBTScale);
                writer.WriteBigEndian(BitConverter.GetBytes((short)m_NBTScaleBlock.IndexOf(mat.NBTScale)), 0, 2);
            }
            #endregion

            public class Material
            {
                public string Name;
                public byte Flag;
                public bool IsTranslucent => (Flag & 3) == 0;
                public byte NumTexGensCount
                {
                    get
                    {
                        byte value = 0;
                        for (int i = 0; i < TexMatrix1.Length; i++)
                        {
                            if (TexMatrix1[i].HasValue)
                                value++;
                        }
                        return value;
                    }
                }
                public byte NumTevStagesCount
                {
                    get
                    {
                        byte value = 0;
                        for (int i = 0; i < TevStages.Length; i++)
                        {
                            if (TevStages[i].HasValue)
                                value++;
                        }
                        return value;
                    }
                }

                public CullMode CullMode;
                public byte LightChannelCount;
                public bool ZCompLoc;
                public bool Dither;

                /// <summary>
                /// Only used during Read/write
                /// </summary>
                internal int[] TextureIndices;
                //public string[] TextureNames;
                /// <summary>
                /// Holds references to the Textures that this material uses
                /// </summary>
                public BTI[] Textures;

                public IndirectTexturing IndTexEntry;
                public Color4?[] MaterialColors;
                public ChannelControl?[] ChannelControls;
                public Color4?[] AmbientColors;
                public Color4?[] LightingColors;
                public TexCoordGen?[] TexCoord1Gens;
                public TexCoordGen?[] PostTexCoordGens;
                public TexMatrix?[] TexMatrix1;
                public TexMatrix?[] PostTexMatrix;
                public TevOrder?[] TevOrders;
                public KonstColorSel[] ColorSels;
                public KonstAlphaSel[] AlphaSels;
                public Color4?[] TevColors;
                public Color4?[] KonstColors;
                public TevStage?[] TevStages; //TODO: Change this to a list
                public TevSwapMode?[] SwapModes;
                public TevSwapModeTable?[] SwapTables;

                public Fog FogInfo;
                public AlphaCompare AlphCompare;
                public BlendMode BMode;
                public ZModeHolder ZMode;
                public NBTScaleHolder NBTScale;

                public Material()
                {
                    CullMode = CullMode.Back;
                    LightChannelCount = 1;
                    MaterialColors = new Color4?[2] { new Color4(1, 1, 1, 1), null };

                    ChannelControls = new ChannelControl?[4];

                    IndTexEntry = new IndirectTexturing();

                    AmbientColors = new Color4?[2] { new Color4(50f / 255f, 50f / 255f, 50f / 255f, 50f / 255f), null };
                    LightingColors = new Color4?[8];

                    TexCoord1Gens = new TexCoordGen?[8];
                    PostTexCoordGens = new TexCoordGen?[8];

                    TexMatrix1 = new TexMatrix?[10];
                    PostTexMatrix = new TexMatrix?[20];

                    TextureIndices = new int[8] { -1, -1, -1, -1, -1, -1, -1, -1 };
                    //TextureNames = new string[8] { "", "", "", "", "", "", "", "" };
                    Textures = new BTI[8];

                    KonstColors = new Color4?[4];
                    KonstColors[0] = new Color4(1, 1, 1, 1);

                    ColorSels = new KonstColorSel[16];
                    AlphaSels = new KonstAlphaSel[16];

                    TevOrders = new TevOrder?[16];
                    //TevOrders[0] = new TevOrder(TexCoordId.TexCoord0, TexMapId.TexMap0, GXColorChannelId.Color0);

                    TevColors = new Color4?[4];
                    TevColors[0] = new Color4(1, 1, 1, 1);

                    TevStages = new TevStage?[16];

                    SwapModes = new TevSwapMode?[16];
                    SwapModes[0] = new TevSwapMode(0, 0);

                    SwapTables = new TevSwapModeTable?[16];
                    SwapTables[0] = new TevSwapModeTable(0, 1, 2, 3);

                    AlphCompare = new AlphaCompare(AlphaCompare.GxCompareType.Greater, 127, AlphaCompare.GXAlphaOp.And, AlphaCompare.GxCompareType.Always, 0);
                    ZMode = new ZModeHolder(true, AlphaCompare.GxCompareType.LEqual, true);
                    BMode = new BlendMode(BlendMode.BlendModeID.Blend, BlendMode.BlendModeControl.SrcAlpha, BlendMode.BlendModeControl.InverseSrcAlpha, BlendMode.LogicOp.NoOp);
                    NBTScale = new NBTScaleHolder(0, Vector3.Zero);
                    FogInfo = new Fog(0, false, 0, 0, 0, 0, 0, new Color4(0, 0, 0, 0), new float[10]);
                }

                public Material(Material src)
                {
                    Flag = src.Flag;
                    CullMode = src.CullMode;
                    LightChannelCount = src.LightChannelCount;
                    ZCompLoc = src.ZCompLoc;
                    Dither = src.Dither;
                    TextureIndices = src.TextureIndices;
                    //TextureNames = src.TextureNames;
                    Textures = src.Textures;
                    IndTexEntry = src.IndTexEntry;
                    MaterialColors = src.MaterialColors;
                    ChannelControls = src.ChannelControls;
                    AmbientColors = src.AmbientColors;
                    LightingColors = src.LightingColors;
                    TexCoord1Gens = src.TexCoord1Gens;
                    PostTexCoordGens = src.PostTexCoordGens;
                    TexMatrix1 = src.TexMatrix1;
                    PostTexMatrix = src.PostTexMatrix;
                    TevOrders = src.TevOrders;
                    ColorSels = src.ColorSels;
                    AlphaSels = src.AlphaSels;
                    TevColors = src.TevColors;
                    KonstColors = src.KonstColors;
                    TevStages = src.TevStages;
                    SwapModes = src.SwapModes;
                    SwapTables = src.SwapTables;

                    FogInfo = src.FogInfo;
                    AlphCompare = src.AlphCompare;
                    BMode = src.BMode;
                    ZMode = src.ZMode;
                    NBTScale = src.NBTScale;
                }

                public void AddChannelControl(TevOrder.GXColorChannelId id, bool enable, ChannelControl.ColorSrc MatSrcColor, ChannelControl.LightId litId, ChannelControl.DiffuseFn diffuse, ChannelControl.J3DAttenuationFn atten, ChannelControl.ColorSrc ambSrcColor)
                {
                    ChannelControl control = new ChannelControl
                    {
                        Enable = enable,
                        MaterialSrcColor = MatSrcColor,
                        LitMask = litId,
                        DiffuseFunction = diffuse,
                        AttenuationFunction = atten,
                        AmbientSrcColor = ambSrcColor
                    };

                    ChannelControls[(int)id] = control;
                }

                public void AddTexMatrix(TexGenType projection, byte type, Vector3 effectTranslation, Vector2 scale, float rotation, Vector2 translation, Matrix4 matrix)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (TexMatrix1[i] == null)
                        {
                            TexMatrix1[i] = new TexMatrix(projection, type, effectTranslation, scale, rotation, translation, matrix);
                            break;
                        }

                        if (i == 9)
                            throw new Exception($"TexMatrix1 array for material \"{Name}\" is full!");
                    }
                }

                public void AddTexIndex(int index)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (TextureIndices[i] == -1)
                        {
                            TextureIndices[i] = index;
                            break;
                        }

                        if (i == 7)
                            throw new Exception($"TextureIndex array for material \"{Name}\" is full!");
                    }
                }

                public void AddTevOrder(TexCoordId coordId, TexMapId mapId, TevOrder.GXColorChannelId colorChanId)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (TevOrders[i] == null)
                        {
                            TevOrders[i] = new TevOrder(coordId, mapId, colorChanId);
                            break;
                        }

                        if (i == 7)
                            throw new Exception($"TevOrder array for material \"{Name}\" is full!");
                    }
                }

                public Material Clone()
                {
                    Material Target = new Material()
                    {
                        Name = Name,
                        Flag = Flag,
                        CullMode = CullMode,
                        LightChannelCount = LightChannelCount,
                        ZCompLoc = ZCompLoc,
                        Dither = Dither,
                        IndTexEntry = IndTexEntry.Clone(),

                        MaterialColors = new Color4?[MaterialColors.Length],
                        ChannelControls = new ChannelControl?[ChannelControls.Length],
                        AmbientColors = new Color4?[AmbientColors.Length],
                        LightingColors = new Color4?[LightingColors.Length],
                        TexCoord1Gens = new TexCoordGen?[TexCoord1Gens.Length],
                        PostTexCoordGens = new TexCoordGen?[PostTexCoordGens.Length],
                        TexMatrix1 = new TexMatrix?[TexMatrix1.Length],
                        PostTexMatrix = new TexMatrix?[PostTexMatrix.Length],
                        TevOrders = new TevOrder?[TevOrders.Length],
                        ColorSels = new KonstColorSel[ColorSels.Length],
                        AlphaSels = new KonstAlphaSel[AlphaSels.Length],
                        TevColors = new Color4?[TevColors.Length],
                        KonstColors = new Color4?[KonstColors.Length],
                        TevStages = new TevStage?[TevStages.Length],
                        SwapModes = new TevSwapMode?[SwapModes.Length],
                        SwapTables = new TevSwapModeTable?[SwapTables.Length],

                        FogInfo = FogInfo.Clone(),
                        AlphCompare = AlphCompare.Clone(),
                        BMode = BMode.Clone(),
                        ZMode = ZMode.Clone(),
                        NBTScale = NBTScale.Clone()
                    };

                    Target.TextureIndices = new int[8];
                    //Target.TextureNames = new string[8];
                    Target.Textures = new BTI[8];
                    for (int i = 0; i < 8; i++)
                    {
                        Target.TextureIndices[i] = TextureIndices[i];
                        //Target.TextureNames[i] = TextureNames[i];
                        Target.Textures[i] = Textures[i];
                    }
                    for (int i = 0; i < MaterialColors.Length; i++)
                    {
                        if (MaterialColors[i].HasValue)
                            Target.MaterialColors[i] = new Color4(MaterialColors[i].Value.R, MaterialColors[i].Value.G, MaterialColors[i].Value.B, MaterialColors[i].Value.A);
                    }
                    for (int i = 0; i < ChannelControls.Length; i++)
                    {
                        if (ChannelControls[i].HasValue)
                            Target.ChannelControls[i] = ChannelControls[i].Value.Clone();
                    }
                    for (int i = 0; i < AmbientColors.Length; i++)
                    {
                        if (AmbientColors[i].HasValue)
                            Target.AmbientColors[i] = new Color4(AmbientColors[i].Value.R, AmbientColors[i].Value.G, AmbientColors[i].Value.B, AmbientColors[i].Value.A);
                    }
                    for (int i = 0; i < LightingColors.Length; i++)
                    {
                        if (LightingColors[i].HasValue)
                            Target.LightingColors[i] = new Color4(LightingColors[i].Value.R, LightingColors[i].Value.G, LightingColors[i].Value.B, LightingColors[i].Value.A);
                    }
                    for (int i = 0; i < TexCoord1Gens.Length; i++)
                    {
                        if (TexCoord1Gens[i].HasValue)
                            Target.TexCoord1Gens[i] = TexCoord1Gens[i].Value.Clone();
                    }
                    for (int i = 0; i < PostTexCoordGens.Length; i++)
                    {
                        if (PostTexCoordGens[i].HasValue)
                            Target.PostTexCoordGens[i] = PostTexCoordGens[i].Value.Clone();
                    }
                    for (int i = 0; i < TexMatrix1.Length; i++)
                    {
                        if (TexMatrix1[i].HasValue)
                            Target.TexMatrix1[i] = TexMatrix1[i].Value.Clone();
                    }
                    for (int i = 0; i < PostTexMatrix.Length; i++)
                    {
                        if (PostTexMatrix[i].HasValue)
                            Target.PostTexMatrix[i] = PostTexMatrix[i].Value.Clone();
                    }
                    for (int i = 0; i < TevOrders.Length; i++)
                    {
                        if (TevOrders[i].HasValue)
                            Target.TevOrders[i] = TevOrders[i].Value.Clone();
                    }
                    for (int i = 0; i < ColorSels.Length; i++)
                        Target.ColorSels[i] = ColorSels[i];
                    for (int i = 0; i < AlphaSels.Length; i++)
                        Target.AlphaSels[i] = AlphaSels[i];
                    for (int i = 0; i < TevColors.Length; i++)
                    {
                        if (TevColors[i].HasValue)
                            Target.TevColors[i] = new Color4(TevColors[i].Value.R, TevColors[i].Value.G, TevColors[i].Value.B, TevColors[i].Value.A);
                    }
                    for (int i = 0; i < KonstColors.Length; i++)
                    {
                        if (KonstColors[i].HasValue)
                            Target.KonstColors[i] = new Color4(KonstColors[i].Value.R, KonstColors[i].Value.G, KonstColors[i].Value.B, KonstColors[i].Value.A);
                    }
                    for (int i = 0; i < TevStages.Length; i++)
                    {
                        if (TevStages[i].HasValue)
                            Target.TevStages[i] = TevStages[i].Value.Clone();
                    }
                    for (int i = 0; i < SwapModes.Length; i++)
                    {
                        if (SwapModes[i].HasValue)
                            Target.SwapModes[i] = SwapModes[i].Value.Clone();
                    }
                    for (int i = 0; i < SwapTables.Length; i++)
                    {
                        if (SwapTables[i].HasValue)
                            Target.SwapTables[i] = SwapTables[i].Value.Clone();
                    }

                    return Target;
                }

                public override string ToString()
                {
                    return $"{Name}";
                }

                public override bool Equals(object obj)
                {
                    if (!(obj is Material right))
                        return false;
                    if (Flag != right.Flag)
                        return false;
                    if (CullMode != right.CullMode)
                        return false;
                    if (LightChannelCount != right.LightChannelCount)
                        return false;
                    if (NumTexGensCount != right.NumTexGensCount)
                        return false;
                    if (NumTevStagesCount != right.NumTevStagesCount)
                        return false;
                    if (ZCompLoc != right.ZCompLoc)
                        return false;
                    if (ZMode != right.ZMode)
                        return false;
                    if (Dither != right.Dither)
                        return false;

                    for (int i = 0; i < 2; i++)
                    {
                        if (MaterialColors[i] != right.MaterialColors[i])
                            return false;
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        if (ChannelControls[i] != right.ChannelControls[i])
                            return false;
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        if (AmbientColors[i] != right.AmbientColors[i])
                            return false;
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        if (LightingColors[i] != right.LightingColors[i])
                            return false;
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        if (TexCoord1Gens[i] != right.TexCoord1Gens[i]) // TODO: does != actually work on these types of things?? might need custom operators
                            return false;
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        if (PostTexCoordGens[i] != right.PostTexCoordGens[i])
                            return false;
                    }
                    for (int i = 0; i < 10; i++)
                    {
                        if (TexMatrix1[i] != right.TexMatrix1[i])
                            return false;
                    }
                    for (int i = 0; i < 20; i++)
                    {
                        if (PostTexMatrix[i] != right.PostTexMatrix[i])
                            return false;
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        //if (TextureNames[i] != right.TextureNames[i])
                        if (Textures[i]?.ImageEquals(right.Textures[i]) ?? true)
                            return false;
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        if (KonstColors[i] != right.KonstColors[i])
                            return false;
                    }
                    for (int i = 0; i < 16; i++)
                    {
                        if (ColorSels[i] != right.ColorSels[i])
                            return false;
                    }
                    for (int i = 0; i < 16; i++)
                    {
                        if (AlphaSels[i] != right.AlphaSels[i])
                            return false;
                    }
                    for (int i = 0; i < 16; i++)
                    {
                        if (TevOrders[i] != right.TevOrders[i])
                            return false;
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        if (TevColors[i] != right.TevColors[i])
                            return false;
                    }
                    for (int i = 0; i < 16; i++)
                    {
                        if (TevStages[i] != right.TevStages[i])
                            return false;
                    }
                    for (int i = 0; i < 16; i++)
                    {
                        if (SwapModes[i] != right.SwapModes[i])
                            return false;
                    }
                    for (int i = 0; i < 16; i++)
                    {
                        if (SwapTables[i] != right.SwapTables[i])
                            return false;
                    }

                    if (FogInfo != right.FogInfo)
                        return false;
                    if (AlphCompare != right.AlphCompare)
                        return false;
                    if (BMode != right.BMode)
                        return false;
                    if (NBTScale != right.NBTScale)
                        return false;

                    return true;
                }

                public override int GetHashCode()
                {
                    var hashCode = 1712440529;
                    hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
                    hashCode = hashCode * -1521134295 + Flag.GetHashCode();
                    hashCode = hashCode * -1521134295 + LightChannelCount.GetHashCode();
                    hashCode = hashCode * -1521134295 + NumTexGensCount.GetHashCode();
                    hashCode = hashCode * -1521134295 + NumTevStagesCount.GetHashCode();
                    hashCode = hashCode * -1521134295 + CullMode.GetHashCode();
                    hashCode = hashCode * -1521134295 + ZCompLoc.GetHashCode();
                    hashCode = hashCode * -1521134295 + Dither.GetHashCode();
                    hashCode = hashCode * -1521134295 + EqualityComparer<int[]>.Default.GetHashCode(TextureIndices);
                    //hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(TextureNames);
                    hashCode = hashCode * -1521134295 + EqualityComparer<IndirectTexturing>.Default.GetHashCode(IndTexEntry);
                    hashCode = hashCode * -1521134295 + EqualityComparer<Color4?[]>.Default.GetHashCode(MaterialColors);
                    hashCode = hashCode * -1521134295 + EqualityComparer<ChannelControl?[]>.Default.GetHashCode(ChannelControls);
                    hashCode = hashCode * -1521134295 + EqualityComparer<Color4?[]>.Default.GetHashCode(AmbientColors);
                    hashCode = hashCode * -1521134295 + EqualityComparer<Color4?[]>.Default.GetHashCode(LightingColors);
                    hashCode = hashCode * -1521134295 + EqualityComparer<TexCoordGen?[]>.Default.GetHashCode(TexCoord1Gens);
                    hashCode = hashCode * -1521134295 + EqualityComparer<TexCoordGen?[]>.Default.GetHashCode(PostTexCoordGens);
                    hashCode = hashCode * -1521134295 + EqualityComparer<TexMatrix?[]>.Default.GetHashCode(TexMatrix1);
                    hashCode = hashCode * -1521134295 + EqualityComparer<TexMatrix?[]>.Default.GetHashCode(PostTexMatrix);
                    hashCode = hashCode * -1521134295 + EqualityComparer<TevOrder?[]>.Default.GetHashCode(TevOrders);
                    hashCode = hashCode * -1521134295 + EqualityComparer<KonstColorSel[]>.Default.GetHashCode(ColorSels);
                    hashCode = hashCode * -1521134295 + EqualityComparer<KonstAlphaSel[]>.Default.GetHashCode(AlphaSels);
                    hashCode = hashCode * -1521134295 + EqualityComparer<Color4?[]>.Default.GetHashCode(TevColors);
                    hashCode = hashCode * -1521134295 + EqualityComparer<Color4?[]>.Default.GetHashCode(KonstColors);
                    hashCode = hashCode * -1521134295 + EqualityComparer<TevStage?[]>.Default.GetHashCode(TevStages);
                    hashCode = hashCode * -1521134295 + EqualityComparer<TevSwapMode?[]>.Default.GetHashCode(SwapModes);
                    hashCode = hashCode * -1521134295 + EqualityComparer<TevSwapModeTable?[]>.Default.GetHashCode(SwapTables);
                    hashCode = hashCode * -1521134295 + EqualityComparer<Fog>.Default.GetHashCode(FogInfo);
                    hashCode = hashCode * -1521134295 + EqualityComparer<AlphaCompare>.Default.GetHashCode(AlphCompare);
                    hashCode = hashCode * -1521134295 + EqualityComparer<BlendMode>.Default.GetHashCode(BMode);
                    hashCode = hashCode * -1521134295 + EqualityComparer<ZModeHolder>.Default.GetHashCode(ZMode);
                    hashCode = hashCode * -1521134295 + EqualityComparer<NBTScaleHolder>.Default.GetHashCode(NBTScale);
                    return hashCode;
                }

                public class IndirectTexturing
                {
                    /// <summary>
                    /// Determines if an indirect texture lookup is to take place
                    /// </summary>
                    public bool HasLookup;
                    /// <summary>
                    /// The number of indirect texturing stages to use
                    /// </summary>
                    public byte IndTexStageNum;

                    public IndirectTevOrder[] TevOrders;

                    /// <summary>
                    /// The dynamic 2x3 matrices to use when transforming the texture coordinates
                    /// </summary>
                    public IndirectTexMatrix[] Matrices;
                    /// <summary>
                    /// U and V scales to use when transforming the texture coordinates
                    /// </summary>
                    public IndirectTexScale[] Scales;
                    /// <summary>
                    /// Instructions for setting up the specified TEV stage for lookup operations
                    /// </summary>
                    public IndirectTevStage[] TevStages;

                    public IndirectTexturing()
                    {
                        HasLookup = false;
                        IndTexStageNum = 0;

                        TevOrders = new IndirectTevOrder[4];
                        for (int i = 0; i < 4; i++)
                            TevOrders[i] = new IndirectTevOrder(TexCoordId.Null, TexMapId.Null);

                        Matrices = new IndirectTexMatrix[3];
                        for (int i = 0; i < 3; i++)
                            Matrices[i] = new IndirectTexMatrix(new Matrix2x3(0.5f, 0.0f, 0.0f, 0.0f, 0.5f, 0.0f), 1);

                        Scales = new IndirectTexScale[4];
                        for (int i = 0; i < 4; i++)
                            Scales[i] = new IndirectTexScale(IndirectScale.ITS_1, IndirectScale.ITS_1);

                        TevStages = new IndirectTevStage[16];
                        for (int i = 0; i < 3; i++)
                            TevStages[i] = new IndirectTevStage(
                                TevStageId.TevStage0,
                                IndirectFormat.ITF_8,
                                IndirectBias.S,
                                IndirectMatrix.ITM_OFF,
                                IndirectWrap.ITW_OFF,
                                IndirectWrap.ITW_OFF,
                                false,
                                false,
                                IndirectAlpha.ITBA_OFF
                                );
                    }
                    public IndirectTexturing(Stream reader)
                    {
                        HasLookup = reader.ReadByte() > 0;
                        IndTexStageNum = (byte)reader.ReadByte();
                        reader.Position += 0x02;

                        TevOrders = new IndirectTevOrder[4];
                        for (int i = 0; i < 4; i++)
                        {
                            TevOrders[i] = new IndirectTevOrder(reader);
                        }

                        Matrices = new IndirectTexMatrix[3];
                        for (int i = 0; i < 3; i++)
                            Matrices[i] = new IndirectTexMatrix(reader);

                        Scales = new IndirectTexScale[4];
                        for (int i = 0; i < 4; i++)
                            Scales[i] = new IndirectTexScale(reader);

                        TevStages = new IndirectTevStage[16];
                        for (int i = 0; i < 16; i++)
                            TevStages[i] = new IndirectTevStage(reader);
                    }

                    public void Write(Stream writer)
                    {
                        writer.WriteByte((byte)(HasLookup ? 0x01 : 0x00));
                        writer.WriteByte(IndTexStageNum);

                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);

                        for (int i = 0; i < 4; i++)
                        {
                            TevOrders[i].Write(writer);
                        }

                        for (int i = 0; i < 3; i++)
                            Matrices[i].Write(writer);

                        for (int i = 0; i < 4; i++)
                            Scales[i].Write(writer);

                        for (int i = 0; i < 16; i++)
                            TevStages[i].Write(writer);
                    }

                    public IndirectTexturing Clone()
                    {
                        IndirectTevOrder[] NewOrders = new IndirectTevOrder[TevOrders.Length];
                        IndirectTexMatrix[] NewMatrix = new IndirectTexMatrix[Matrices.Length];
                        IndirectTexScale[] NewScales = new IndirectTexScale[Scales.Length];
                        IndirectTevStage[] NewStages = new IndirectTevStage[TevStages.Length];

                        for (int i = 0; i < TevOrders.Length; i++)
                        {
                            NewOrders[i] = new IndirectTevOrder(TevOrders[i].TexCoord, TevOrders[i].TexMap);
                        }

                        for (int i = 0; i < Matrices.Length; i++)
                            NewMatrix[i] = new IndirectTexMatrix(new Matrix2x3(Matrices[i].Matrix.M11, Matrices[i].Matrix.M12, Matrices[i].Matrix.M13, Matrices[i].Matrix.M21, Matrices[i].Matrix.M22, Matrices[i].Matrix.M23), Matrices[i].Exponent);

                        for (int i = 0; i < Scales.Length; i++)
                            NewScales[i] = new IndirectTexScale(Scales[i].ScaleS, Scales[i].ScaleT);

                        for (int i = 0; i < TevStages.Length; i++)
                            NewStages[i] = new IndirectTevStage(TevStages[i].TevStageID, TevStages[i].IndTexFormat, TevStages[i].IndTexBias, TevStages[i].IndTexMtxId, TevStages[i].IndTexWrapS, TevStages[i].IndTexWrapT, TevStages[i].AddPrev, TevStages[i].UtcLod, TevStages[i].AlphaSel);

                        return new IndirectTexturing() { HasLookup = HasLookup, IndTexStageNum = IndTexStageNum, TevOrders = NewOrders, Matrices = NewMatrix, Scales = NewScales, TevStages = NewStages };
                    }

                    public override bool Equals(object obj)
                    {
                        return obj is IndirectTexturing texturing &&
                               HasLookup == texturing.HasLookup &&
                               IndTexStageNum == texturing.IndTexStageNum &&
                               EqualityComparer<IndirectTevOrder[]>.Default.Equals(TevOrders, texturing.TevOrders) &&
                               EqualityComparer<IndirectTexMatrix[]>.Default.Equals(Matrices, texturing.Matrices) &&
                               EqualityComparer<IndirectTexScale[]>.Default.Equals(Scales, texturing.Scales) &&
                               EqualityComparer<IndirectTevStage[]>.Default.Equals(TevStages, texturing.TevStages);
                    }

                    public override int GetHashCode()
                    {
                        var hashCode = -407782791;
                        hashCode = hashCode * -1521134295 + HasLookup.GetHashCode();
                        hashCode = hashCode * -1521134295 + IndTexStageNum.GetHashCode();
                        hashCode = hashCode * -1521134295 + EqualityComparer<IndirectTevOrder[]>.Default.GetHashCode(TevOrders);
                        hashCode = hashCode * -1521134295 + EqualityComparer<IndirectTexMatrix[]>.Default.GetHashCode(Matrices);
                        hashCode = hashCode * -1521134295 + EqualityComparer<IndirectTexScale[]>.Default.GetHashCode(Scales);
                        hashCode = hashCode * -1521134295 + EqualityComparer<IndirectTevStage[]>.Default.GetHashCode(TevStages);
                        return hashCode;
                    }

                    public struct IndirectTevOrder
                    {
                        public TexCoordId TexCoord;
                        public TexMapId TexMap;

                        public IndirectTevOrder(TexCoordId coordId, TexMapId mapId)
                        {
                            TexCoord = coordId;
                            TexMap = mapId;
                        }

                        public IndirectTevOrder(Stream reader)
                        {
                            TexCoord = (TexCoordId)reader.ReadByte();
                            TexMap = (TexMapId)reader.ReadByte();
                            reader.Position += 0x02;
                        }

                        public void Write(Stream writer)
                        {
                            writer.WriteByte((byte)TexCoord);
                            writer.WriteByte((byte)TexMap);
                            writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2); //TODO: remove?
                        }

                        public override bool Equals(object obj)
                        {
                            if (!(obj is IndirectTevOrder order))
                                return false;

                            return TexCoord == order.TexCoord &&
                                   TexMap == order.TexMap;
                        }

                        public override int GetHashCode()
                        {
                            var hashCode = -584153469;
                            hashCode = hashCode * -1521134295 + TexCoord.GetHashCode();
                            hashCode = hashCode * -1521134295 + TexMap.GetHashCode();
                            return hashCode;
                        }

                        public static bool operator ==(IndirectTevOrder order1, IndirectTevOrder order2) => order1.Equals(order2);

                        public static bool operator !=(IndirectTevOrder order1, IndirectTevOrder order2) => !(order1 == order2);
                    }
                    public struct IndirectTexMatrix
                    {
                        /// <summary>
                        /// The floats that make up the matrix
                        /// </summary>
                        public Matrix2x3 Matrix;
                        /// <summary>
                        /// The exponent (of 2) to multiply the matrix by
                        /// </summary>
                        public byte Exponent;

                        public IndirectTexMatrix(Matrix2x3 matrix, byte exponent)
                        {
                            Matrix = matrix;

                            Exponent = exponent;
                        }

                        public IndirectTexMatrix(Stream stream)
                        {
                            Matrix = new Matrix2x3(
                                stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big),
                                stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big));

                            Exponent = (byte)stream.ReadByte();

                            stream.Position += 0x03;
                        }

                        public void Write(Stream writer)
                        {
                            writer.WriteBigEndian(BitConverter.GetBytes(Matrix.M11), 0, 4);
                            writer.WriteBigEndian(BitConverter.GetBytes(Matrix.M12), 0, 4);
                            writer.WriteBigEndian(BitConverter.GetBytes(Matrix.M13), 0, 4);

                            writer.WriteBigEndian(BitConverter.GetBytes(Matrix.M21), 0, 4);
                            writer.WriteBigEndian(BitConverter.GetBytes(Matrix.M22), 0, 4);
                            writer.WriteBigEndian(BitConverter.GetBytes(Matrix.M23), 0, 4);

                            writer.WriteByte(Exponent);
                            writer.WriteByte(0xFF);
                            writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                        }

                        public override bool Equals(object obj)
                        {
                            if (!(obj is IndirectTexMatrix matrix))
                                return false;

                            return Matrix.Equals(matrix.Matrix) &&
                                   Exponent == matrix.Exponent;
                        }

                        public override int GetHashCode()
                        {
                            var hashCode = 428002898;
                            hashCode = hashCode * -1521134295 + EqualityComparer<Matrix2x3>.Default.GetHashCode(Matrix);
                            hashCode = hashCode * -1521134295 + Exponent.GetHashCode();
                            return hashCode;
                        }

                        public static bool operator ==(IndirectTexMatrix matrix1, IndirectTexMatrix matrix2)
                        {
                            return matrix1.Equals(matrix2);
                        }

                        public static bool operator !=(IndirectTexMatrix matrix1, IndirectTexMatrix matrix2)
                        {
                            return !(matrix1 == matrix2);
                        }
                    }
                    public class IndirectTexScale
                    {
                        /// <summary>
                        /// Scale value for the source texture coordinates' S (U) component
                        /// </summary>
                        public IndirectScale ScaleS { get; private set; }
                        /// <summary>
                        /// Scale value for the source texture coordinates' T (V) component
                        /// </summary>
                        public IndirectScale ScaleT { get; private set; }

                        public IndirectTexScale(IndirectScale s, IndirectScale t)
                        {
                            ScaleS = s;
                            ScaleT = t;
                        }

                        public IndirectTexScale(Stream reader)
                        {
                            ScaleS = (IndirectScale)reader.ReadByte();
                            ScaleT = (IndirectScale)reader.ReadByte();
                            reader.Position += 0x02;
                        }

                        public void Write(Stream writer)
                        {
                            writer.WriteByte((byte)ScaleS);
                            writer.WriteByte((byte)ScaleT);
                            writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                        }

                        public override bool Equals(object obj)
                        {
                            var scale = obj as IndirectTexScale;
                            return scale != null &&
                                   ScaleS == scale.ScaleS &&
                                   ScaleT == scale.ScaleT;
                        }

                        public override int GetHashCode()
                        {
                            var hashCode = 302584437;
                            hashCode = hashCode * -1521134295 + ScaleS.GetHashCode();
                            hashCode = hashCode * -1521134295 + ScaleT.GetHashCode();
                            return hashCode;
                        }

                        public static bool operator ==(IndirectTexScale scale1, IndirectTexScale scale2)
                        {
                            return EqualityComparer<IndirectTexScale>.Default.Equals(scale1, scale2);
                        }

                        public static bool operator !=(IndirectTexScale scale1, IndirectTexScale scale2)
                        {
                            return !(scale1 == scale2);
                        }
                    }
                    public struct IndirectTevStage
                    {
                        public TevStageId TevStageID;
                        public IndirectFormat IndTexFormat;
                        public IndirectBias IndTexBias;
                        public IndirectMatrix IndTexMtxId;
                        public IndirectWrap IndTexWrapS;
                        public IndirectWrap IndTexWrapT;
                        public bool AddPrev;
                        public bool UtcLod;
                        public IndirectAlpha AlphaSel;

                        public IndirectTevStage(TevStageId stageId, IndirectFormat format, IndirectBias bias, IndirectMatrix matrixId, IndirectWrap wrapS, IndirectWrap wrapT, bool addPrev, bool utcLod, IndirectAlpha alphaSel)
                        {
                            TevStageID = stageId;
                            IndTexFormat = format;
                            IndTexBias = bias;
                            IndTexMtxId = matrixId;
                            IndTexWrapS = wrapS;
                            IndTexWrapT = wrapT;
                            AddPrev = addPrev;
                            UtcLod = utcLod;
                            AlphaSel = alphaSel;
                        }

                        public IndirectTevStage(Stream reader)
                        {
                            TevStageID = (TevStageId)reader.ReadByte();
                            IndTexFormat = (IndirectFormat)reader.ReadByte();
                            IndTexBias = (IndirectBias)reader.ReadByte();
                            IndTexMtxId = (IndirectMatrix)reader.ReadByte();
                            IndTexWrapS = (IndirectWrap)reader.ReadByte();
                            IndTexWrapT = (IndirectWrap)reader.ReadByte();
                            AddPrev = reader.ReadByte() > 0;
                            UtcLod = reader.ReadByte() > 0;
                            AlphaSel = (IndirectAlpha)reader.ReadByte();
                            reader.Position += 0x03;
                        }

                        public void Write(Stream writer)
                        {
                            writer.WriteByte((byte)TevStageID);
                            writer.WriteByte((byte)IndTexFormat);
                            writer.WriteByte((byte)IndTexBias);
                            writer.WriteByte((byte)IndTexMtxId);
                            writer.WriteByte((byte)IndTexWrapS);
                            writer.WriteByte((byte)IndTexWrapT);
                            writer.WriteByte((byte)(AddPrev ? 0x01 : 0x00));
                            writer.WriteByte((byte)(UtcLod ? 0x01 : 0x00));
                            writer.WriteByte((byte)AlphaSel);

                            writer.Write(new byte[] { 0xFF, 0xFF, 0xFF }, 0, 3);
                        }

                        public override bool Equals(object obj)
                        {
                            if (!(obj is IndirectTevStage stage))
                                return false;

                            return TevStageID == stage.TevStageID &&
                                   IndTexFormat == stage.IndTexFormat &&
                                   IndTexBias == stage.IndTexBias &&
                                   IndTexMtxId == stage.IndTexMtxId &&
                                   IndTexWrapS == stage.IndTexWrapS &&
                                   IndTexWrapT == stage.IndTexWrapT &&
                                   AddPrev == stage.AddPrev &&
                                   UtcLod == stage.UtcLod &&
                                   AlphaSel == stage.AlphaSel;
                        }

                        public override int GetHashCode()
                        {
                            var hashCode = -1309543118;
                            hashCode = hashCode * -1521134295 + TevStageID.GetHashCode();
                            hashCode = hashCode * -1521134295 + IndTexFormat.GetHashCode();
                            hashCode = hashCode * -1521134295 + IndTexBias.GetHashCode();
                            hashCode = hashCode * -1521134295 + IndTexMtxId.GetHashCode();
                            hashCode = hashCode * -1521134295 + IndTexWrapS.GetHashCode();
                            hashCode = hashCode * -1521134295 + IndTexWrapT.GetHashCode();
                            hashCode = hashCode * -1521134295 + AddPrev.GetHashCode();
                            hashCode = hashCode * -1521134295 + UtcLod.GetHashCode();
                            hashCode = hashCode * -1521134295 + AlphaSel.GetHashCode();
                            return hashCode;
                        }

                        public static bool operator ==(IndirectTevStage stage1, IndirectTevStage stage2) => stage1.Equals(stage2);

                        public static bool operator !=(IndirectTevStage stage1, IndirectTevStage stage2) => !(stage1 == stage2);
                    }

                    public enum IndirectFormat
                    {
                        ITF_8,
                        ITF_5,
                        ITF_4,
                        ITF_3
                    }
                    public enum IndirectBias
                    {
                        None,
                        S,
                        T,
                        ST,
                        U,
                        SU,
                        TU,
                        STU
                    }
                    public enum IndirectAlpha
                    {
                        ITBA_OFF,

                        ITBA_S,
                        ITBA_T,
                        ITBA_U
                    }
                    public enum IndirectMatrix
                    {
                        ITM_OFF,

                        ITM_0,
                        ITM_1,
                        ITM_2,

                        ITM_S0 = 5,
                        ITM_S1,
                        ITM_S2,

                        ITM_T0 = 9,
                        ITM_T1,
                        ITM_T2
                    }
                    public enum IndirectWrap
                    {
                        ITW_OFF,

                        ITW_256,
                        ITW_128,
                        ITW_64,
                        ITW_32,
                        ITW_16,
                        ITW_0
                    }
                    public enum IndirectScale
                    {
                        ITS_1,
                        ITS_2,
                        ITS_4,
                        ITS_8,
                        ITS_16,
                        ITS_32,
                        ITS_64,
                        ITS_128,
                        ITS_256
                    }

                    public static bool operator ==(IndirectTexturing texturing1, IndirectTexturing texturing2) => texturing1.Equals(texturing2);

                    public static bool operator !=(IndirectTexturing texturing1, IndirectTexturing texturing2) => !(texturing1 == texturing2);
                }
                public struct ChannelControl
                {
                    public bool Enable;
                    public ColorSrc MaterialSrcColor;
                    public LightId LitMask;
                    public DiffuseFn DiffuseFunction;
                    public J3DAttenuationFn AttenuationFunction;
                    public ColorSrc AmbientSrcColor;

                    public ChannelControl(bool enable, ColorSrc matSrcColor, LightId litMask, DiffuseFn diffFn, J3DAttenuationFn attenFn, ColorSrc ambSrcColor)
                    {
                        Enable = enable;
                        MaterialSrcColor = matSrcColor;
                        LitMask = litMask;
                        DiffuseFunction = diffFn;
                        AttenuationFunction = attenFn;
                        AmbientSrcColor = ambSrcColor;
                    }

                    public ChannelControl(Stream reader)
                    {
                        Enable = reader.ReadByte() > 0;
                        MaterialSrcColor = (ColorSrc)reader.ReadByte();
                        LitMask = (LightId)reader.ReadByte();
                        DiffuseFunction = (DiffuseFn)reader.ReadByte();
                        AttenuationFunction = (J3DAttenuationFn)reader.ReadByte();
                        AmbientSrcColor = (ColorSrc)reader.ReadByte();

                        reader.Position += 0x02;
                    }

                    public void Write(Stream writer)
                    {
                        writer.WriteByte((byte)(Enable ? 0x01 : 0x00));
                        writer.WriteByte((byte)MaterialSrcColor);
                        writer.WriteByte((byte)LitMask);
                        writer.WriteByte((byte)DiffuseFunction);
                        writer.WriteByte((byte)AttenuationFunction);
                        writer.WriteByte((byte)AmbientSrcColor);

                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                    }

                    public ChannelControl Clone() => new ChannelControl(Enable, MaterialSrcColor, LitMask, DiffuseFunction, AttenuationFunction, AmbientSrcColor);

                    public override bool Equals(object obj)
                    {
                        if (!(obj is ChannelControl control))
                            return false;
                        return Enable == control.Enable &&
                               MaterialSrcColor == control.MaterialSrcColor &&
                               LitMask == control.LitMask &&
                               DiffuseFunction == control.DiffuseFunction &&
                               AttenuationFunction == control.AttenuationFunction &&
                               AmbientSrcColor == control.AmbientSrcColor;
                    }

                    public override int GetHashCode()
                    {
                        var hashCode = -1502031869;
                        hashCode = hashCode * -1521134295 + Enable.GetHashCode();
                        hashCode = hashCode * -1521134295 + MaterialSrcColor.GetHashCode();
                        hashCode = hashCode * -1521134295 + LitMask.GetHashCode();
                        hashCode = hashCode * -1521134295 + DiffuseFunction.GetHashCode();
                        hashCode = hashCode * -1521134295 + AttenuationFunction.GetHashCode();
                        hashCode = hashCode * -1521134295 + AmbientSrcColor.GetHashCode();
                        return hashCode;
                    }

                    public enum ColorSrc
                    {
                        Register = 0, // Use Register Colors
                        Vertex = 1 // Use Vertex Colors
                    }
                    public enum LightId
                    {
                        Light0 = 0x001,
                        Light1 = 0x002,
                        Light2 = 0x004,
                        Light3 = 0x008,
                        Light4 = 0x010,
                        Light5 = 0x020,
                        Light6 = 0x040,
                        Light7 = 0x080,
                        None = 0x000
                    }
                    public enum DiffuseFn
                    {
                        None = 0,
                        Signed = 1,
                        Clamp = 2
                    }
                    public enum J3DAttenuationFn
                    {
                        None_0 = 0,
                        Spec = 1,
                        None_2 = 2,
                        Spot = 3
                    }

                    public static bool operator ==(ChannelControl control1, ChannelControl control2) => control1.Equals(control2);

                    public static bool operator !=(ChannelControl control1, ChannelControl control2) => !(control1 == control2);
                }
                public struct TexCoordGen
                {
                    public TexGenType Type;
                    public TexGenSrc Source;
                    public TexMatrixId TexMatrixSource;

                    public TexCoordGen(Stream reader)
                    {
                        Type = (TexGenType)reader.ReadByte();
                        Source = (TexGenSrc)reader.ReadByte();
                        TexMatrixSource = (TexMatrixId)reader.ReadByte();

                        reader.Position++;
                    }

                    public void Write(Stream writer)
                    {
                        writer.WriteByte((byte)Type);
                        writer.WriteByte((byte)Source);
                        writer.WriteByte((byte)TexMatrixSource);

                        // Pad entry to 4 bytes
                        writer.WriteByte(0xFF);
                    }

                    public TexCoordGen Clone() => new TexCoordGen() { Type = Type, Source = Source, TexMatrixSource = TexMatrixSource };

                    public override bool Equals(object obj)
                    {
                        if (!(obj is TexCoordGen gen))
                            return false;
                        return Type == gen.Type &&
                               Source == gen.Source &&
                               TexMatrixSource == gen.TexMatrixSource;
                    }

                    public override int GetHashCode()
                    {
                        var hashCode = -1253954333;
                        hashCode = hashCode * -1521134295 + Type.GetHashCode();
                        hashCode = hashCode * -1521134295 + Source.GetHashCode();
                        hashCode = hashCode * -1521134295 + TexMatrixSource.GetHashCode();
                        return hashCode;
                    }

                    public static bool operator ==(TexCoordGen gen1, TexCoordGen gen2) => gen1.Equals(gen2);

                    public static bool operator !=(TexCoordGen gen1, TexCoordGen gen2) => !(gen1 == gen2);
                }
                public struct TexMatrix
                {
                    public TexGenType Projection;
                    public TexMtxMapMode MappingMode;
                    public bool IsMaya;
                    public Vector3 Center;
                    public Vector2 Scale;
                    public float Rotation;
                    public Vector2 Translation;
                    public Matrix4 ProjectionMatrix;

                    public TexMatrix(TexGenType projection, byte info, Vector3 effectTranslation, Vector2 scale, float rotation, Vector2 translation, Matrix4 matrix)
                    {
                        Projection = projection;
                        MappingMode = (TexMtxMapMode)(info & 0x3F);
                        IsMaya = (info & ~0x3F) != 0;
                        Center = effectTranslation;

                        Scale = scale;
                        Rotation = rotation;
                        Translation = translation;

                        ProjectionMatrix = matrix;
                    }

                    public TexMatrix(Stream stream)
                    {
                        Projection = (TexGenType)stream.ReadByte();
                        byte info = (byte)stream.ReadByte();
                        MappingMode = (TexMtxMapMode)(info & 0x3F);
                        IsMaya = (info & ~0x3F) != 0;
                        stream.Position += 0x02;
                        Center = new Vector3(stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big));
                        Scale = new Vector2(stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big));
                        Rotation = stream.ReadInt16(Endian.Big) * (180 / 32768f);
                        stream.Position += 0x02;
                        Translation = new Vector2(stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big));

                        ProjectionMatrix = new Matrix4(
                            stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big),
                            stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big),
                            stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big),
                            stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big));
                    }

                    public void Write(Stream writer)
                    {
                        writer.WriteByte((byte)Projection);
                        writer.WriteByte((byte)((IsMaya ? 0x80 : 0) | (byte)MappingMode));
                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                        writer.WriteBigEndian(BitConverter.GetBytes(Center.X), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(Center.Y), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(Center.Z), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(Scale.X), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(Scale.Y), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes((short)(Rotation * (32768.0f / 180))), 0, 2);
                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                        writer.WriteBigEndian(BitConverter.GetBytes(Translation.X), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(Translation.Y), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(ProjectionMatrix.M11), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(ProjectionMatrix.M12), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(ProjectionMatrix.M13), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(ProjectionMatrix.M14), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(ProjectionMatrix.M21), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(ProjectionMatrix.M22), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(ProjectionMatrix.M23), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(ProjectionMatrix.M24), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(ProjectionMatrix.M31), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(ProjectionMatrix.M32), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(ProjectionMatrix.M33), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(ProjectionMatrix.M34), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(ProjectionMatrix.M41), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(ProjectionMatrix.M42), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(ProjectionMatrix.M43), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(ProjectionMatrix.M44), 0, 4);
                    }

                    public TexMatrix Clone()
                    {
                        return new TexMatrix()
                        {
                            Projection = Projection,
                            MappingMode = MappingMode,
                            IsMaya = IsMaya,
                            Center = new Vector3(Center.X, Center.Y, Center.Z),
                            Scale = new Vector2(Scale.X, Scale.Y),
                            Rotation = Rotation,
                            Translation = new Vector2(Translation.X, Translation.Y),
                            ProjectionMatrix = new Matrix4(
                                ProjectionMatrix.M11, ProjectionMatrix.M12, ProjectionMatrix.M13, ProjectionMatrix.M14,
                                ProjectionMatrix.M21, ProjectionMatrix.M22, ProjectionMatrix.M23, ProjectionMatrix.M24,
                                ProjectionMatrix.M31, ProjectionMatrix.M32, ProjectionMatrix.M33, ProjectionMatrix.M34,
                                ProjectionMatrix.M41, ProjectionMatrix.M42, ProjectionMatrix.M43, ProjectionMatrix.M44)
                        };
                    }

                    public override bool Equals(object obj)
                    {
                        if (!(obj is TexMatrix matrix))
                            return false;

                        return Projection == matrix.Projection &&
                               MappingMode == matrix.MappingMode &&
                               IsMaya == matrix.IsMaya &&
                               Center.Equals(matrix.Center) &&
                               Scale.Equals(matrix.Scale) &&
                               Rotation == matrix.Rotation &&
                               Translation.Equals(matrix.Translation) &&
                               ProjectionMatrix.Equals(matrix.ProjectionMatrix);
                    }

                    public override int GetHashCode()
                    {
                        var hashCode = 1621759504;
                        hashCode = hashCode * -1521134295 + Projection.GetHashCode();
                        hashCode = hashCode * -1521134295 + MappingMode.GetHashCode();
                        hashCode = hashCode * -1521134295 + IsMaya.GetHashCode();
                        hashCode = hashCode * -1521134295 + EqualityComparer<Vector3>.Default.GetHashCode(Center);
                        hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(Scale);
                        hashCode = hashCode * -1521134295 + Rotation.GetHashCode();
                        hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(Translation);
                        hashCode = hashCode * -1521134295 + EqualityComparer<Matrix4>.Default.GetHashCode(ProjectionMatrix);
                        return hashCode;
                    }

                    public static bool operator ==(TexMatrix matrix1, TexMatrix matrix2) => matrix1.Equals(matrix2);

                    public static bool operator !=(TexMatrix matrix1, TexMatrix matrix2) => !(matrix1 == matrix2);
                }
                public struct TevOrder
                {
                    public TexCoordId TexCoord;
                    public TexMapId TexMap;
                    public GXColorChannelId ChannelId;

                    public TevOrder(TexCoordId texCoord, TexMapId texMap, GXColorChannelId chanID)
                    {
                        TexCoord = texCoord;
                        TexMap = texMap;
                        ChannelId = chanID;
                    }

                    public TevOrder(Stream reader)
                    {
                        TexCoord = (TexCoordId)reader.ReadByte();
                        TexMap = (TexMapId)reader.ReadByte();
                        ChannelId = (GXColorChannelId)reader.ReadByte();
                        reader.Position++;
                    }
                    public void Write(Stream writer)
                    {
                        writer.WriteByte((byte)TexCoord);
                        writer.WriteByte((byte)TexMap);
                        writer.WriteByte((byte)ChannelId);
                        writer.WriteByte(0xFF);
                    }

                    public TevOrder Clone() => new TevOrder(TexCoord, TexMap, ChannelId);

                    public override bool Equals(object obj)
                    {
                        if (!(obj is TevOrder order))
                            return false;
                        return TexCoord == order.TexCoord &&
                               TexMap == order.TexMap &&
                               ChannelId == order.ChannelId;
                    }

                    public override int GetHashCode()
                    {
                        var hashCode = -1126351388;
                        hashCode = hashCode * -1521134295 + TexCoord.GetHashCode();
                        hashCode = hashCode * -1521134295 + TexMap.GetHashCode();
                        hashCode = hashCode * -1521134295 + ChannelId.GetHashCode();
                        return hashCode;
                    }

                    public enum GXColorChannelId
                    {
                        Color0 = 0,
                        Color1 = 1,
                        Alpha0 = 2,
                        Alpha1 = 3,
                        Color0A0 = 4,
                        Color1A1 = 5,
                        ColorZero = 6,
                        AlphaBump = 7,
                        AlphaBumpN = 8,
                        ColorNull = 0xFF
                    }

                    public static bool operator ==(TevOrder order1, TevOrder order2) => order1.Equals(order2);

                    public static bool operator !=(TevOrder order1, TevOrder order2) => !(order1 == order2);
                }
                public struct TevStage
                {
                    public CombineColorInput ColorInA;
                    public CombineColorInput ColorInB;
                    public CombineColorInput ColorInC;
                    public CombineColorInput ColorInD;

                    public TevOp ColorOp;
                    public TevBias ColorBias;
                    public TevScale ColorScale;
                    public bool ColorClamp;
                    public TevRegisterId ColorRegId;

                    public CombineAlphaInput AlphaInA;
                    public CombineAlphaInput AlphaInB;
                    public CombineAlphaInput AlphaInC;
                    public CombineAlphaInput AlphaInD;

                    public TevOp AlphaOp;
                    public TevBias AlphaBias;
                    public TevScale AlphaScale;
                    public bool AlphaClamp;
                    public TevRegisterId AlphaRegId;

                    public TevStage(Stream reader)
                    {
                        reader.Position++;

                        ColorInA = (CombineColorInput)reader.ReadByte();
                        ColorInB = (CombineColorInput)reader.ReadByte();
                        ColorInC = (CombineColorInput)reader.ReadByte();
                        ColorInD = (CombineColorInput)reader.ReadByte();

                        ColorOp = (TevOp)reader.ReadByte();
                        ColorBias = (TevBias)reader.ReadByte();
                        ColorScale = (TevScale)reader.ReadByte();
                        ColorClamp = reader.ReadByte() > 0;
                        ColorRegId = (TevRegisterId)reader.ReadByte();

                        AlphaInA = (CombineAlphaInput)reader.ReadByte();
                        AlphaInB = (CombineAlphaInput)reader.ReadByte();
                        AlphaInC = (CombineAlphaInput)reader.ReadByte();
                        AlphaInD = (CombineAlphaInput)reader.ReadByte();

                        AlphaOp = (TevOp)reader.ReadByte();
                        AlphaBias = (TevBias)reader.ReadByte();
                        AlphaScale = (TevScale)reader.ReadByte();
                        AlphaClamp = reader.ReadByte() > 0;
                        AlphaRegId = (TevRegisterId)reader.ReadByte();

                        reader.Position++;
                    }

                    public void Write(Stream writer)
                    {
                        writer.WriteByte(0xFF);

                        writer.WriteByte((byte)ColorInA);
                        writer.WriteByte((byte)ColorInB);
                        writer.WriteByte((byte)ColorInC);
                        writer.WriteByte((byte)ColorInD);

                        writer.WriteByte((byte)ColorOp);
                        writer.WriteByte((byte)ColorBias);
                        writer.WriteByte((byte)ColorScale);
                        writer.WriteByte((byte)(ColorClamp ? 0x01 : 0x00));
                        writer.WriteByte((byte)ColorRegId);

                        writer.WriteByte((byte)AlphaInA);
                        writer.WriteByte((byte)AlphaInB);
                        writer.WriteByte((byte)AlphaInC);
                        writer.WriteByte((byte)AlphaInD);

                        writer.WriteByte((byte)AlphaOp);
                        writer.WriteByte((byte)AlphaBias);
                        writer.WriteByte((byte)AlphaScale);
                        writer.WriteByte((byte)(AlphaClamp ? 0x01 : 0x00));
                        writer.WriteByte((byte)AlphaRegId);

                        writer.WriteByte(0xFF);
                    }

                    public TevStage Clone() => new TevStage()
                    {
                        ColorInA = ColorInA,
                        ColorInB = ColorInB,
                        ColorInC = ColorInC,
                        ColorInD = ColorInD,
                        ColorOp = ColorOp,
                        ColorBias = ColorBias,
                        ColorScale = ColorScale,
                        ColorClamp = ColorClamp,
                        ColorRegId = ColorRegId,
                        AlphaInA = AlphaInA,
                        AlphaInB = AlphaInB,
                        AlphaInC = AlphaInC,
                        AlphaInD = AlphaInD,
                        AlphaOp = AlphaOp,
                        AlphaBias = AlphaBias,
                        AlphaScale = AlphaScale,
                        AlphaClamp = AlphaClamp,
                        AlphaRegId = AlphaRegId
                    };

                    public override string ToString()
                    {
                        string ret = "";

                        ret += $"Color In A: {ColorInA}\n";
                        ret += $"Color In B: {ColorInB}\n";
                        ret += $"Color In C: {ColorInC}\n";
                        ret += $"Color In D: {ColorInD}\n";

                        ret += '\n';

                        ret += $"Color Op: {ColorOp}\n";
                        ret += $"Color Bias: {ColorBias}\n";
                        ret += $"Color Scale: {ColorScale}\n";
                        ret += $"Color Clamp: {ColorClamp}\n";
                        ret += $"Color Reg ID: {ColorRegId}\n";

                        ret += '\n';

                        ret += $"Alpha In A: {AlphaInA}\n";
                        ret += $"Alpha In B: {AlphaInB}\n";
                        ret += $"Alpha In C: {AlphaInC}\n";
                        ret += $"Alpha In D: {AlphaInD}\n";

                        ret += '\n';

                        ret += $"Alpha Op: {AlphaOp}\n";
                        ret += $"Alpha Bias: {AlphaBias}\n";
                        ret += $"Alpha Scale: {AlphaScale}\n";
                        ret += $"Alpha Clamp: {AlphaClamp}\n";
                        ret += $"Alpha Reg ID: {AlphaRegId}\n";

                        ret += '\n';

                        return ret;
                    }

                    public override bool Equals(object obj)
                    {
                        if (!(obj is TevStage stage))
                            return false;

                        return ColorInA == stage.ColorInA &&
                               ColorInB == stage.ColorInB &&
                               ColorInC == stage.ColorInC &&
                               ColorInD == stage.ColorInD &&
                               ColorOp == stage.ColorOp &&
                               ColorBias == stage.ColorBias &&
                               ColorScale == stage.ColorScale &&
                               ColorClamp == stage.ColorClamp &&
                               ColorRegId == stage.ColorRegId &&
                               AlphaInA == stage.AlphaInA &&
                               AlphaInB == stage.AlphaInB &&
                               AlphaInC == stage.AlphaInC &&
                               AlphaInD == stage.AlphaInD &&
                               AlphaOp == stage.AlphaOp &&
                               AlphaBias == stage.AlphaBias &&
                               AlphaScale == stage.AlphaScale &&
                               AlphaClamp == stage.AlphaClamp &&
                               AlphaRegId == stage.AlphaRegId;
                    }

                    public override int GetHashCode()
                    {
                        var hashCode = -411571779;
                        hashCode = hashCode * -1521134295 + ColorInA.GetHashCode();
                        hashCode = hashCode * -1521134295 + ColorInB.GetHashCode();
                        hashCode = hashCode * -1521134295 + ColorInC.GetHashCode();
                        hashCode = hashCode * -1521134295 + ColorInD.GetHashCode();
                        hashCode = hashCode * -1521134295 + ColorOp.GetHashCode();
                        hashCode = hashCode * -1521134295 + ColorBias.GetHashCode();
                        hashCode = hashCode * -1521134295 + ColorScale.GetHashCode();
                        hashCode = hashCode * -1521134295 + ColorClamp.GetHashCode();
                        hashCode = hashCode * -1521134295 + ColorRegId.GetHashCode();
                        hashCode = hashCode * -1521134295 + AlphaInA.GetHashCode();
                        hashCode = hashCode * -1521134295 + AlphaInB.GetHashCode();
                        hashCode = hashCode * -1521134295 + AlphaInC.GetHashCode();
                        hashCode = hashCode * -1521134295 + AlphaInD.GetHashCode();
                        hashCode = hashCode * -1521134295 + AlphaOp.GetHashCode();
                        hashCode = hashCode * -1521134295 + AlphaBias.GetHashCode();
                        hashCode = hashCode * -1521134295 + AlphaScale.GetHashCode();
                        hashCode = hashCode * -1521134295 + AlphaClamp.GetHashCode();
                        hashCode = hashCode * -1521134295 + AlphaRegId.GetHashCode();
                        return hashCode;
                    }

                    public enum CombineColorInput
                    {
                        ColorPrev = 0,  // ! < Use Color Value from previous TEV stage
                        AlphaPrev = 1,  // ! < Use Alpha Value from previous TEV stage
                        C0 = 2,         // ! < Use the Color Value from the Color/Output Register 0
                        A0 = 3,         // ! < Use the Alpha value from the Color/Output Register 0
                        C1 = 4,         // ! < Use the Color Value from the Color/Output Register 1
                        A1 = 5,         // ! < Use the Alpha value from the Color/Output Register 1
                        C2 = 6,         // ! < Use the Color Value from the Color/Output Register 2
                        A2 = 7,         // ! < Use the Alpha value from the Color/Output Register 2
                        TexColor = 8,   // ! < Use the Color value from Texture
                        TexAlpha = 9,   // ! < Use the Alpha value from Texture
                        RasColor = 10,  // ! < Use the color value from rasterizer
                        RasAlpha = 11,  // ! < Use the alpha value from rasterizer
                        One = 12,
                        Half = 13,
                        Konst = 14,
                        Zero = 15       //
                    }
                    public enum CombineAlphaInput
                    {
                        AlphaPrev = 0,  // Use the Alpha value form the previous TEV stage
                        A0 = 1,         // Use the Alpha value from the Color/Output Register 0
                        A1 = 2,         // Use the Alpha value from the Color/Output Register 1
                        A2 = 3,         // Use the Alpha value from the Color/Output Register 2
                        TexAlpha = 4,   // Use the Alpha value from the Texture
                        RasAlpha = 5,   // Use the Alpha value from the rasterizer
                        Konst = 6,
                        Zero = 7
                    }
                    public enum TevOp
                    {
                        Add = 0,
                        Sub = 1,
                        Comp_R8_GT = 8,
                        Comp_R8_EQ = 9,
                        Comp_GR16_GT = 10,
                        Comp_GR16_EQ = 11,
                        Comp_BGR24_GT = 12,
                        Comp_BGR24_EQ = 13,
                        Comp_RGB8_GT = 14,
                        Comp_RGB8_EQ = 15,
                        Comp_A8_EQ = Comp_RGB8_EQ,
                        Comp_A8_GT = Comp_RGB8_GT
                    }
                    public enum TevBias
                    {
                        Zero = 0,
                        AddHalf = 1,
                        SubHalf = 2
                    }
                    public enum TevScale
                    {
                        Scale_1 = 0,
                        Scale_2 = 1,
                        Scale_4 = 2,
                        Divide_2 = 3
                    }
                    public enum TevRegisterId
                    {
                        TevPrev,
                        TevReg0,
                        TevReg1,
                        TevReg2
                    }

                    public static bool operator ==(TevStage stage1, TevStage stage2) => stage1.Equals(stage2);

                    public static bool operator !=(TevStage stage1, TevStage stage2) => !(stage1 == stage2);
                }
                public struct TevSwapMode
                {
                    public byte RasSel;
                    public byte TexSel;

                    public TevSwapMode(byte rasSel, byte texSel)
                    {
                        RasSel = rasSel;
                        TexSel = texSel;
                    }

                    public TevSwapMode(Stream reader)
                    {
                        RasSel = (byte)reader.ReadByte();
                        TexSel = (byte)reader.ReadByte();
                        reader.Position += 0x02;
                    }
                    public void Write(Stream writer)
                    {
                        writer.WriteByte(RasSel);
                        writer.WriteByte(TexSel);
                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                    }

                    public TevSwapMode Clone() => new TevSwapMode(RasSel, TexSel);

                    public override bool Equals(object obj)
                    {
                        if (!(obj is TevSwapMode mode))
                            return false;
                        return RasSel == mode.RasSel &&
                               TexSel == mode.TexSel;
                    }

                    public override int GetHashCode()
                    {
                        var hashCode = 2132594825;
                        hashCode = hashCode * -1521134295 + RasSel.GetHashCode();
                        hashCode = hashCode * -1521134295 + TexSel.GetHashCode();
                        return hashCode;
                    }

                    public static bool operator ==(TevSwapMode mode1, TevSwapMode mode2)
                    {
                        return mode1.Equals(mode2);
                    }

                    public static bool operator !=(TevSwapMode mode1, TevSwapMode mode2)
                    {
                        return !(mode1 == mode2);
                    }
                }
                public struct TevSwapModeTable
                {
                    public byte R;
                    public byte G;
                    public byte B;
                    public byte A;

                    public TevSwapModeTable(byte r, byte g, byte b, byte a)
                    {
                        R = r;
                        G = g;
                        B = b;
                        A = a;
                    }

                    public TevSwapModeTable(Stream reader)
                    {
                        R = (byte)reader.ReadByte();
                        G = (byte)reader.ReadByte();
                        B = (byte)reader.ReadByte();
                        A = (byte)reader.ReadByte();
                    }

                    public void Write(Stream writer)
                    {
                        writer.WriteByte(R);
                        writer.WriteByte(G);
                        writer.WriteByte(B);
                        writer.WriteByte(A);
                    }

                    public TevSwapModeTable Clone() => new TevSwapModeTable(R, G, B, A);

                    public override bool Equals(object obj)
                    {
                        if (!(obj is TevSwapModeTable table))
                            return false;
                        return R == table.R &&
                               G == table.G &&
                               B == table.B &&
                               A == table.A;
                    }

                    public override int GetHashCode()
                    {
                        var hashCode = 1960784236;
                        hashCode = hashCode * -1521134295 + R.GetHashCode();
                        hashCode = hashCode * -1521134295 + G.GetHashCode();
                        hashCode = hashCode * -1521134295 + B.GetHashCode();
                        hashCode = hashCode * -1521134295 + A.GetHashCode();
                        return hashCode;
                    }

                    public static bool operator ==(TevSwapModeTable table1, TevSwapModeTable table2)
                    {
                        return table1.Equals(table2);
                    }

                    public static bool operator !=(TevSwapModeTable table1, TevSwapModeTable table2)
                    {
                        return !(table1 == table2);
                    }
                }
                public struct Fog
                {
                    public byte Type;
                    public bool Enable;
                    public ushort Center;
                    public float StartZ;
                    public float EndZ;
                    public float NearZ;
                    public float FarZ;
                    public Color4 Color;
                    public float[] RangeAdjustmentTable;

                    public Fog(byte type, bool enable, ushort center, float startZ, float endZ, float nearZ, float farZ, Color4 color, float[] rangeAdjust)
                    {
                        Type = type;
                        Enable = enable;
                        Center = center;
                        StartZ = startZ;
                        EndZ = endZ;
                        NearZ = nearZ;
                        FarZ = farZ;
                        Color = color;
                        RangeAdjustmentTable = rangeAdjust;
                    }

                    public Fog(Stream stream)
                    {
                        RangeAdjustmentTable = new float[10];

                        Type = (byte)stream.ReadByte();
                        Enable = stream.ReadByte() > 0;
                        Center = stream.ReadUInt16(Endian.Big);
                        StartZ = stream.ReadSingle(Endian.Big);
                        EndZ = stream.ReadSingle(Endian.Big);
                        NearZ = stream.ReadSingle(Endian.Big);
                        FarZ = stream.ReadSingle(Endian.Big);
                        Color = new Color4((float)stream.ReadByte() / 255, (float)stream.ReadByte() / 255, (float)stream.ReadByte() / 255, (float)stream.ReadByte() / 255);

                        for (int i = 0; i < 10; i++)
                        {
                            ushort inVal = stream.ReadUInt16(Endian.Big);
                            RangeAdjustmentTable[i] = (float)inVal / 256;
                        }
                    }

                    public void Write(Stream writer)
                    {
                        writer.WriteByte(Type);
                        writer.WriteByte((byte)(Enable ? 0x01 : 0x00));
                        writer.WriteBigEndian(BitConverter.GetBytes(Center), 0, 2);
                        writer.WriteBigEndian(BitConverter.GetBytes(StartZ), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(EndZ), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(NearZ), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(FarZ), 0, 4);
                        writer.WriteByte((byte)(Color.R * 255));
                        writer.WriteByte((byte)(Color.G * 255));
                        writer.WriteByte((byte)(Color.B * 255));
                        writer.WriteByte((byte)(Color.A * 255));

                        for (int i = 0; i < 10; i++)
                            writer.WriteBigEndian(BitConverter.GetBytes((ushort)(RangeAdjustmentTable[i] * 256)), 0, 2);
                    }

                    public Fog Clone()
                    {
                        float[] temp = new float[RangeAdjustmentTable.Length];
                        for (int i = 0; i < RangeAdjustmentTable.Length; i++)
                            temp[i] = RangeAdjustmentTable[i];
                        return new Fog(Type, Enable, Center, StartZ, EndZ, NearZ, FarZ, new Color4(Color.R, Color.G, Color.B, Color.A), temp);
                    }

                    public override bool Equals(object obj)
                    {
                        if (!(obj is Fog fog))
                            return false;
                        return Type == fog.Type &&
                               Enable == fog.Enable &&
                               Center == fog.Center &&
                               StartZ == fog.StartZ &&
                               EndZ == fog.EndZ &&
                               NearZ == fog.NearZ &&
                               FarZ == fog.FarZ &&
                               Color.Equals(fog.Color) &&
                               EqualityComparer<float[]>.Default.Equals(RangeAdjustmentTable, fog.RangeAdjustmentTable);
                    }

                    public override int GetHashCode()
                    {
                        var hashCode = 1878492404;
                        hashCode = hashCode * -1521134295 + Type.GetHashCode();
                        hashCode = hashCode * -1521134295 + Enable.GetHashCode();
                        hashCode = hashCode * -1521134295 + Center.GetHashCode();
                        hashCode = hashCode * -1521134295 + StartZ.GetHashCode();
                        hashCode = hashCode * -1521134295 + EndZ.GetHashCode();
                        hashCode = hashCode * -1521134295 + NearZ.GetHashCode();
                        hashCode = hashCode * -1521134295 + FarZ.GetHashCode();
                        hashCode = hashCode * -1521134295 + EqualityComparer<Color4>.Default.GetHashCode(Color);
                        hashCode = hashCode * -1521134295 + EqualityComparer<float[]>.Default.GetHashCode(RangeAdjustmentTable);
                        return hashCode;
                    }

                    public static bool operator ==(Fog fog1, Fog fog2) => fog1.Equals(fog2);

                    public static bool operator !=(Fog fog1, Fog fog2) => !(fog1 == fog2);
                }
                public struct AlphaCompare
                {
                    /// <summary> subfunction 0 </summary>
                    public GxCompareType Comp0;
                    /// <summary> Reference value for subfunction 0. </summary>
                    public byte Reference0;
                    /// <summary> Alpha combine control for subfunctions 0 and 1. </summary>
                    public GXAlphaOp Operation;
                    /// <summary> subfunction 1 </summary>
                    public GxCompareType Comp1;
                    /// <summary> Reference value for subfunction 1. </summary>
                    public byte Reference1;

                    public AlphaCompare(GxCompareType comp0, byte ref0, GXAlphaOp operation, GxCompareType comp1, byte ref1)
                    {
                        Comp0 = comp0;
                        Reference0 = ref0;
                        Operation = operation;
                        Comp1 = comp1;
                        Reference1 = ref1;
                    }

                    public AlphaCompare(Stream reader)
                    {
                        Comp0 = (GxCompareType)reader.ReadByte();
                        Reference0 = (byte)reader.ReadByte();
                        Operation = (GXAlphaOp)reader.ReadByte();
                        Comp1 = (GxCompareType)reader.ReadByte();
                        Reference1 = (byte)reader.ReadByte();
                        reader.Position += 0x03;
                    }

                    public void Write(Stream writer)
                    {
                        writer.WriteByte((byte)Comp0);
                        writer.WriteByte(Reference0);
                        writer.WriteByte((byte)Operation);
                        writer.WriteByte((byte)Comp1);
                        writer.WriteByte(Reference1);
                        writer.WriteByte(0xFF);
                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                    }

                    public AlphaCompare Clone() => new AlphaCompare(Comp0, Reference0, Operation, Comp1, Reference1);

                    public override bool Equals(object obj)
                    {
                        if (!(obj is AlphaCompare compare))
                            return false;
                        return Comp0 == compare.Comp0 &&
                               Reference0 == compare.Reference0 &&
                               Operation == compare.Operation &&
                               Comp1 == compare.Comp1 &&
                               Reference1 == compare.Reference1;
                    }

                    public override int GetHashCode()
                    {
                        var hashCode = 233009852;
                        hashCode = hashCode * -1521134295 + Comp0.GetHashCode();
                        hashCode = hashCode * -1521134295 + Reference0.GetHashCode();
                        hashCode = hashCode * -1521134295 + Operation.GetHashCode();
                        hashCode = hashCode * -1521134295 + Comp1.GetHashCode();
                        hashCode = hashCode * -1521134295 + Reference1.GetHashCode();
                        return hashCode;
                    }

                    public enum GxCompareType
                    {
                        Never = 0,
                        Less = 1,
                        Equal = 2,
                        LEqual = 3,
                        Greater = 4,
                        NEqual = 5,
                        GEqual = 6,
                        Always = 7
                    }
                    public enum GXAlphaOp
                    {
                        And = 0,
                        Or = 1,
                        XOR = 2,
                        XNOR = 3
                    }

                    public static bool operator ==(AlphaCompare compare1, AlphaCompare compare2) => compare1.Equals(compare2);

                    public static bool operator !=(AlphaCompare compare1, AlphaCompare compare2) => !(compare1 == compare2);
                }
                public struct BlendMode
                {
                    /// <summary> Blending Type </summary>
                    public BlendModeID Type;
                    /// <summary> Blending Control </summary>
                    public BlendModeControl SourceFact;
                    /// <summary> Blending Control </summary>
                    public BlendModeControl DestinationFact;
                    /// <summary> What operation is used to blend them when <see cref="Type"/> is set to <see cref="GXBlendMode.Logic"/>. </summary>
                    public LogicOp Operation; // Seems to be logic operators such as clear, and, copy, equiv, inv, invand, etc.

                    public BlendMode(BlendModeID type, BlendModeControl src, BlendModeControl dest, LogicOp operation)
                    {
                        Type = type;
                        SourceFact = src;
                        DestinationFact = dest;
                        Operation = operation;
                    }

                    public BlendMode(Stream reader)
                    {
                        Type = (BlendModeID)reader.ReadByte();
                        SourceFact = (BlendModeControl)reader.ReadByte();
                        DestinationFact = (BlendModeControl)reader.ReadByte();
                        Operation = (LogicOp)reader.ReadByte();
                    }

                    public void Write(Stream write)
                    {
                        write.WriteByte((byte)Type);
                        write.WriteByte((byte)SourceFact);
                        write.WriteByte((byte)DestinationFact);
                        write.WriteByte((byte)Operation);
                    }

                    public BlendMode Clone() => new BlendMode(Type, SourceFact, DestinationFact, Operation);

                    public override bool Equals(object obj)
                    {
                        if (!(obj is BlendMode mode))
                            return false;
                        return Type == mode.Type &&
                               SourceFact == mode.SourceFact &&
                               DestinationFact == mode.DestinationFact &&
                               Operation == mode.Operation;
                    }

                    public override int GetHashCode()
                    {
                        var hashCode = -565238750;
                        hashCode = hashCode * -1521134295 + Type.GetHashCode();
                        hashCode = hashCode * -1521134295 + SourceFact.GetHashCode();
                        hashCode = hashCode * -1521134295 + DestinationFact.GetHashCode();
                        hashCode = hashCode * -1521134295 + Operation.GetHashCode();
                        return hashCode;
                    }

                    public enum BlendModeID
                    {
                        None = 0,
                        Blend = 1,
                        Logic = 2,
                        Subtract = 3
                    }
                    public enum BlendModeControl
                    {
                        Zero = 0,               // ! < 0.0
                        One = 1,                // ! < 1.0
                        SrcColor = 2,           // ! < Source Color
                        InverseSrcColor = 3,    // ! < 1.0 - (Source Color)
                        SrcAlpha = 4,           // ! < Source Alpha
                        InverseSrcAlpha = 5,    // ! < 1.0 - (Source Alpha)
                        DstAlpha = 6,           // ! < Framebuffer Alpha
                        InverseDstAlpha = 7     // ! < 1.0 - (Framebuffer Alpha)
                    }
                    public enum LogicOp
                    {
                        Clear = 0,
                        And = 1,
                        RevAnd = 2,
                        Copy = 3,
                        InvAnd = 4,
                        NoOp = 5,
                        XOr = 6,
                        Or = 7,
                        NOr = 8,
                        Equiv = 9,
                        Inv = 10,
                        RevOr = 11,
                        InvCopy = 12,
                        InvOr = 13,
                        NAnd = 14,
                        Set = 15,
                    }

                    public static bool operator ==(BlendMode mode1, BlendMode mode2) => mode1.Equals(mode2);

                    public static bool operator !=(BlendMode mode1, BlendMode mode2) => !(mode1 == mode2);
                }
                public struct ZModeHolder
                {
                    /// <summary> If false, ZBuffering is disabled and the Z buffer is not updated. </summary>
                    public bool Enable;

                    /// <summary> Determines the comparison that is performed.
                    /// The newely rasterized Z value is on the left while the value from the Z buffer is on the right.
                    /// If the result of the comparison is false, the newly rasterized pixel is discarded. </summary>
                    public AlphaCompare.GxCompareType Function;

                    /// <summary> If true, the Z buffer is updated with the new Z value after a comparison is performed.
                    /// Example: Disabling this would prevent a write to the Z buffer, useful for UI elements or other things
                    /// that shouldn't write to Z Buffer. See glDepthMask. </summary>
                    public bool UpdateEnable;

                    public ZModeHolder(bool enable, AlphaCompare.GxCompareType func, bool update)
                    {
                        Enable = enable;
                        Function = func;
                        UpdateEnable = update;
                    }

                    public ZModeHolder(Stream reader)
                    {
                        Enable = reader.ReadByte() > 0;
                        Function = (AlphaCompare.GxCompareType)reader.ReadByte();
                        UpdateEnable = reader.ReadByte() > 0;
                        reader.Position++;
                    }

                    public void Write(Stream writer)
                    {
                        writer.WriteByte((byte)(Enable ? 0x01 : 0x00));
                        writer.WriteByte((byte)Function);
                        writer.WriteByte((byte)(UpdateEnable ? 0x01 : 0x00));
                        writer.WriteByte(0xFF);
                    }

                    public ZModeHolder Clone() => new ZModeHolder(Enable, Function, UpdateEnable);

                    public override int GetHashCode()
                    {
                        var hashCode = -1724780622;
                        hashCode = hashCode * -1521134295 + Enable.GetHashCode();
                        hashCode = hashCode * -1521134295 + Function.GetHashCode();
                        hashCode = hashCode * -1521134295 + UpdateEnable.GetHashCode();
                        return hashCode;
                    }

                    public override bool Equals(object obj)
                    {
                        if (!(obj is ZModeHolder holder))
                            return false;

                        return Enable == holder.Enable &&
                               Function == holder.Function &&
                               UpdateEnable == holder.UpdateEnable;
                    }

                    public static bool operator ==(ZModeHolder holder1, ZModeHolder holder2) => holder1.Equals(holder2);

                    public static bool operator !=(ZModeHolder holder1, ZModeHolder holder2) => !(holder1 == holder2);
                }
                public struct NBTScaleHolder
                {
                    public byte Unknown1;

                    public Vector3 Scale;

                    public NBTScaleHolder(byte unk1, Vector3 scale)
                    {
                        Unknown1 = unk1;
                        Scale = scale;
                    }

                    public NBTScaleHolder(Stream stream)
                    {
                        Unknown1 = (byte)stream.ReadByte();
                        stream.Position += 0x03;
                        Scale = new Vector3(stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big));
                    }

                    public void Write(Stream writer)
                    {
                        writer.WriteByte(Unknown1);
                        writer.WriteByte(0xFF);
                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                        writer.WriteBigEndian(BitConverter.GetBytes(Scale.X), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(Scale.Y), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(Scale.Z), 0, 4);
                    }

                    public NBTScaleHolder Clone() => new NBTScaleHolder(Unknown1, new Vector3(Scale.X, Scale.Y, Scale.Z));

                    public override bool Equals(object obj)
                    {
                        if (!(obj is NBTScaleHolder holder))
                            return false;
                        return Unknown1 == holder.Unknown1 &&
                               Scale.Equals(holder.Scale);
                    }

                    public override int GetHashCode()
                    {
                        var hashCode = 1461352585;
                        hashCode = hashCode * -1521134295 + Unknown1.GetHashCode();
                        hashCode = hashCode * -1521134295 + EqualityComparer<Vector3>.Default.GetHashCode(Scale);
                        return hashCode;
                    }

                    public static bool operator ==(NBTScaleHolder holder1, NBTScaleHolder holder2)
                    {
                        return holder1.Equals(holder2);
                    }

                    public static bool operator !=(NBTScaleHolder holder1, NBTScaleHolder holder2)
                    {
                        return !(holder1 == holder2);
                    }
                }

                public static bool operator ==(Material material1, Material material2) => material1.Equals(material2);

                public static bool operator !=(Material material1, Material material2) => !(material1 == material2);
            }

            public enum CullMode
            {
                None = 0,   // Do not cull any primitives
                Front = 1,  // Cull front-facing primitives
                Back = 2,   // Cull back-facing primitives
                All = 3     // Cull all primitives
            }
            public enum TexCoordId
            {
                TexCoord0 = 0,
                TexCoord1 = 1,
                TexCoord2 = 2,
                TexCoord3 = 3,
                TexCoord4 = 4,
                TexCoord5 = 5,
                TexCoord6 = 6,
                TexCoord7 = 7,
                Null = 0xFF
            }
            public enum TexMapId
            {
                TexMap0,
                TexMap1,
                TexMap2,
                TexMap3,
                TexMap4,
                TexMap5,
                TexMap6,
                TexMap7,

                Null = 0xFF,
            }
            public enum TexMatrixId
            {
                Identity = 60,
                TexMtx0 = 30,
                TexMtx1 = 33,
                TexMtx2 = 36,
                TexMtx3 = 39,
                TexMtx4 = 42,
                TexMtx5 = 45,
                TexMtx6 = 48,
                TexMtx7 = 51,
                TexMtx8 = 54,
                TexMtx9 = 57
            }
            public enum TexGenType
            {
                Matrix3x4 = 0,
                Matrix2x4 = 1,
                Bump0 = 2,
                Bump1 = 3,
                Bump2 = 4,
                Bump3 = 5,
                Bump4 = 6,
                Bump5 = 7,
                Bump6 = 8,
                Bump7 = 9,
                SRTG = 10
            }
            public enum TexMtxMapMode
            {
                None = 0x00,
                // Uses "Basic" conventions, no -1...1 remap.
                // Peach Beach uses EnvmapBasic, not sure on what yet...
                EnvmapBasic = 0x01,
                ProjmapBasic = 0x02,
                ViewProjmapBasic = 0x03,
                // Unknown: 0x04, 0x05. No known uses.
                // Uses "Old" conventions, remaps translation in fourth component
                // TODO(jstpierre): Figure out the geometric interpretation of old vs. new
                EnvmapOld = 0x06,
                // Uses "New" conventions, remaps translation in third component
                Envmap = 0x07,
                Projmap = 0x08,
                ViewProjmap = 0x09,
                // Environment map, but based on a custom effect matrix instead of the default view
                // matrix. Used by certain actors in Wind Waker, like zouK1 in Master Sword Chamber.
                EnvmapOldEffectMtx = 0x0A,
                EnvmapEffectMtx = 0x0B,
            }
            public enum TexGenSrc
            {
                Position = 0,
                Normal = 1,
                Binormal = 2,
                Tangent = 3,
                Tex0 = 4,
                Tex1 = 5,
                Tex2 = 6,
                Tex3 = 7,
                Tex4 = 8,
                Tex5 = 9,
                Tex6 = 10,
                Tex7 = 11,
                TexCoord0 = 12,
                TexCoord1 = 13,
                TexCoord2 = 14,
                TexCoord3 = 15,
                TexCoord4 = 16,
                TexCoord5 = 17,
                TexCoord6 = 18,
                Color0 = 19,
                Color1 = 20,
            }
            public enum TevStageId
            {
                TevStage0,
                TevStage1,
                TevStage2,
                TevStage3,
                TevStage4,
                TevStage5,
                TevStage6,
                TevStage7,
                TevStage8,
                TevStage9,
                TevStage10,
                TevStage11,
                TevStage12,
                TevStage13,
                TevStage14,
                TevStage15
            }

            public enum KonstColorSel
            {
                KCSel_1 = 0x00,     // Constant 1.0
                KCSel_7_8 = 0x01,   // Constant 7/8
                KCSel_6_8 = 0x02,   // Constant 3/4
                KCSel_5_8 = 0x03,   // Constant 5/8
                KCSel_4_8 = 0x04,   // Constant 1/2
                KCSel_3_8 = 0x05,   // Constant 3/8
                KCSel_2_8 = 0x06,   // Constant 1/4
                KCSel_1_8 = 0x07,   // Constant 1/8

                KCSel_K0 = 0x0C,    // K0[RGB] Register
                KCSel_K1 = 0x0D,    // K1[RGB] Register
                KCSel_K2 = 0x0E,    // K2[RGB] Register
                KCSel_K3 = 0x0F,    // K3[RGB] Register
                KCSel_K0_R = 0x10,  // K0[RRR] Register
                KCSel_K1_R = 0x11,  // K1[RRR] Register
                KCSel_K2_R = 0x12,  // K2[RRR] Register
                KCSel_K3_R = 0x13,  // K3[RRR] Register
                KCSel_K0_G = 0x14,  // K0[GGG] Register
                KCSel_K1_G = 0x15,  // K1[GGG] Register
                KCSel_K2_G = 0x16,  // K2[GGG] Register
                KCSel_K3_G = 0x17,  // K3[GGG] Register
                KCSel_K0_B = 0x18,  // K0[BBB] Register
                KCSel_K1_B = 0x19,  // K1[BBB] Register
                KCSel_K2_B = 0x1A,  // K2[BBB] Register
                KCSel_K3_B = 0x1B,  // K3[BBB] Register
                KCSel_K0_A = 0x1C,  // K0[AAA] Register
                KCSel_K1_A = 0x1D,  // K1[AAA] Register
                KCSel_K2_A = 0x1E,  // K2[AAA] Register
                KCSel_K3_A = 0x1F   // K3[AAA] Register
            }
            public enum KonstAlphaSel
            {
                KASel_1 = 0x00,     // Constant 1.0
                KASel_7_8 = 0x01,   // Constant 7/8
                KASel_6_8 = 0x02,   // Constant 3/4
                KASel_5_8 = 0x03,   // Constant 5/8
                KASel_4_8 = 0x04,   // Constant 1/2
                KASel_3_8 = 0x05,   // Constant 3/8
                KASel_2_8 = 0x06,   // Constant 1/4
                KASel_1_8 = 0x07,   // Constant 1/8
                KASel_K0_R = 0x10,  // K0[R] Register
                KASel_K1_R = 0x11,  // K1[R] Register
                KASel_K2_R = 0x12,  // K2[R] Register
                KASel_K3_R = 0x13,  // K3[R] Register
                KASel_K0_G = 0x14,  // K0[G] Register
                KASel_K1_G = 0x15,  // K1[G] Register
                KASel_K2_G = 0x16,  // K2[G] Register
                KASel_K3_G = 0x17,  // K3[G] Register
                KASel_K0_B = 0x18,  // K0[B] Register
                KASel_K1_B = 0x19,  // K1[B] Register
                KASel_K2_B = 0x1A,  // K2[B] Register
                KASel_K3_B = 0x1B,  // K3[B] Register
                KASel_K0_A = 0x1C,  // K0[A] Register
                KASel_K1_A = 0x1D,  // K1[A] Register
                KASel_K2_A = 0x1E,  // K2[A] Register
                KASel_K3_A = 0x1F   // K3[A] Register
            }

            public enum Mat3OffsetIndex
            {
                MaterialData = 0,
                IndexData = 1,
                NameTable = 2,
                IndirectData = 3,
                CullMode = 4,
                MaterialColor = 5,
                ColorChannelCount = 6,
                ColorChannelData = 7,
                AmbientColorData = 8,
                LightData = 9,
                TexGenCount = 10,
                TexCoordData = 11,
                TexCoord2Data = 12,
                TexMatrixData = 13,
                TexMatrix2Data = 14,
                TexNoData = 15,
                TevOrderData = 16,
                TevColorData = 17,
                TevKColorData = 18,
                TevStageCount = 19,
                TevStageData = 20,
                TevSwapModeData = 21,
                TevSwapModeTable = 22,
                FogData = 23,
                AlphaCompareData = 24,
                BlendData = 25,
                ZModeData = 26,
                ZCompLoc = 27,
                DitherData = 28,
                NBTScaleData = 29
            }
        }

        //=====================================================================
    }
}
