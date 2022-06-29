using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK;
using static Hack.io.J3D.J3DGraph;

//Heavily based on the SuperBMD Library.
namespace Hack.io.BMD
{
    public partial class BMD
    {
        public class SHP1
        {
            public List<Shape> Shapes { get; private set; } = new List<Shape>();
            public List<int> RemapTable { get; private set; } = new List<int>();
            /// <summary>
            /// Get a shape with respect to the Remap table
            /// </summary>
            /// <param name="Index"></param>
            /// <returns></returns>
            public Shape this[int Index] { get => Shapes[RemapTable[Index]]; }

            private static readonly string Magic = "SHP1";

            public SHP1(Stream BMD)
            {
                int ChunkStart = (int)BMD.Position;
                if (!BMD.ReadString(4).Equals(Magic))
                    throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");

                int shp1Size = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);
                int ShapeEntryCount = BitConverter.ToInt16(BMD.ReadReverse(0, 2), 0);
                BMD.Position += 0x02;

                int shapeHeaderDataOffset = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);
                int shapeRemapTableOffset = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);
                int StringTableOffset = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);
                int attributeDataOffset = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);
                int DRW1IndexTableOffset = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);
                int primitiveDataOffset = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);
                int MatrixDataOffset = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);
                int PacketInfoDataOffset = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);


                // Remap table
                BMD.Seek(ChunkStart + shapeRemapTableOffset, SeekOrigin.Begin);
                int highestIndex = int.MinValue;
                for (int i = 0; i < ShapeEntryCount; i++)
                {
                    RemapTable.Add(BitConverter.ToInt16(BMD.ReadReverse(0, 2), 0));

                    if (RemapTable[i] > highestIndex)
                        highestIndex = RemapTable[i];
                }

                for (int SID = 0; SID < ShapeEntryCount; SID++)
                {
                    // Shapes can have different attributes for each shape. (ie: Some have only Position, while others have Pos & TexCoord, etc.) Each 
                    // shape (which has a consistent number of attributes) it is split into individual packets, which are a collection of geometric primitives.
                    // Each packet can have individual unique skinning data.
                    //      ~Probably LordNed
                    //
                    // . . .
                    //Why Nintendo why

                    BMD.Position = ChunkStart + shapeHeaderDataOffset + (0x28 * SID);
                    long ShapeStart = BMD.Position;
                    Shape CurrentShape = new Shape() { MatrixType = (DisplayFlags)BMD.ReadByte() };
                    BMD.Position++;
                    ushort PacketCount = BitConverter.ToUInt16(BMD.ReadReverse(0, 2), 0);
                    ushort batchAttributeOffset = BitConverter.ToUInt16(BMD.ReadReverse(0, 2), 0);
                    ushort firstMatrixIndex = BitConverter.ToUInt16(BMD.ReadReverse(0, 2), 0);
                    ushort firstPacketIndex = BitConverter.ToUInt16(BMD.ReadReverse(0, 2), 0);
                    BMD.Position += 0x02;
                    BoundingVolume shapeVol = new BoundingVolume(BMD);

                    ShapeVertexDescriptor Desc = new ShapeVertexDescriptor(BMD, BMD.Position = ChunkStart + attributeDataOffset + batchAttributeOffset);
                    CurrentShape.Bounds = shapeVol;
                    CurrentShape.Descriptor = Desc;
                    Shapes.Add(CurrentShape);

                    for (int PacketID = 0; PacketID < PacketCount; PacketID++)
                    {
                        Packet Pack = new Packet();

                        // The packets are all stored linearly and then they point to the specific size and offset of the data for this particular packet.
                        BMD.Position = ChunkStart + PacketInfoDataOffset + ((firstPacketIndex + PacketID) * 0x8); /* 0x8 is the size of one Packet entry */

                        int PacketSize = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);
                        int PacketOffset = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);

                        BMD.Position = ChunkStart +  MatrixDataOffset + (firstMatrixIndex + PacketID) * 0x08;
                        // 8 bytes long
                        Pack.DRW1MatrixID = BitConverter.ToInt16(BMD.ReadReverse(0, 2), 0);
                        short MatrixCount = BitConverter.ToInt16(BMD.ReadReverse(0, 2), 0);
                        int StartingMatrix = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);


                        BMD.Position = ChunkStart + DRW1IndexTableOffset + (StartingMatrix * 0x2);
                        int LastID = 0;
                        for (int m = 0; m < MatrixCount; m++)
                        {
                            Pack.MatrixIndices.Add(BitConverter.ToInt16(BMD.ReadReverse(0, 2), 0));
                            if (Pack.MatrixIndices[m] == -1)
                                Pack.MatrixIndices[m] = LastID;
                            else
                                LastID = Pack.MatrixIndices[m];
                        }
                        
                        Pack.ReadPrimitives(BMD, Desc, ChunkStart + primitiveDataOffset + PacketOffset, PacketSize);
                        CurrentShape.Packets.Add(Pack);
                    }
                }

                BMD.Position = ChunkStart + shp1Size;
            }

            public void SetVertexWeights(EVP1 envelopes, DRW1 drawList)
            {
                for (int i = 0; i < Shapes.Count; i++)
                {
                    for (int j = 0; j < Shapes[i].Packets.Count; j++)
                    {
                        foreach (Primitive prim in Shapes[i].Packets[j].Primitives)
                        {
                            foreach (Vertex vert in prim.Vertices)
                            {
                                if (Shapes[i].Descriptor.CheckAttribute(GXVertexAttribute.PositionMatrixIdx))
                                {
                                    int drw1Index = Shapes[i].Packets[j].MatrixIndices[(int)vert.PositionMatrixIDxIndex];
                                    int curPacketIndex = j;
                                    while (drw1Index == -1)
                                    {
                                        curPacketIndex--;
                                        drw1Index = Shapes[i].Packets[curPacketIndex].MatrixIndices[(int)vert.PositionMatrixIDxIndex];
                                    }

                                    if (drawList.IsPartialWeight[(int)drw1Index])
                                    {
                                        int evp1Index = drawList.TransformIndexTable[(int)drw1Index];
                                        vert.SetWeight(envelopes.Weights[evp1Index]);
                                    }
                                    else
                                    {
                                        EVP1.Weight vertWeight = new EVP1.Weight();
                                        vertWeight.AddWeight(1.0f, drawList.TransformIndexTable[(int)drw1Index]);
                                        vert.SetWeight(vertWeight);
                                    }
                                }
                                else
                                {
                                    EVP1.Weight vertWeight = new EVP1.Weight();
                                    vertWeight.AddWeight(1.0f, drawList.TransformIndexTable[Shapes[i].Packets[j].MatrixIndices[0]]);
                                    vert.SetWeight(vertWeight);
                                }
                            }
                        }
                    }
                }
            }

            public static int CountPackets(SHP1 Target)
            {
                int packetCount = 0;
                foreach (Shape shape in Target.Shapes)
                    packetCount += shape.Packets.Count;
                return packetCount;
            }

            public override string ToString() => $"SHP1: {Shapes.Count} Shapes";

            internal List<Vertex> GetAllUsedVertices()
            {
                List<Vertex> results = new List<Vertex>();
                for (int i = 0; i < Shapes.Count; i++)
                    results.AddRange(Shapes[i].GetAllUsedVertices());
                return results;
            }

            public void Write(Stream writer)
            {
                long start = writer.Position;

                List<byte> RemapTableData = new List<byte>();
                for (int i = 0; i < RemapTable.Count; i++)
                    RemapTableData.AddRange(BitConverter.GetBytes((short)RemapTable[i]).Reverse());

                int RemapTableOffset = 0x2C + (0x28 * Shapes.Count), NameTableOffset = 0, AttributeTableOffset, DRW1IndexTableOffset, PrimitiveDataOffset, MatrixDataOffset, PrimitiveLocationDataOffset;
                //In the event that a BMD/BDL with a Name Table for Shapes is found, the saving code will go here

                writer.WriteString("SHP1");
                writer.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for Section Size
                writer.WriteReverse(BitConverter.GetBytes((short)Shapes.Count), 0, 2);
                writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);

                writer.Write(new byte[4] { 0x00, 0x00, 0x00, 0x2C }, 0, 4); // ShapeDataOffset
                writer.WriteReverse(BitConverter.GetBytes(RemapTableOffset), 0, 4);
                writer.WriteReverse(BitConverter.GetBytes(NameTableOffset), 0, 4);
                writer.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for AttributeTableOffset
                writer.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for MatrixTableOffset
                writer.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for PrimitiveDataOffset
                writer.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for MatrixDataOffset
                writer.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for MatrixGroupTableOffset

                for (int SID = 0; SID < Shapes.Count; SID++)
                    Shapes[SID].Write(writer);

                writer.Write(RemapTableData.ToArray(), 0, RemapTableData.Count);

                AddPadding(writer, 4);
                AddPadding(writer, 32);

                AttributeTableOffset = (int)writer.Position;
                List<Tuple<ShapeVertexDescriptor, int>> descriptorOffsets = WriteShapeAttributeDescriptors(writer);

                DRW1IndexTableOffset = (int)writer.Position;
                List<Tuple<Packet, int>> packetMatrixOffsets = WritePacketMatrixIndices(writer);
                AddPadding(writer, 32);

                PrimitiveDataOffset = (int)writer.Position;
                List<Tuple<int, int>> PrimitiveOffsets = WritePrimitives(writer);
                AddPadding(writer, 32);

                MatrixDataOffset = (int)writer.Position;
                WriteMatrixData(writer, packetMatrixOffsets);

                PrimitiveLocationDataOffset = (int)writer.Position;
                foreach (Tuple<int, int> tup in PrimitiveOffsets)
                {
                    writer.WriteReverse(BitConverter.GetBytes(tup.Item1), 0, 4);
                    writer.WriteReverse(BitConverter.GetBytes(tup.Item2), 0, 4);
                }
                AddPadding(writer, 32);

                writer.Position = start + 0x2C;
                foreach (Shape shape in Shapes)
                {
                    writer.Position += 0x04;
                    writer.WriteReverse(BitConverter.GetBytes((short)descriptorOffsets.Find(x => x.Item1 == shape.Descriptor).Item2), 0, 2);
                    writer.WriteReverse(BitConverter.GetBytes((short)packetMatrixOffsets.IndexOf(packetMatrixOffsets.Find(x => x.Item1 == shape.Packets[0]))), 0, 2);
                    writer.WriteReverse(BitConverter.GetBytes((short)packetMatrixOffsets.IndexOf(packetMatrixOffsets.Find(x => x.Item1 == shape.Packets[0]))), 0, 2);
                    writer.Position += 0x1E;
                }

                writer.Position = start + 0x04;
                writer.WriteReverse(BitConverter.GetBytes((int)(writer.Length - start)), 0, 4);
                writer.Position += 0x10;
                writer.WriteReverse(BitConverter.GetBytes((int)(AttributeTableOffset - start)), 0, 4);
                writer.WriteReverse(BitConverter.GetBytes((int)(DRW1IndexTableOffset - start)), 0, 4);
                writer.WriteReverse(BitConverter.GetBytes((int)(PrimitiveDataOffset - start)), 0, 4);
                writer.WriteReverse(BitConverter.GetBytes((int)(MatrixDataOffset - start)), 0, 4);
                writer.WriteReverse(BitConverter.GetBytes((int)(PrimitiveLocationDataOffset - start)), 0, 4);
                writer.Position = writer.Length;
            }

            private List<Tuple<ShapeVertexDescriptor, int>> WriteShapeAttributeDescriptors(Stream writer)
            {
                List<Tuple<ShapeVertexDescriptor, int>> outList = new List<Tuple<ShapeVertexDescriptor, int>>();
                List<ShapeVertexDescriptor> written = new List<ShapeVertexDescriptor>();

                long start = writer.Position;

                foreach (Shape shape in Shapes)
                {
                    if (written.Any(SVD => SVD == shape.Descriptor))
                        continue;
                    else
                    {
                        outList.Add(new Tuple<ShapeVertexDescriptor, int>(shape.Descriptor, (int)(writer.Position - start)));
                        shape.Descriptor.Write(writer);
                        written.Add(shape.Descriptor);
                    }
                }
                return outList;
            }

            private List<Tuple<Packet, int>> WritePacketMatrixIndices(Stream writer)
            {
                List<Tuple<Packet, int>> outList = new List<Tuple<Packet, int>>();

                int indexOffset = 0;
                foreach (Shape shape in Shapes)
                {
                    foreach (Packet pack in shape.Packets)
                    {
                        outList.Add(new Tuple<Packet, int>(pack, indexOffset));

                        int Last = -1;
                        for (int i = 0; i < pack.MatrixIndices.Count; i++)
                        {
                            if (i > 0 && pack.MatrixIndices[i] == Last)
                                writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                            else
                            {
                                writer.WriteReverse(BitConverter.GetBytes((ushort)pack.MatrixIndices[i]), 0, 2);
                                Last = pack.MatrixIndices[i];
                            }
                            indexOffset++;
                        }
                    }
                }

                return outList;
            }

            private List<Tuple<int, int>> WritePrimitives(Stream writer)
            {
                List<Tuple<int, int>> outList = new List<Tuple<int, int>>();

                long start = writer.Position;

                foreach (Shape shape in Shapes)
                {
                    foreach (Packet pack in shape.Packets)
                    {
                        int offset = (int)(writer.Position - start);

                        foreach (Primitive prim in pack.Primitives)
                        {
                            prim.Write(writer, shape.Descriptor);
                        }

                        writer.PadTo(32);

                        outList.Add(new Tuple<int, int>((int)((writer.Position - start) - offset), offset));
                    }
                }

                return outList;
            }

            private void WriteMatrixData(Stream writer, List<Tuple<Packet, int>> MatrixOffsets)
            {
                int StartingIndex = 0;
                for (int i = 0; i < Shapes.Count; i++)
                {
                    for (int y = 0; y < Shapes[i].Packets.Count; y++)
                    {
                        writer.WriteReverse(BitConverter.GetBytes(Shapes[i].Packets[y].DRW1MatrixID), 0, 2);
                        writer.WriteReverse(BitConverter.GetBytes((short)Shapes[i].Packets[y].MatrixIndices.Count), 0, 2);
                        writer.WriteReverse(BitConverter.GetBytes(StartingIndex), 0, 4);
                        StartingIndex += Shapes[i].Packets[y].MatrixIndices.Count;
                    }
                }
            }

            public class Shape
            {
                public ShapeVertexDescriptor Descriptor { get; set; } = new ShapeVertexDescriptor();

                public DisplayFlags MatrixType { get; set; } = DisplayFlags.MultiMatrix;
                public BoundingVolume Bounds { get; set; } = new BoundingVolume();

                public List<Packet> Packets { get; set; } = new List<Packet>();
                
                // The maximum number of unique vertex weights that can be in a single shape packet without causing visual errors.
                private const int MaxMatricesPerPacket = 10;

                public Shape()
                {
                }

                public Shape(DisplayFlags matrixType) : this()
                {
                    MatrixType = matrixType;
                }

                public Shape(ShapeVertexDescriptor desc, BoundingVolume bounds, List<Packet> prims, DisplayFlags matrixType)
                {
                    Descriptor = desc;
                    Bounds = bounds;
                    Packets = prims;
                    MatrixType = matrixType;
                }

                internal List<Vertex> GetAllUsedVertices()
                {
                    List<Vertex> results = new List<Vertex>();
                    for (int i = 0; i < Packets.Count; i++)
                    {
                        for (int x = 0; x < Packets[i].Primitives.Count; x++)
                        {
                            results.AddRange(Packets[i].Primitives[x].Vertices);
                        }
                    }
                    return results;
                }

                public void Write(Stream writer)
                {
                    writer.WriteByte((byte)MatrixType);
                    writer.WriteByte(0xFF);
                    writer.WriteReverse(BitConverter.GetBytes((short)Packets.Count), 0, 2);
                    writer.Write(new byte[2] { 0xDD, 0xDD }, 0, 2); // Placeholder for descriptor offset
                    writer.Write(new byte[2] { 0xDD, 0xDD }, 0, 2); // Placeholder for starting packet index
                    writer.Write(new byte[2] { 0xDD, 0xDD }, 0, 2); // Placeholder for starting packet matrix index offset
                    writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                    Bounds.Write(writer);
                }

                public override string ToString() => $"Shape: {MatrixType.ToString()}";
            }

            public class ShapeVertexDescriptor
            {
                public SortedDictionary<GXVertexAttribute, Tuple<VertexInputType, int>> Attributes { get; private set; } = new SortedDictionary<GXVertexAttribute, Tuple<VertexInputType, int>>();

                public ShapeVertexDescriptor() { }

                public ShapeVertexDescriptor(Stream reader, long offset)
                {
                    Attributes = new SortedDictionary<GXVertexAttribute, Tuple<VertexInputType, int>>();
                    reader.Position = offset;

                    int index = 0;
                    GXVertexAttribute attrib = (GXVertexAttribute)BitConverter.ToInt32(reader.ReadReverse(0, 4), 0);

                    while (attrib != GXVertexAttribute.Null)
                    {
                        Attributes.Add(attrib, new Tuple<VertexInputType, int>((VertexInputType)BitConverter.ToInt32(reader.ReadReverse(0, 4), 0), index));

                        index++;
                        attrib = (GXVertexAttribute)BitConverter.ToInt32(reader.ReadReverse(0, 4), 0);
                    }
                }

                public bool CheckAttribute(GXVertexAttribute attribute) => Attributes.ContainsKey(attribute);

                public void SetAttribute(GXVertexAttribute attribute, VertexInputType inputType, int vertexIndex)
                {
                    if (CheckAttribute(attribute))
                        throw new Exception($"Attribute \"{ attribute }\" is already in the vertex descriptor!");

                    Attributes.Add(attribute, new Tuple<VertexInputType, int>(inputType, vertexIndex));
                }

                public List<GXVertexAttribute> GetActiveAttributes() => new List<GXVertexAttribute>(Attributes.Keys);

                public int GetAttributeIndex(GXVertexAttribute attribute)
                {
                    if (CheckAttribute(attribute))
                        return Attributes[attribute].Item2;
                    else
                        throw new ArgumentException("attribute");
                }

                public VertexInputType GetAttributeType(GXVertexAttribute attribute)
                {
                    if (CheckAttribute(attribute))
                        return Attributes[attribute].Item1;
                    else
                        throw new ArgumentException("attribute");
                }

                public void Write(Stream writer)
                {
                    if (CheckAttribute(GXVertexAttribute.PositionMatrixIdx))
                    {
                        writer.WriteReverse(BitConverter.GetBytes((int)GXVertexAttribute.PositionMatrixIdx), 0, 4);
                        writer.WriteReverse(BitConverter.GetBytes((int)Attributes[GXVertexAttribute.PositionMatrixIdx].Item1), 0, 4);
                    }

                    if (CheckAttribute(GXVertexAttribute.Position))
                    {
                        writer.WriteReverse(BitConverter.GetBytes((int)GXVertexAttribute.Position), 0, 4);
                        writer.WriteReverse(BitConverter.GetBytes((int)Attributes[GXVertexAttribute.Position].Item1), 0, 4);
                    }

                    if (CheckAttribute(GXVertexAttribute.Normal))
                    {
                        writer.WriteReverse(BitConverter.GetBytes((int)GXVertexAttribute.Normal), 0, 4);
                        writer.WriteReverse(BitConverter.GetBytes((int)Attributes[GXVertexAttribute.Normal].Item1), 0, 4);
                    }

                    if (CheckAttribute(GXVertexAttribute.Color0))
                    {
                        writer.WriteReverse(BitConverter.GetBytes((int)GXVertexAttribute.Color0), 0, 4);
                        writer.WriteReverse(BitConverter.GetBytes((int)Attributes[GXVertexAttribute.Color0].Item1), 0, 4);
                    }

                    if (CheckAttribute(GXVertexAttribute.Color1))
                    {
                        writer.WriteReverse(BitConverter.GetBytes((int)GXVertexAttribute.Color1), 0, 4);
                        writer.WriteReverse(BitConverter.GetBytes((int)Attributes[GXVertexAttribute.Color1].Item1), 0, 4);
                    }

                    if (CheckAttribute(GXVertexAttribute.Tex0))
                    {
                        writer.WriteReverse(BitConverter.GetBytes((int)GXVertexAttribute.Tex0), 0, 4);
                        writer.WriteReverse(BitConverter.GetBytes((int)Attributes[GXVertexAttribute.Tex0].Item1), 0, 4);
                    }

                    if (CheckAttribute(GXVertexAttribute.Tex1))
                    {
                        writer.WriteReverse(BitConverter.GetBytes((int)GXVertexAttribute.Tex1), 0, 4);
                        writer.WriteReverse(BitConverter.GetBytes((int)Attributes[GXVertexAttribute.Tex1].Item1), 0, 4);
                    }

                    if (CheckAttribute(GXVertexAttribute.Tex2))
                    {
                        writer.WriteReverse(BitConverter.GetBytes((int)GXVertexAttribute.Tex2), 0, 4);
                        writer.WriteReverse(BitConverter.GetBytes((int)Attributes[GXVertexAttribute.Tex2].Item1), 0, 4);
                    }

                    if (CheckAttribute(GXVertexAttribute.Tex3))
                    {
                        writer.WriteReverse(BitConverter.GetBytes((int)GXVertexAttribute.Tex3), 0, 4);
                        writer.WriteReverse(BitConverter.GetBytes((int)Attributes[GXVertexAttribute.Tex3].Item1), 0, 4);
                    }

                    if (CheckAttribute(GXVertexAttribute.Tex4))
                    {
                        writer.WriteReverse(BitConverter.GetBytes((int)GXVertexAttribute.Tex4), 0, 4);
                        writer.WriteReverse(BitConverter.GetBytes((int)Attributes[GXVertexAttribute.Tex4].Item1), 0, 4);
                    }

                    if (CheckAttribute(GXVertexAttribute.Tex5))
                    {
                        writer.WriteReverse(BitConverter.GetBytes((int)GXVertexAttribute.Tex5), 0, 4);
                        writer.WriteReverse(BitConverter.GetBytes((int)Attributes[GXVertexAttribute.Tex5].Item1), 0, 4);
                    }

                    if (CheckAttribute(GXVertexAttribute.Tex6))
                    {
                        writer.WriteReverse(BitConverter.GetBytes((int)GXVertexAttribute.Tex6), 0, 4);
                        writer.WriteReverse(BitConverter.GetBytes((int)Attributes[GXVertexAttribute.Tex6].Item1), 0, 4);
                    }

                    if (CheckAttribute(GXVertexAttribute.Tex7))
                    {
                        writer.WriteReverse(BitConverter.GetBytes((int)GXVertexAttribute.Tex7), 0, 4);
                        writer.WriteReverse(BitConverter.GetBytes((int)Attributes[GXVertexAttribute.Tex7].Item1), 0, 4);
                    }

                    // Null attribute
                    writer.Write(new byte[4] { 0x00, 0x00, 0x00, 0xFF }, 0, 4);
                    writer.Write(new byte[4], 0, 4);
                }

                public override string ToString() => $"Descriptor: {Attributes.Count} Attributes";

                public override bool Equals(object obj)
                {
                    if (!(obj is ShapeVertexDescriptor descriptor) || Attributes.Count != descriptor.Attributes.Count)
                        return false;
                    foreach (KeyValuePair<GXVertexAttribute, Tuple<VertexInputType, int>> item in Attributes)
                    {
                        if (!descriptor.CheckAttribute(item.Key) || Attributes[item.Key].Item1 != descriptor.Attributes[item.Key].Item1 || Attributes[item.Key].Item2 != descriptor.Attributes[item.Key].Item2)
                            return false;
                    }
                    return true;
                }

                public override int GetHashCode()
                {
                    return -2135698220 + EqualityComparer<SortedDictionary<GXVertexAttribute, Tuple<VertexInputType, int>>>.Default.GetHashCode(Attributes);
                }

                public static bool operator ==(ShapeVertexDescriptor descriptor1, ShapeVertexDescriptor descriptor2)
                {
                    return descriptor1.Equals(descriptor2);
                }

                public static bool operator !=(ShapeVertexDescriptor descriptor1, ShapeVertexDescriptor descriptor2)
                {
                    return !(descriptor1 == descriptor2);
                }
            }

            public class Packet
            {
                public List<Primitive> Primitives { get; private set; }
                public short DRW1MatrixID { get; set; }
                public List<int> MatrixIndices { get; private set; }

                public Packet()
                {
                    Primitives = new List<Primitive>();
                    MatrixIndices = new List<int>();
                }

                public void ReadPrimitives(Stream reader, ShapeVertexDescriptor desc, long Location, int Size)
                {
                    reader.Position = Location;

                    while (true)
                    {
                        GXPrimitiveType type = (GXPrimitiveType)reader.PeekByte();
                        if (type == 0 || reader.Position >= Size + Location)
                            break;
                        Primitive prim = new Primitive(reader, desc);
                        Primitives.Add(prim);
                    }
                }

                public override string ToString() => $"Packet: {Primitives.Count} Primitives, {MatrixIndices.Count} MatrixIndicies";
            }

            public class Primitive
            {
                public GXPrimitiveType PrimitiveType { get; private set; }
                public List<Vertex> Vertices { get; private set; }

                public Primitive()
                {
                    PrimitiveType = GXPrimitiveType.Lines;
                    Vertices = new List<Vertex>();
                }

                public Primitive(GXPrimitiveType primType)
                {
                    PrimitiveType = primType;
                    Vertices = new List<Vertex>();
                }

                public Primitive(Stream reader, ShapeVertexDescriptor activeAttribs)
                {
                    Vertices = new List<Vertex>();

                    PrimitiveType = (GXPrimitiveType)(reader.ReadByte() & 0xF8);
                    int vertCount = BitConverter.ToInt16(reader.ReadReverse(0, 2), 0);

                    for (int i = 0; i < vertCount; i++)
                    {
                        Vertex vert = new Vertex();

                        foreach (GXVertexAttribute attrib in activeAttribs.Attributes.Keys)
                        {
                            switch (activeAttribs.GetAttributeType(attrib))
                            {
                                case VertexInputType.Direct:
                                    vert.SetAttributeIndex(attrib, attrib == GXVertexAttribute.PositionMatrixIdx ? (uint)(reader.ReadByte() / 3) : (byte)reader.ReadByte());
                                    break;
                                case VertexInputType.Index8:
                                    vert.SetAttributeIndex(attrib, (uint)reader.ReadByte());
                                    break;
                                case VertexInputType.Index16:
                                    ushort temp = BitConverter.ToUInt16(reader.ReadReverse(0, 2), 0);
                                    vert.SetAttributeIndex(attrib, temp);
                                    break;
                                case VertexInputType.None:
                                    throw new Exception("Found \"None\" as vertex input type in Primitive(reader, activeAttribs)!");
                            }
                        }

                        Vertices.Add(vert);
                    }
                }

                public void Write(Stream writer, ShapeVertexDescriptor desc)
                {
                    writer.WriteByte((byte)PrimitiveType);
                    writer.WriteReverse(BitConverter.GetBytes((short)Vertices.Count), 0, 2);

                    foreach (Vertex vert in Vertices)
                        vert.Write(writer, desc);
                }

                public override string ToString() => $"{PrimitiveType.ToString()}, {Vertices.Count} Vertices";
            }

            public class Vertex
            {
                public uint PositionMatrixIDxIndex { get; private set; }
                public uint PositionIndex { get; private set; }
                public uint NormalIndex { get; private set; }
                public uint Color0Index { get; private set; }
                public uint Color1Index { get; private set; }
                public uint TexCoord0Index { get; private set; }
                public uint TexCoord1Index { get; private set; }
                public uint TexCoord2Index { get; private set; }
                public uint TexCoord3Index { get; private set; }
                public uint TexCoord4Index { get; private set; }
                public uint TexCoord5Index { get; private set; }
                public uint TexCoord6Index { get; private set; }
                public uint TexCoord7Index { get; private set; }

                public uint Tex0MtxIndex { get; private set; }
                public uint Tex1MtxIndex { get; private set; }
                public uint Tex2MtxIndex { get; private set; }
                public uint Tex3MtxIndex { get; private set; }
                public uint Tex4MtxIndex { get; private set; }
                public uint Tex5MtxIndex { get; private set; }
                public uint Tex6MtxIndex { get; private set; }
                public uint Tex7MtxIndex { get; private set; }

                public uint PositionMatrixIndex { get; set; }
                public uint NormalMatrixIndex { get; set; }

                public EVP1.Weight VertexWeight { get; private set; } = new EVP1.Weight();

                public Vertex() { }

                public Vertex(Vertex src)
                {
                    // The position matrix index index is specific to the packet the vertex is in.
                    // So if copying a vertex across different packets, this value will be wrong and it needs to be recalculated manually.
                    PositionMatrixIDxIndex = src.PositionMatrixIDxIndex;

                    PositionIndex = src.PositionIndex;
                    NormalIndex = src.NormalIndex;
                    Color0Index = src.Color0Index;
                    Color1Index = src.Color1Index;
                    TexCoord0Index = src.TexCoord0Index;
                    TexCoord1Index = src.TexCoord1Index;
                    TexCoord2Index = src.TexCoord2Index;
                    TexCoord3Index = src.TexCoord3Index;
                    TexCoord4Index = src.TexCoord4Index;
                    TexCoord5Index = src.TexCoord5Index;
                    TexCoord6Index = src.TexCoord6Index;
                    TexCoord7Index = src.TexCoord7Index;

                    Tex0MtxIndex = src.Tex0MtxIndex;
                    Tex1MtxIndex = src.Tex1MtxIndex;
                    Tex2MtxIndex = src.Tex2MtxIndex;
                    Tex3MtxIndex = src.Tex3MtxIndex;
                    Tex4MtxIndex = src.Tex4MtxIndex;
                    Tex5MtxIndex = src.Tex5MtxIndex;
                    Tex6MtxIndex = src.Tex6MtxIndex;
                    Tex7MtxIndex = src.Tex7MtxIndex;

                    VertexWeight = src.VertexWeight;
                }

                public uint GetAttributeIndex(GXVertexAttribute attribute)
                {
                    switch (attribute)
                    {
                        case GXVertexAttribute.PositionMatrixIdx:
                            return PositionMatrixIDxIndex;
                        case GXVertexAttribute.Position:
                            return PositionIndex;
                        case GXVertexAttribute.Normal:
                            return NormalIndex;
                        case GXVertexAttribute.Color0:
                            return Color0Index;
                        case GXVertexAttribute.Color1:
                            return Color1Index;
                        case GXVertexAttribute.Tex0:
                            return TexCoord0Index;
                        case GXVertexAttribute.Tex1:
                            return TexCoord1Index;
                        case GXVertexAttribute.Tex2:
                            return TexCoord2Index;
                        case GXVertexAttribute.Tex3:
                            return TexCoord3Index;
                        case GXVertexAttribute.Tex4:
                            return TexCoord4Index;
                        case GXVertexAttribute.Tex5:
                            return TexCoord5Index;
                        case GXVertexAttribute.Tex6:
                            return TexCoord6Index;
                        case GXVertexAttribute.Tex7:
                            return TexCoord7Index;
                        case GXVertexAttribute.Tex0Mtx:
                            return Tex0MtxIndex;
                        case GXVertexAttribute.Tex1Mtx:
                            return Tex1MtxIndex;
                        case GXVertexAttribute.Tex2Mtx:
                            return Tex2MtxIndex;
                        case GXVertexAttribute.Tex3Mtx:
                            return Tex3MtxIndex;
                        case GXVertexAttribute.Tex4Mtx:
                            return Tex4MtxIndex;
                        case GXVertexAttribute.Tex5Mtx:
                            return Tex5MtxIndex;
                        case GXVertexAttribute.Tex6Mtx:
                            return Tex6MtxIndex;
                        case GXVertexAttribute.Tex7Mtx:
                            return Tex7MtxIndex;
                        default:
                            throw new ArgumentException(String.Format("attribute {0}", attribute));
                    }
                }

                public void SetAttributeIndex(GXVertexAttribute attribute, uint index)
                {
                    switch (attribute)
                    {
                        case GXVertexAttribute.PositionMatrixIdx:
                            PositionMatrixIDxIndex = index;
                            break;
                        case GXVertexAttribute.Position:
                            PositionIndex = index;
                            break;
                        case GXVertexAttribute.Normal:
                            NormalIndex = index;
                            break;
                        case GXVertexAttribute.Color0:
                            Color0Index = index;
                            break;
                        case GXVertexAttribute.Color1:
                            Color1Index = index;
                            break;
                        case GXVertexAttribute.Tex0:
                            TexCoord0Index = index;
                            break;
                        case GXVertexAttribute.Tex1:
                            TexCoord1Index = index;
                            break;
                        case GXVertexAttribute.Tex2:
                            TexCoord2Index = index;
                            break;
                        case GXVertexAttribute.Tex3:
                            TexCoord3Index = index;
                            break;
                        case GXVertexAttribute.Tex4:
                            TexCoord4Index = index;
                            break;
                        case GXVertexAttribute.Tex5:
                            TexCoord5Index = index;
                            break;
                        case GXVertexAttribute.Tex6:
                            TexCoord6Index = index;
                            break;
                        case GXVertexAttribute.Tex7:
                            TexCoord7Index = index;
                            break;
                        case GXVertexAttribute.Tex0Mtx:
                            Tex0MtxIndex = index;
                            break;
                        case GXVertexAttribute.Tex1Mtx:
                            Tex1MtxIndex = index;
                            break;
                        case GXVertexAttribute.Tex2Mtx:
                            Tex2MtxIndex = index;
                            break;
                        case GXVertexAttribute.Tex3Mtx:
                            Tex3MtxIndex = index;
                            break;
                        case GXVertexAttribute.Tex4Mtx:
                            Tex4MtxIndex = index;
                            break;
                        case GXVertexAttribute.Tex5Mtx:
                            Tex5MtxIndex = index;
                            break;
                        case GXVertexAttribute.Tex6Mtx:
                            Tex6MtxIndex = index;
                            break;
                        case GXVertexAttribute.Tex7Mtx:
                            Tex7MtxIndex = index;
                            break;
                        default:
                            //Modification: Should not be so important in our case.
                            //throw new ArgumentException(String.Format("attribute {0}", attribute));
                            Console.WriteLine(String.Format("attribute {0}", attribute));
                            break;
                    }
                }

                public void SetWeight(EVP1.Weight weight)
                {
                    VertexWeight = weight;
                }

                public void Write(Stream writer, ShapeVertexDescriptor desc)
                {
                    if (desc.CheckAttribute(GXVertexAttribute.PositionMatrixIdx))
                    {
                        WriteAttributeIndex(writer, PositionMatrixIDxIndex * 3, desc.Attributes[GXVertexAttribute.PositionMatrixIdx].Item1);
                    }

                    if (desc.CheckAttribute(GXVertexAttribute.Position))
                    {
                        WriteAttributeIndex(writer, PositionIndex, desc.Attributes[GXVertexAttribute.Position].Item1);
                    }

                    if (desc.CheckAttribute(GXVertexAttribute.Normal))
                    {
                        WriteAttributeIndex(writer, NormalIndex, desc.Attributes[GXVertexAttribute.Normal].Item1);
                    }

                    if (desc.CheckAttribute(GXVertexAttribute.Color0))
                    {
                        WriteAttributeIndex(writer, Color0Index, desc.Attributes[GXVertexAttribute.Color0].Item1);
                    }

                    if (desc.CheckAttribute(GXVertexAttribute.Color1))
                    {
                        WriteAttributeIndex(writer, Color1Index, desc.Attributes[GXVertexAttribute.Color1].Item1);
                    }

                    if (desc.CheckAttribute(GXVertexAttribute.Tex0))
                    {
                        WriteAttributeIndex(writer, TexCoord0Index, desc.Attributes[GXVertexAttribute.Tex0].Item1);
                    }

                    if (desc.CheckAttribute(GXVertexAttribute.Tex1))
                    {
                        WriteAttributeIndex(writer, TexCoord1Index, desc.Attributes[GXVertexAttribute.Tex1].Item1);
                    }

                    if (desc.CheckAttribute(GXVertexAttribute.Tex2))
                    {
                        WriteAttributeIndex(writer, TexCoord2Index, desc.Attributes[GXVertexAttribute.Tex2].Item1);
                    }

                    if (desc.CheckAttribute(GXVertexAttribute.Tex3))
                    {
                        WriteAttributeIndex(writer, TexCoord3Index, desc.Attributes[GXVertexAttribute.Tex3].Item1);
                    }

                    if (desc.CheckAttribute(GXVertexAttribute.Tex4))
                    {
                        WriteAttributeIndex(writer, TexCoord4Index, desc.Attributes[GXVertexAttribute.Tex4].Item1);
                    }

                    if (desc.CheckAttribute(GXVertexAttribute.Tex5))
                    {
                        WriteAttributeIndex(writer, TexCoord5Index, desc.Attributes[GXVertexAttribute.Tex5].Item1);
                    }

                    if (desc.CheckAttribute(GXVertexAttribute.Tex6))
                    {
                        WriteAttributeIndex(writer, TexCoord6Index, desc.Attributes[GXVertexAttribute.Tex6].Item1);
                    }

                    if (desc.CheckAttribute(GXVertexAttribute.Tex7))
                    {
                        WriteAttributeIndex(writer, TexCoord7Index, desc.Attributes[GXVertexAttribute.Tex7].Item1);
                    }
                }

                private void WriteAttributeIndex(Stream writer, uint value, VertexInputType type)
                {
                    switch (type)
                    {
                        case VertexInputType.Direct:
                        case VertexInputType.Index8:
                            writer.WriteByte((byte)value);
                            break;
                        case VertexInputType.Index16:
                            writer.WriteReverse(BitConverter.GetBytes((short)value), 0, 2);
                            break;
                        case VertexInputType.None:
                        default:
                            throw new ArgumentException("vertex input type");
                    }
                }
            }

            public class BoundingVolume
            {
                public float SphereRadius { get; private set; }
                public Vector3 MinBounds { get; private set; }
                public Vector3 MaxBounds { get; private set; }

                public Vector3 Center => (MaxBounds + MinBounds) / 2;

                public BoundingVolume()
                {
                    MinBounds = new Vector3();
                    MaxBounds = new Vector3();
                }

                public BoundingVolume(Stream BMD)
                {
                    SphereRadius = BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0);

                    MinBounds = new Vector3(BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0), BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0), BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0));
                    MaxBounds = new Vector3(BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0), BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0), BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0));
                }

                public void GetBoundsValues(List<Vector3> positions)
                {
                    float minX = float.MaxValue;
                    float minY = float.MaxValue;
                    float minZ = float.MaxValue;

                    float maxX = float.MinValue;
                    float maxY = float.MinValue;
                    float maxZ = float.MinValue;

                    foreach (Vector3 vec in positions)
                    {
                        if (vec.X > maxX)
                            maxX = vec.X;
                        if (vec.Y > maxY)
                            maxY = vec.Y;
                        if (vec.Z > maxZ)
                            maxZ = vec.Z;

                        if (vec.X < minX)
                            minX = vec.X;
                        if (vec.Y < minY)
                            minY = vec.Y;
                        if (vec.Z < minZ)
                            minZ = vec.Z;
                    }

                    MinBounds = new Vector3(minX, minY, minZ);
                    MaxBounds = new Vector3(maxX, maxY, maxZ);
                    SphereRadius = (MaxBounds - Center).Length;
                }

                public void Write(Stream writer)
                {
                    writer.WriteReverse(BitConverter.GetBytes(SphereRadius), 0, 4);
                    writer.WriteReverse(BitConverter.GetBytes(MinBounds.X), 0, 4);
                    writer.WriteReverse(BitConverter.GetBytes(MinBounds.Y), 0, 4);
                    writer.WriteReverse(BitConverter.GetBytes(MinBounds.Z), 0, 4);
                    writer.WriteReverse(BitConverter.GetBytes(MaxBounds.X), 0, 4);
                    writer.WriteReverse(BitConverter.GetBytes(MaxBounds.Y), 0, 4);
                    writer.WriteReverse(BitConverter.GetBytes(MaxBounds.Z), 0, 4);
                }

                public override string ToString() => $"Min: {MinBounds.ToString()}, Max: {MaxBounds.ToString()}, Radius: {SphereRadius.ToString()}, Center: {Center.ToString()}";
            }
        }

        //=====================================================================
    }
}
