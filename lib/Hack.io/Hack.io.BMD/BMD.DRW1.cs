using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using static Hack.io.J3D.J3DGraph;

//Heavily based on the SuperBMD Library.
namespace Hack.io.BMD
{
    public partial class BMD
    {
        public class DRW1
        {
            public List<bool> IsPartialWeight { get; private set; } = new List<bool>();
            public List<int> TransformIndexTable { get; private set; } = new List<int>();

            public Matrix4[] Matrices;

            private static readonly string Magic = "DRW1";

            public DRW1(Stream BMD)
            {
                int ChunkStart = (int)BMD.Position;
                if (!BMD.ReadString(4).Equals(Magic))
                    throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");

                int ChunkSize = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);
                int entryCount = BitConverter.ToInt16(BMD.ReadReverse(0, 2), 0);
                BMD.Position += 0x2;

                int boolDataOffset = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);
                int indexDataOffset = BitConverter.ToInt32(BMD.ReadReverse(0, 4), 0);

                IsPartialWeight = new List<bool>();

                BMD.Seek(ChunkStart + boolDataOffset, System.IO.SeekOrigin.Begin);
                for (int i = 0; i < entryCount; i++)
                    IsPartialWeight.Add(BMD.ReadByte() > 0);

                BMD.Seek(ChunkStart + indexDataOffset, System.IO.SeekOrigin.Begin);
                for (int i = 0; i < entryCount; i++)
                    TransformIndexTable.Add(BitConverter.ToInt16(BMD.ReadReverse(0, 2), 0));

                BMD.Position = ChunkStart + ChunkSize;
                Matrices = new Matrix4[entryCount];
            }

            public void Write(Stream writer)
            {
                long start = writer.Position;

                writer.WriteString("DRW1");
                writer.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for section size
                writer.WriteReverse(BitConverter.GetBytes((short)IsPartialWeight.Count), 0, 2);
                writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);

                writer.Write(new byte[4] { 0x00, 0x00, 0x00, 0x14 }, 0, 4); // Offset to weight type bools, always 20
                long IndiciesOffset = writer.Position;
                writer.WriteReverse(BitConverter.GetBytes(20 + IsPartialWeight.Count), 0, 4); // Offset to indices, always 20 + number of weight type bools

                foreach (bool bol in IsPartialWeight)
                    writer.WriteByte((byte)(bol ? 0x01 : 0x00));

                AddPadding(writer, 2);

                uint IndOffs = (uint)(writer.Position - start);
                foreach (int inte in TransformIndexTable)
                    writer.WriteReverse(BitConverter.GetBytes((short)inte), 0, 2);

                AddPadding(writer, 32);

                long end = writer.Position;
                long length = end - start;

                writer.Position = start + 4;
                writer.WriteReverse(BitConverter.GetBytes((int)length), 0, 4);
                writer.Position = start + 0x10;
                writer.WriteReverse(BitConverter.GetBytes(IndOffs), 0, 4); // Offset to indices, always 20 + number of weight type bools
                writer.Position = end;
            }

            public void UpdateMatrices(IList<BMD.JNT1.Bone> bones, EVP1 envelopes)
            {
                Matrix4[] bone_mats = new Matrix4[bones.Count];

                for (int i = 0; i < bones.Count; i++)
                {
                    bone_mats[i] = bones[i].CompiledMatrix;
                }

                for (int i = 0; i < Matrices.Length; i++)
                {
                    if (IsPartialWeight[i])
                    {
                        EVP1.Weight env = envelopes.Weights[TransformIndexTable[i]];

                        Matrix4 result = Matrix4.Zero;
                        for (int j = 0; j < env.BoneIndices.Count; j++)
                        {
                            Matrix4 sm1 = envelopes.InverseBindMatrices[env.BoneIndices[j]];
                            Matrix4 sm2 = bone_mats[env.BoneIndices[j]];

                            result += Matrix4.Mult(Matrix4.Mult(sm1, sm2), env.BoneIndices[j]);
                        }

                        Matrices[i] = result;
                    }
                    else
                    {
                        Matrices[i] = bone_mats[TransformIndexTable[i]];
                    }
                }
            }
        }

        //=====================================================================
    }
}
