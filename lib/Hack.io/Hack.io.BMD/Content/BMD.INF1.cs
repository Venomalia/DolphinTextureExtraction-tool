using AuroraLip.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//Heavily based on the SuperBMD Library.
namespace Hack.io.BMD
{
    public partial class BMD
    {
        public class INF1 : IEnumerable
        {
            public Node Root { get; set; } = null;
            public J3DLoadFlags ScalingRule { get; set; }

            private static readonly string Magic = "INF1";


            public INF1() { }
            public INF1(Stream BMD, out int VertexCount)
            {
                long ChunkStart = BMD.Position;
                if (!BMD.ReadString(4).Equals(Magic))
                    throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");

                int ChunkSize = BitConverter.ToInt32(BMD.ReadBigEndian(0, 4), 0);
                ScalingRule = (J3DLoadFlags)BitConverter.ToInt16(BMD.ReadBigEndian(0, 2), 0);
                BMD.Position += 0x02;
                BMD.Position += 0x04;
                VertexCount = BitConverter.ToInt32(BMD.ReadBigEndian(0, 4), 0);
                int HierarchyOffset = BitConverter.ToInt32(BMD.ReadBigEndian(0, 4), 0);
                BMD.Position = ChunkStart + HierarchyOffset;

                Node parent = new Node(BMD, null);
                Node node = null;

                Root = parent;
                do
                {
                    node = new Node(BMD, parent);

                    if (node.Type == NodeType.OpenChild)
                    {
                        Node newNode = new Node(BMD, parent);
                        parent.Children.Add(newNode);
                        parent = newNode;
                    }
                    else if (node.Type == NodeType.CloseChild)
                        parent = parent.Parent;
                    else if (node.Type != NodeType.End)
                    {
                        parent.Parent.Children.Add(node);
                        node.SetParent(parent.Parent);
                        parent = node;
                    }

                } while (node.Type != NodeType.End);

                BMD.Position = ChunkStart + ChunkSize;
            }

            public void Write(Stream writer, SHP1 ShapesForMatrixGroupCount, VTX1 VerticiesForVertexCount)
            {
                long start = writer.Position;

                writer.WriteString("INF1");
                writer.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for section size
                writer.WriteBigEndian(BitConverter.GetBytes((short)ScalingRule), 0, 2);
                writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);

                writer.WriteBigEndian(BitConverter.GetBytes(SHP1.CountPackets(ShapesForMatrixGroupCount)), 0, 4); // Number of packets
                writer.WriteBigEndian(BitConverter.GetBytes(VerticiesForVertexCount.Attributes.Positions.Count), 0, 4); // Number of vertex positions
                writer.WriteBigEndian(BitConverter.GetBytes(24), 0, 4);

                Root.Write(writer);

                writer.WriteBigEndian(BitConverter.GetBytes((short)0x0000), 0, 2);
                writer.WriteBigEndian(BitConverter.GetBytes((short)0x0000), 0, 2);

                writer.AddPadding(32, Padding);

                long end = writer.Position;
                writer.Position = start+4;
                writer.WriteBigEndian(BitConverter.GetBytes((int)(end - start)), 0, 4);
                writer.Position = end;
            }

            public class Node
            {
                public Node Parent { get; set; }

                public NodeType Type { get; set; }
                public int Index { get; set; }
                public List<Node> Children { get; set; } = new List<Node>();

                public Node()
                {
                    Parent = null;
                    Type = NodeType.End;
                    Index = 0;
                }

                public Node(Stream BMD, Node parent)
                {
                    Parent = parent;
                    Type = (NodeType)BitConverter.ToInt16(BMD.ReadBigEndian(0, 2), 0);
                    Index = BitConverter.ToInt16(BMD.ReadBigEndian(0, 2), 0);
                }

                public Node(NodeType type, int index, Node parent)
                {
                    Type = type;
                    Index = index;
                    Parent = parent;

                    if (Parent != null)
                        Parent.Children.Add(this);

                    Children = new List<Node>();
                }

                public void Write(Stream BMD)
                {
                    BMD.WriteBigEndian(BitConverter.GetBytes((short)Type), 0, 2);
                    BMD.WriteBigEndian(BitConverter.GetBytes((short)Index), 0, 2);
                    if (Children.Count > 0)
                    {
                        BMD.WriteBigEndian(BitConverter.GetBytes((short)0x0001), 0, 2);
                        BMD.WriteBigEndian(BitConverter.GetBytes((short)0x0000), 0, 2);
                    }
                    for (int i = 0; i < Children.Count; i++)
                    {
                        Children[i].Write(BMD);
                    }
                    if (Children.Count > 0)
                    {
                        BMD.WriteBigEndian(BitConverter.GetBytes((short)0x0002), 0, 2);
                        BMD.WriteBigEndian(BitConverter.GetBytes((short)0x0000), 0, 2);
                    }
                }

                public void SetParent(Node parent)
                {
                    Parent = parent;
                }

                public override string ToString()
                {
                    return $"{ Type } : { Index }";
                }
            }

            public int FetchMaterialIndex(int ShapeID)
            {
                return Search(Root, ShapeID, NodeType.Shape, NodeType.Material);
            }

            private int Search(Node Root, int Index, NodeType IndexType, NodeType SearchType)
            {
                if (Root.Type == IndexType && Root.Index == Index)
                {
                    switch (IndexType)
                    {
                        case NodeType.Joint:
                            break;
                        case NodeType.Material:
                            break;
                        case NodeType.Shape:
                            switch (SearchType)
                            {
                                case NodeType.Joint:
                                    break;
                                case NodeType.Material:
                                    return Root.Parent.Index;
                                case NodeType.Shape:
                                    break;
                                default:
                                    throw new Exception("Bruh Moment!!");
                            }
                            break;
                        default:
                            throw new Exception("Bruh Moment!!");
                    }
                    return -1;
                }
                else if (Root.Children.Count > 0)
                {
                    for (int i = 0; i < Root.Children.Count; i++)
                    {
                        int value = Search(Root.Children[i], Index, IndexType, SearchType);
                        if (value != -1)
                            return value;
                    }
                    return -1;
                }
                else
                    return -1;
            }

            public IEnumerator GetEnumerator() => new NodeEnumerator(Root);

            private class NodeEnumerator : IEnumerator
            {
                private int index = -1;
                private List<Node> flatnodes = new List<Node>();
                public NodeEnumerator(Node start)
                {
                    Flatten(start, ref flatnodes);
                }

                private void Flatten(Node node, ref List<Node> output)
                {
                    output.Add(node);
                    for (int i = 0; i < node.Children.Count; i++)
                    {
                        Flatten(node.Children[i], ref output);
                    }
                }

                //IEnumerator and IEnumerable require these methods.
                public IEnumerator GetEnumerator() => this;
                //IEnumerator
                public bool MoveNext() => ++index < flatnodes.Count;
                //IEnumerable
                public void Reset() => index = 0;
                //IEnumerable
                public object Current => flatnodes[index];
            }

            public enum J3DLoadFlags
            {
                // Scaling rule
                ScalingRule_Basic = 0x00000000,
                ScalingRule_XSI = 0x00000001,
                ScalingRule_Maya = 0x00000002,
                ScalingRule_Mask = 0x0000000F,
                //unfinished documentations
            }

            public enum NodeType
            {
                End = 0,
                OpenChild = 1,
                CloseChild = 2,
                Joint = 16,
                Material = 17,
                Shape = 18
            }
        }

        //=====================================================================

    }
}
