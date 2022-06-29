using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using static Hack.io.J3D.J3DGraph;

//Heavily based on the SuperBMD Library.
namespace Hack.io.BMD
{
    public partial class BMD
    {
        public class EVP1
        {
            public List<Weight> Weights { get; private set; } = new List<Weight>();
            public List<Matrix4> InverseBindMatrices { get; private set; } = new List<Matrix4>();

            private static readonly string Magic = "EVP1";

            public Weight this[Weight target]
            {
                get
                {
                    for (int i = 0; i < Weights.Count; i++)
                    {
                        if (ReferenceEquals(target, Weights[i]))
                        {
                            return target;
                        }
                    }
                    throw new Exception();
                }
            }

            public EVP1(Stream BMD)
            {
                int ChunkStart = (int)BMD.Position;
                if (!BMD.ReadString(4).Equals(Magic))
                    throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");

                int ChunkSize = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);
                int entryCount = BitConverter.ToInt16(BMD.ReadReverse(0, 2), 0);
                BMD.Position += 0x02;

                int weightCountsOffset = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);
                int boneIndicesOffset = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);
                int weightDataOffset = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);
                int inverseBindMatricesOffset = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);

                List<int> counts = new List<int>();
                List<float> weights = new List<float>();
                List<int> indices = new List<int>();

                for (int i = 0; i < entryCount; i++)
                    counts.Add(BMD.ReadByte());

                BMD.Seek(boneIndicesOffset + ChunkStart, SeekOrigin.Begin);

                for (int i = 0; i < entryCount; i++)
                {
                    for (int j = 0; j < counts[i]; j++)
                    {
                        indices.Add(BitConverter.ToInt16(BMD.ReadReverse(0, 2), 0));
                    }
                }

                BMD.Seek(weightDataOffset + ChunkStart, SeekOrigin.Begin);

                for (int i = 0; i < entryCount; i++)
                {
                    for (int j = 0; j < counts[i]; j++)
                    {
                        weights.Add(BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0));
                    }
                }

                int totalRead = 0;
                for (int i = 0; i < entryCount; i++)
                {
                    Weight weight = new Weight();

                    for (int j = 0; j < counts[i]; j++)
                    {
                        weight.AddWeight(weights[totalRead + j], indices[totalRead + j]);
                    }

                    Weights.Add(weight);
                    totalRead += counts[i];
                }

                BMD.Seek(inverseBindMatricesOffset + ChunkStart, SeekOrigin.Begin);
                int matrixCount = (ChunkSize - inverseBindMatricesOffset) / 48;

                for (int i = 0; i < matrixCount; i++)
                {
                    Matrix3x4 invBind = new Matrix3x4(BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0), BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0), BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0), BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0),
                                                      BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0), BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0), BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0), BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0),
                                                      BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0), BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0), BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0), BitConverter.ToSingle(BMD.ReadReverse(0, 4), 0));

                    Matrix4 BindMatrix = new Matrix4(invBind.Row0, invBind.Row1, invBind.Row2, Vector4.UnitW);
                    BindMatrix.Transpose();
                    InverseBindMatrices.Add(BindMatrix);
                }

                BMD.Position = ChunkStart + ChunkSize;
            }

            public void SetInverseBindMatrices(List<JNT1.Bone> flatSkel)
            {
                if (InverseBindMatrices.Count == 0)
                {
                    // If the original file didn't specify any inverse bind matrices, use default values instead of all zeroes.
                    // And these must be set both in the skeleton and the EVP1.
                    for (int i = 0; i < flatSkel.Count; i++)
                    {
                        Matrix4 newMat = new Matrix4(Vector4.UnitX, Vector4.UnitY, Vector4.UnitZ, Vector4.UnitW);
                        InverseBindMatrices.Add(newMat);
                        flatSkel[i].SetInverseBindMatrix(newMat);
                    }
                    return;
                }

                for (int i = 0; i < flatSkel.Count; i++)
                {
                    Matrix4 newMat = InverseBindMatrices[i];
                    flatSkel[i].SetInverseBindMatrix(newMat);
                }
            }

            public void Write(Stream writer)
            {
                long start = writer.Position;

                writer.WriteString("EVP1");
                writer.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for section size
                writer.WriteReverse(BitConverter.GetBytes((short)Weights.Count), 0, 2);
                writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);

                if (Weights.Count == 0)
                {
                    writer.Write(new byte[4] { 0x00, 0x00, 0x00, 0x00 }, 0, 4);
                    writer.Write(new byte[4] { 0x00, 0x00, 0x00, 0x00 }, 0, 4);
                    writer.Write(new byte[4] { 0x00, 0x00, 0x00, 0x00 }, 0, 4);
                    writer.Write(new byte[4] { 0x00, 0x00, 0x00, 0x00 }, 0, 4);
                    writer.Position = start + 4;
                    writer.Write(new byte[4] { 0x00, 0x00, 0x00, 0x20 }, 0, 4);
                    writer.Seek(0, SeekOrigin.End);
                    AddPadding(writer, 8);
                    return;
                }
                
                writer.Write(new byte[4] { 0x00, 0x00, 0x00, 0x1C }, 0, 4); // Offset to weight count data. Always 28
                writer.WriteReverse(BitConverter.GetBytes(28 + Weights.Count), 0, 4); // Offset to bone/weight indices. Always 28 + the number of weights
                writer.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for weight data offset
                writer.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for inverse bind matrix data offset

                foreach (Weight w in Weights)
                    writer.WriteByte((byte)w.Count);

                foreach (Weight w in Weights)
                {
                    foreach (int inte in w.BoneIndices)
                        writer.WriteReverse(BitConverter.GetBytes((short)inte), 0, 2);
                }

                AddPadding(writer, 4);

                long curOffset = writer.Position;

                writer.Position = start + 20;
                writer.WriteReverse(BitConverter.GetBytes((int)(curOffset - start)), 0, 4);
                writer.Position = curOffset;

                foreach (Weight w in Weights)
                {
                    foreach (float fl in w.Weights)
                        writer.WriteReverse(BitConverter.GetBytes(fl), 0, 4);
                }

                curOffset = writer.Position;

                writer.Position = start + 24;
                writer.WriteReverse(BitConverter.GetBytes((int)(curOffset - start)), 0, 4);
                writer.Position = curOffset;

                foreach (Matrix4 mat in InverseBindMatrices)
                {
                    Vector4 Row1 = mat.Row0;
                    Vector4 Row2 = mat.Row1;
                    Vector4 Row3 = mat.Row2;

                    writer.WriteReverse(BitConverter.GetBytes(Row1.X), 0, 4);
                    writer.WriteReverse(BitConverter.GetBytes(Row1.Y), 0, 4);
                    writer.WriteReverse(BitConverter.GetBytes(Row1.Z), 0, 4);
                    writer.WriteReverse(BitConverter.GetBytes(Row1.W), 0, 4);

                    writer.WriteReverse(BitConverter.GetBytes(Row2.X), 0, 4);
                    writer.WriteReverse(BitConverter.GetBytes(Row2.Y), 0, 4);
                    writer.WriteReverse(BitConverter.GetBytes(Row2.Z), 0, 4);
                    writer.WriteReverse(BitConverter.GetBytes(Row2.W), 0, 4);

                    writer.WriteReverse(BitConverter.GetBytes(Row3.X), 0, 4);
                    writer.WriteReverse(BitConverter.GetBytes(Row3.Y), 0, 4);
                    writer.WriteReverse(BitConverter.GetBytes(Row3.Z), 0, 4);
                    writer.WriteReverse(BitConverter.GetBytes(Row3.W), 0, 4);
                }

                AddPadding(writer, 32);

                long end = writer.Position;
                long length = end - start;

                writer.Position = start + 4;
                writer.WriteReverse(BitConverter.GetBytes((int)length), 0, 4);
                writer.Position = end;
            }

            public class Weight
            {
                public List<float> Weights { get; private set; }
                public List<int> BoneIndices { get; private set; }
                public int Count { get => Weights.Count; }

                public Weight()
                {
                    Weights = new List<float>();
                    BoneIndices = new List<int>();
                }

                public void AddWeight(float weight, int boneIndex)
                {
                    Weights.Add(weight);
                    BoneIndices.Add(boneIndex);
                }
            }
        }

        //=====================================================================
    }
}
