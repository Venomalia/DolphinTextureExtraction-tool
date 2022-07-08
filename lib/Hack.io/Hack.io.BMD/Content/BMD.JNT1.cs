using System;
using System.Collections.Generic;
using System.IO;
using AuroraLip.Common;
using AuroraLip.Texture.J3D;
using OpenTK;

//Heavily based on the SuperBMD Library.
namespace Hack.io.BMD
{
    public partial class BMD
    {
        public class JNT1
        {
            public List<JNT1.Bone> FlatSkeleton { get; private set; } = new List<JNT1.Bone>();
            public Dictionary<string, int> BoneNameIndices { get; private set; } = new Dictionary<string, int>();

            private static readonly string Magic = "JNT1";

            public JNT1(Stream BMD)
            {
                int ChunkStart = (int)BMD.Position;
                if (!BMD.ReadString(4).Equals(Magic))
                    throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");

                int jnt1Size = BitConverter.ToInt32(BMD.ReadBigEndian( 4), 0);
                int jointCount = BitConverter.ToInt16(BMD.ReadBigEndian( 2), 0);
                BMD.Position += 0x02;
                
                int jointDataOffset = BitConverter.ToInt32(BMD.ReadBigEndian( 4), 0);
                int internTableOffset = BitConverter.ToInt32(BMD.ReadBigEndian( 4), 0);
                int nameTableOffset = BitConverter.ToInt32(BMD.ReadBigEndian( 4), 0);

                List<string> names = new List<string>();

                BMD.Seek(ChunkStart + nameTableOffset, SeekOrigin.Begin);

                short stringCount = BitConverter.ToInt16(BMD.ReadBigEndian( 2), 0);
                BMD.Position += 0x02;

                for (int i = 0; i < stringCount; i++)
                {
                    BMD.Position += 0x02;
                    short nameOffset = BitConverter.ToInt16(BMD.ReadBigEndian( 2), 0);
                    long saveReaderPos = BMD.Position;
                    BMD.Position = ChunkStart + nameTableOffset + nameOffset;

                    names.Add(BMD.ReadString());

                    BMD.Position = saveReaderPos;
                }

                int highestRemap = 0;
                List<int> remapTable = new List<int>();
                BMD.Seek(ChunkStart + internTableOffset, SeekOrigin.Begin);
                for (int i = 0; i < jointCount; i++)
                {
                    int test = BitConverter.ToInt16(BMD.ReadBigEndian( 2), 0);
                    remapTable.Add(test);

                    if (test > highestRemap)
                        highestRemap = test;
                }

                List<JNT1.Bone> tempList = new List<JNT1.Bone>();
                BMD.Seek(ChunkStart + jointDataOffset, SeekOrigin.Begin);
                for (int i = 0; i <= highestRemap; i++)
                {
                    tempList.Add(new JNT1.Bone(BMD, names[i]));
                }

                for (int i = 0; i < jointCount; i++)
                {
                    FlatSkeleton.Add(tempList[remapTable[i]]);
                }

                foreach (JNT1.Bone bone in FlatSkeleton)
                    BoneNameIndices.Add(bone.Name, FlatSkeleton.IndexOf(bone));

                BMD.Position = ChunkStart + jnt1Size;
            }

            public void Write(Stream writer)
            {
                long start = writer.Position;

                writer.WriteString("JNT1");
                writer.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for section size
                writer.WriteBigEndian(BitConverter.GetBytes((short)FlatSkeleton.Count), 0, 2);
                writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);

                writer.Write(new byte[4] { 0x00, 0x00, 0x00, 0x18 }, 0, 4); // Offset to joint data, always 24
                writer.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for remap data offset
                writer.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for name table offset

                List<string> names = new List<string>();
                foreach (JNT1.Bone bone in FlatSkeleton)
                {
                    byte[] BoneData = bone.ToBytes();
                    writer.Write(BoneData, 0, BoneData.Length);
                    names.Add(bone.Name);
                }

