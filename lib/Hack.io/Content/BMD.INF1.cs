using AuroraLib.Common;
using System.Collections;

//Heavily based on the SuperBMD Library.
namespace Hack.io
{
    public partial class BMD
    {
        public class INF1 : IEnumerable
        {
            public Node Root { get; set; } = null;
            public J3DLoadFlags ScalingRule { get; set; }

            private static readonly string Magic = "INF1";


            public INF1() { }
            public INF1(Stream stream, out int VertexCount)
            {
                long ChunkStart = stream.Position;
                if (!stream.ReadString(4).Equals(Magic))
                    throw new InvalidIdentifierException(Magic);

                int ChunkSize = stream.ReadInt32(Endian.Big);
                ScalingRule = (J3DLoadFlags)stream.ReadInt16(Endian.Big);
                stream.Position += 0x02;
                stream.Position += 0x04;
                VertexCount = stream.ReadInt32(Endian.Big);
                int HierarchyOffset = stream.ReadInt32(Endian.Big);
                stream.Position = ChunkStart + HierarchyOffset;

                Node parent = new Node(stream, null);
                Node node = null;

                Root = parent;
                do
                {
                    node = new Node(stream, parent);

                    if (node.Type == NodeType.OpenChild)
                    {
                        Node newNode = new Node(stream, parent);
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

                stream.Position = ChunkStart + ChunkSize;
            }

            public void Write(Stream stream, SHP1 ShapesForMatrixGroupCount, VTX1 VerticiesForVertexCount)
            {
                long start = stream.Position;

                stream.Write("INF1");
                stream.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for section size
                stream.Write((short)ScalingRule, Endian.Big);
                stream.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);

                stream.Write(SHP1.CountPackets(ShapesForMatrixGroupCount), Endian.Big); // Number of packets
                stream.Write(VerticiesForVertexCount.Attributes.Positions.Count, Endian.Big); // Number of vertex positions
                stream.Write(24, Endian.Big);

                Root.Write(stream);

                stream.Write((short)0x0000, Endian.Big);
                stream.Write((short)0x0000, Endian.Big);

                stream.WriteAlign(32, Padding);

                long end = stream.Position;
                stream.Position = start + 4;
                stream.Write((int)(end - start), Endian.Big);
                stream.Position = end;
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

                public Node(Stream stream, Node parent)
                {
                    Parent = parent;
                    Type = (NodeType)stream.ReadInt16(Endian.Big);
                    Index = stream.ReadInt16(Endian.Big);
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

                public void Write(Stream stream)
                {
                    stream.Write((short)Type, Endian.Big);
                    stream.Write((short)Index, Endian.Big);
                    if (Children.Count > 0)
                    {
                        stream.Write((short)0x0001, Endian.Big);
                        stream.Write((short)0x0000, Endian.Big);
                    }
                    for (int i = 0; i < Children.Count; i++)
                    {
                        Children[i].Write(stream);
                    }
                    if (Children.Count > 0)
                    {
                        stream.Write((short)0x0002, Endian.Big);
                        stream.Write((short)0x0000, Endian.Big);
                    }
                }

                public void SetParent(Node parent)
                {
                    Parent = parent;
                }

                public override string ToString()
                {
                    return $"{Type} : {Index}";
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
