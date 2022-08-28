using AuroraLip.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;

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

            public DRW1(Stream stream)
            {
                int ChunkStart = (int)stream.Position;
                if (!stream.ReadString(4).Equals(Magic))
                    throw new InvalidIdentifierException(Magic);

                int ChunkSize = stream.ReadInt32(Endian.Big);
                int entryCount = stream.ReadInt16(Endian.Big);
                stream.Position += 0x2;

                int boolDataOffset = stream.ReadInt32(Endian.Big);
                int indexDataOffset = stream.ReadInt32(Endian.Big);

                IsPartialWeight = new List<bool>();

                stream.Seek(ChunkStart + boolDataOffset, System.IO.SeekOrigin.Begin);
                for (int i = 0; i < entryCount; i++)
                    IsPartialWeight.Add(stream.ReadByte() > 0);

                stream.Seek(ChunkStart + indexDataOffset, System.IO.SeekOrigin.Begin);
                for (int i = 0; i < entryCount; i++)
                    TransformIndexTable.Add(stream.ReadInt16(Endian.Big));

                stream.Position = ChunkStart + ChunkSize;
                Matrices = new Matrix4[entryCount];
            }

            public void Write(Stream stream)
            {
                long start = stream.Position;

                stream.WriteString("DRW1");
                stream.Write(new byte[4] { 0xDD, 0xDD, 0xDD, 0xDD }, 0, 4); // Placeholder for section size
                stream.WriteBigEndian(BitConverter.GetBytes((short)IsPartialWeight.Count), 0, 2);
                stream.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);

                stream.Write(new byte[4] { 0x00, 0x00, 0x00, 0x14 }, 0, 4); // Offset to weight type bools, always 20
                long IndiciesOffset = stream.Position;
                stream.WriteBigEndian(BitConverter.GetBytes(20 + IsPartialWeight.Count), 0, 4); // Offset to indices, always 20 + number of weight type bools

                foreach (bool bol in IsPartialWeight)
                    stream.WriteByte((byte)(bol ? 0x01 : 0x00));

                stream.AddPadding(2, Padding);

                uint IndOffs = (uint)(stream.Position - start);
                foreach (int inte in TransformIndexTable)
                    stream.WriteBigEndian(BitConverter.GetBytes((short)inte), 0, 2);

                stream.AddPadding(32, Padding);

                long end = stream.Position;
                long length = end - start;

                stream.Position = start + 4;
                stream.WriteBigEndian(BitConverter.GetBytes((int)length), 0, 4);
                stream.Position = start + 0x10;
                stream.WriteBigEndian(BitConverter.GetBytes(IndOffs), 0, 4); // Offset to indices, always 20 + number of weight type bools
                stream.Position = end;
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