                long curOffset = writer.Position;

                writer.Seek((int)(start + 16), SeekOrigin.Begin);
                writer.WriteBigEndian(BitConverter.GetBytes((int)(curOffset - start)), 0, 4);
                writer.Seek((int)curOffset, SeekOrigin.Begin);

                for (int i = 0; i < FlatSkeleton.Count; i++)
                    writer.WriteBigEndian(BitConverter.GetBytes((short)i), 0, 2);

                writer.AddPadding(4, Padding);

                curOffset = writer.Position;

                writer.Seek((int)(start + 20), SeekOrigin.Begin);
                writer.WriteBigEndian(BitConverter.GetBytes((int)(curOffset - start)), 0, 4);
                writer.Seek((int)curOffset, SeekOrigin.Begin);

                writer.WriteStringTable(names);

                writer.AddPadding(32, Padding);

                long end = writer.Position;
                long length = end - start;

                writer.Seek((int)start + 4, SeekOrigin.Begin);
                writer.WriteBigEndian(BitConverter.GetBytes((int)length), 0, 4);
                writer.Seek((int)end, SeekOrigin.Begin);
            }

            public void InitBoneFamilies(INF1 Scenegraph)
            {
                List<JNT1.Bone> processedJoints = new List<JNT1.Bone>();
                IterateHierarchyForSkeletonRecursive(Scenegraph.Root, processedJoints, -1);
            }
            private void IterateHierarchyForSkeletonRecursive(INF1.Node curNode, List<JNT1.Bone> processedJoints, int parentIndex)
            {
                switch (curNode.Type)
                {
                    case INF1.NodeType.Joint:
                        JNT1.Bone joint = FlatSkeleton[curNode.Index];

                        if (parentIndex >= 0)
                        {
                            joint.Parent = processedJoints[parentIndex];
                        }
                        processedJoints.Add(joint);
                        break;
                }

                parentIndex = processedJoints.Count - 1;
                foreach (var child in curNode.Children)
                    IterateHierarchyForSkeletonRecursive(child, processedJoints, parentIndex);
            }
            internal void InitBoneMatricies(INF1 SceneGraph)
            {
                for (int i = 0; i < FlatSkeleton.Count; i++)
                {
                    Bone jnt = FlatSkeleton[i];
                    foreach (INF1.Node node in SceneGraph)
                    {
                        if (node.Type != INF1.NodeType.Joint) continue;
                        if (node.Index != i) continue;

                        INF1.Node parentnode = node;
                        do
                        {
                            if (parentnode.Parent == null)
                            {
                                parentnode = null;
                                break;
                            }

                            parentnode = parentnode.Parent;

                        } while (parentnode.Type != INF1.NodeType.Joint);

                        if (parentnode != null)
                            Matrix4.Mult(ref jnt.NormalMatrix, ref FlatSkeleton[parentnode.Index].CompiledMatrix, out jnt.CompiledMatrix);
                        else
                            jnt.CompiledMatrix = jnt.NormalMatrix;

                        break;
                    }
                }
            }

            public class Bone
            {
                public string Name { get; private set; }
                public Bone Parent { get; internal set; }
                public List<Bone> Children { get; private set; }
                public Matrix4 InverseBindMatrix { get; private set; }
                public Matrix4 TransformationMatrix => SRTToMatrix();
                public SHP1.BoundingVolume Bounds { get; private set; }

                private short m_MatrixType;
                private bool InheritParentScale;
                private Vector3 m_Scale;
                public Vector3 Scale => m_Scale;
                private Vector3 m_Rotation;
                public Quaternion Rotation => Quaternion.FromAxisAngle(new Vector3(0, 0, 1), m_Rotation.Z) * Quaternion.FromAxisAngle(new Vector3(0, 1, 0), m_Rotation.Y) * Quaternion.FromAxisAngle(new Vector3(1, 0, 0), m_Rotation.X);
                private Vector3 m_Translation;
                public Vector3 Translation => m_Translation;
                public Matrix4 CompiledMatrix;
                public Matrix4 NormalMatrix;

