using AuroraLib.Core.Exceptions;
using OpenTK.Mathematics;

//Heavily based on the SuperBMD Library.
namespace Hack.io
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

            public EVP1(Stream stream)
            {
                int ChunkStart = (int)stream.Position;
                if (!stream.ReadString(4).Equals(Magic))
                    throw new InvalidIdentifierException(Magic);

                int ChunkSize = stream.ReadInt32(Endian.Big);
                int entryCount = stream.ReadInt16(Endian.Big);
                stream.Position += 0x02;

                int weightCountsOffset = stream.ReadInt32(Endian.Big);
                int boneIndicesOffset = stream.ReadInt32(Endian.Big);
                int weightDataOffset = stream.ReadInt32(Endian.Big);
                int inverseBindMatricesOffset = stream.ReadInt32(Endian.Big);

                List<int> counts = new List<int>();
                List<float> weights = new List<float>();
                List<int> indices = new List<int>();

                for (int i = 0; i < entryCount; i++)
                    counts.Add(stream.ReadByte());

                stream.Seek(boneIndicesOffset + ChunkStart, SeekOrigin.Begin);

                for (int i = 0; i < entryCount; i++)
                {
                    for (int j = 0; j < counts[i]; j++)
                    {
                        indices.Add(stream.ReadInt16(Endian.Big));
                    }
                }

                stream.Seek(weightDataOffset + ChunkStart, SeekOrigin.Begin);

                for (int i = 0; i < entryCount; i++)
                {
                    for (int j = 0; j < counts[i]; j++)
                    {
                        weights.Add(stream.ReadSingle(Endian.Big));
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

                stream.Seek(inverseBindMatricesOffset + ChunkStart, SeekOrigin.Begin);
                int matrixCount = (ChunkSize - inverseBindMatricesOffset) / 48;

                for (int i = 0; i < matrixCount; i++)
                {
                    Matrix3x4 invBind = new Matrix3x4(stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big),
                                                      stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big),
                                                      stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big));

                    Matrix4 BindMatrix = new Matrix4(invBind.Row0, invBind.Row1, invBind.Row2, Vector4.UnitW);
                    BindMatrix.Transpose();
                    InverseBindMatrices.Add(BindMatrix);
                }

                stream.Position = ChunkStart + ChunkSize;
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

            public void Write(Stream stream)
            {
                long start = stream.Position;

                stream.Write("EVP1");
                stream.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for section size
                stream.Write((short)Weights.Count, Endian.Big); ;
                stream.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);

                if (Weights.Count == 0)
                {
                    stream.Write(new byte[4] { 0x00, 0x00, 0x00, 0x00 }, 0, 4);
                    stream.Write(new byte[4] { 0x00, 0x00, 0x00, 0x00 }, 0, 4);
                    stream.Write(new byte[4] { 0x00, 0x00, 0x00, 0x00 }, 0, 4);
                    stream.Write(new byte[4] { 0x00, 0x00, 0x00, 0x00 }, 0, 4);
                    stream.Position = start + 4;
                    stream.Write(new byte[4] { 0x00, 0x00, 0x00, 0x20 }, 0, 4);
                    stream.Seek(0, SeekOrigin.End);
                    stream.WriteAlign(8, Padding);
                    return;
                }

                stream.Write(new byte[4] { 0x00, 0x00, 0x00, 0x1C }, 0, 4); // Offset to weight count data. Always 28
                stream.Write(28 + Weights.Count, Endian.Big); // Offset to bone/weight indices. Always 28 + the number of weights
                stream.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for weight data offset
                stream.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for inverse bind matrix data offset

                foreach (Weight w in Weights)
                    stream.WriteByte((byte)w.Count);

                foreach (Weight w in Weights)
                {
                    foreach (int inte in w.BoneIndices)
                        stream.Write((short)inte, Endian.Big);
                }

                stream.WriteAlign(4, Padding);

                long curOffset = stream.Position;

                stream.Position = start + 20;
                stream.Write((int)(curOffset - start), Endian.Big);
                stream.Position = curOffset;

                foreach (Weight w in Weights)
                {
                    foreach (float fl in w.Weights)
                        stream.Write(fl, Endian.Big);
                }

                curOffset = stream.Position;

                stream.Position = start + 24;
                stream.Write((int)(curOffset - start), Endian.Big);
                stream.Position = curOffset;

                foreach (Matrix4 mat in InverseBindMatrices)
                {
                    Vector4 Row1 = mat.Row0;
                    Vector4 Row2 = mat.Row1;
                    Vector4 Row3 = mat.Row2;

                    stream.Write(Row1.X, Endian.Big);
                    stream.Write(Row1.Y, Endian.Big);
                    stream.Write(Row1.Z, Endian.Big);
                    stream.Write(Row1.W, Endian.Big);

                    stream.Write(Row2.X, Endian.Big);
                    stream.Write(Row2.Y, Endian.Big);
                    stream.Write(Row2.Z, Endian.Big);
                    stream.Write(Row2.W, Endian.Big);

                    stream.Write(Row3.X, Endian.Big);
                    stream.Write(Row3.Y, Endian.Big);
                    stream.Write(Row3.Z, Endian.Big);
                    stream.Write(Row3.W, Endian.Big);
                }

                stream.WriteAlign(32, Padding);

                long end = stream.Position;
                long length = end - start;

                stream.Position = start + 4;
                stream.Write((int)length, Endian.Big);
                stream.Position = end;
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