                public Bone(string name)
                {
                    Name = name;
                    Children = new List<Bone>();
                    Bounds = new SHP1.BoundingVolume();
                    m_Scale = Vector3.One;
                }

                public Bone(Stream BMD, string name)
                {
                    Children = new List<Bone>();

                    Name = name;
                    m_MatrixType = BitConverter.ToInt16(BMD.ReadBigEndian( 2), 0);
                    InheritParentScale = BMD.ReadByte() == 0;

                    BMD.Position++;

                    m_Scale = new Vector3(BitConverter.ToSingle(BMD.ReadBigEndian( 4), 0), BitConverter.ToSingle(BMD.ReadBigEndian( 4), 0), BitConverter.ToSingle(BMD.ReadBigEndian( 4), 0));

                    short xRot = BitConverter.ToInt16(BMD.ReadBigEndian( 2), 0);
                    short yRot = BitConverter.ToInt16(BMD.ReadBigEndian( 2), 0);
                    short zRot = BitConverter.ToInt16(BMD.ReadBigEndian( 2), 0);

                    float xConvRot = (float)(xRot * 180.0 / 32767.0);
                    float yConvRot = (float)(yRot * 180.0 / 32767.0);
                    float zConvRot = (float)(zRot * 180.0 / 32767.0);

                    m_Rotation = new Vector3((float)(xConvRot * (Math.PI / 180.0)), (float)(yConvRot * (Math.PI / 180.0)), (float)(zConvRot * (Math.PI / 180.0)));

                    //m_Rotation = Quaternion.FromAxisAngle(new Vector3(0, 0, 1), rotFull.Z) *
                    //             Quaternion.FromAxisAngle(new Vector3(0, 1, 0), rotFull.Y) *
                    //             Quaternion.FromAxisAngle(new Vector3(1, 0, 0), rotFull.X);

                    BMD.Position += 0x02;

                    m_Translation = new Vector3(BitConverter.ToSingle(BMD.ReadBigEndian( 4), 0), BitConverter.ToSingle(BMD.ReadBigEndian( 4), 0), BitConverter.ToSingle(BMD.ReadBigEndian( 4), 0));

                    Bounds = new SHP1.BoundingVolume(BMD);
                    CompiledMatrix = TransformationMatrix;
                    NormalMatrix = TransformationMatrix;
                }

                public void SetInverseBindMatrix(Matrix4 matrix)
                {
                    InverseBindMatrix = matrix;
                }

                public byte[] ToBytes()
                {
                    List<byte> outList = new List<byte>();

                    using (MemoryStream writer = new MemoryStream())
                    {
                        writer.WriteBigEndian(BitConverter.GetBytes(m_MatrixType), 0, 2);
                        writer.WriteByte((byte)(InheritParentScale ? 0x00 : 0x01));
                        writer.WriteByte(0xFF);

                        Vector3 Euler = new Vector3();

                        float ysqr = m_Rotation.Y * m_Rotation.Y;
                        //TODO: Fix this! It expects a Quaternion m_Rotation.W * m_Rotation.X...
                        float t0 = 2.0f * (m_Rotation.X + m_Rotation.Y * m_Rotation.Z);
                        float t1 = 1.0f - 2.0f * (m_Rotation.X * m_Rotation.X + ysqr);

                        Euler.X = (float)Math.Atan2(t0, t1);

                        float t2 = 2.0f * (m_Rotation.Y - m_Rotation.Z * m_Rotation.X);
                        t2 = t2 > 1.0f ? 1.0f : t2;
                        t2 = t2 < -1.0f ? -1.0f : t2;

                        Euler.Y = (float)Math.Asin(t2);

                        float t3 = 2.0f * (m_Rotation.Z + m_Rotation.X * m_Rotation.Y);
                        float t4 = 1.0f - 2.0f * (ysqr + m_Rotation.Z * m_Rotation.Z);

                        Euler.Z = (float)Math.Atan2(t3, t4);

                        Euler.X = Euler.X * (float)(180.0 / Math.PI);
                        Euler.Y = Euler.Y * (float)(180.0 / Math.PI);
                        Euler.Z = Euler.Z * (float)(180.0 / Math.PI);

                        short[] compressRot = new short[3];

                        //compressRot[0] = (ushort)(Euler.X * 32767.0 / 180.0);
                        //compressRot[1] = (ushort)(Euler.Y * 32767.0 / 180.0);
                        //compressRot[2] = (ushort)(Euler.Z * 32767.0 / 180.0);

                        //Some of this is broken apparently
                        compressRot[0] = (short)(Euler.X < 180 && Euler.X > -180 ? (m_MatrixType == 0x0002 ? Math.Round(Euler.X) * 32767.0 / 180.0 : Math.Round(Euler.X * 32767.0 / 180.0)) : (Euler.X >= 180 ? -32768 : 32767));
                        compressRot[1] = (short)(Euler.Y < 180 && Euler.Y > -180 ? (m_MatrixType == 0x0002 ? Math.Round(Euler.Y) * 32767.0 / 180.0 : Math.Round(Euler.Y * 32767.0 / 180.0)) : (Euler.Y >= 180 ? -32768 : 32767));
                        compressRot[2] = (short)(Euler.Z < 180 && Euler.Z > -180 ? (m_MatrixType == 0x0002 ? Math.Round(Euler.Z) * 32767.0 / 180.0 : Math.Round(Euler.Z * 32767.0 / 180.0)) : (Euler.Z >= 180 ? -32768 : 32767));

                        //=====
                        //Console.WriteLine(Name);
                        //Console.WriteLine($"Matrix Mode: {m_MatrixType.ToString("X4")}");
                        //Console.WriteLine($"compressRot[0]: {compressRot[0].ToString("X4")}");
                        //Console.WriteLine($"compressRot[1]: {compressRot[1].ToString("X4")}");
                        //Console.WriteLine($"compressRot[2]: {compressRot[2].ToString("X4")}");
                        //Console.WriteLine();
                        //=====

                        writer.WriteBigEndian(BitConverter.GetBytes(m_Scale.X), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(m_Scale.Y), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(m_Scale.Z), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(compressRot[0]), 0, 2);
                        writer.WriteBigEndian(BitConverter.GetBytes(compressRot[1]), 0, 2);
                        writer.WriteBigEndian(BitConverter.GetBytes(compressRot[2]), 0, 2);
                        writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);
                        writer.WriteBigEndian(BitConverter.GetBytes(m_Translation.X), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(m_Translation.Y), 0, 4);
                        writer.WriteBigEndian(BitConverter.GetBytes(m_Translation.Z), 0, 4);

                        Bounds.Write(writer);

                        outList.AddRange(writer.ToArray());
                    }

                    return outList.ToArray();
                }

                private Matrix4 SRTToMatrix()
                {
                    Bone curJoint = this;

                    Matrix4 cumulativeTransform = Matrix4.Identity;
                    Bone prevJoint = null;
                    while (curJoint != null)
                    {
                        Vector3 scale = curJoint.Scale;
                        if (prevJoint != null && !prevJoint.InheritParentScale)
                            scale = Vector3.One;
                        Matrix4 jointMatrix = Matrix4.CreateScale(scale) *
                                              Matrix4.CreateFromQuaternion(curJoint.Rotation) *
                                              Matrix4.CreateTranslation(curJoint.Translation);
                        cumulativeTransform *= jointMatrix;

                        prevJoint = curJoint;
                        curJoint = curJoint.Parent;
                    }

                    return cumulativeTransform;
                }
            }
        }

        //=====================================================================
    }
}
