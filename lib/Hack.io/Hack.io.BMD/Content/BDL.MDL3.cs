using AuroraLip.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;

//Heavily based on the SuperBMD Library.
namespace Hack.io.BMD
{

    public partial class BDL
    {
        public class MDL3
        {
            public List<Packet> Packets { get; set; } = new List<Packet>();
            private static readonly string Magic = "MDL3";
            public MDL3(Stream BDL)
            {
                int ChunkStart = (int)BDL.Position;
                if (!BDL.ReadString(4).Equals(Magic))
                    throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");

                int mdl3Size = BitConverter.ToInt32(BDL.ReadBigEndian(4), 0);
                short EntryCount = BitConverter.ToInt16(BDL.ReadBigEndian(2), 0);
                BDL.Position += 0x02; //Skip the padding
                uint PacketListingOffset = BitConverter.ToUInt32(BDL.ReadBigEndian(4), 0), SubPacketOffset = BitConverter.ToUInt32(BDL.ReadBigEndian(4), 0),
                    MatrixIDOffset = BitConverter.ToUInt32(BDL.ReadBigEndian(4), 0), UnknownOffset = BitConverter.ToUInt32(BDL.ReadBigEndian(4), 0),
                    IndiciesOffset = BitConverter.ToUInt32(BDL.ReadBigEndian(4), 0), StringTableOFfset = BitConverter.ToUInt32(BDL.ReadBigEndian(4), 0);

                BDL.Position = ChunkStart + PacketListingOffset;
                for (int i = 0; i < EntryCount; i++)
                    Packets.Add(new Packet(BDL));


                BDL.Position = ChunkStart + mdl3Size;
            }

            public class Packet
            {
                public List<GXCommand> Commands { get; set; } = new List<GXCommand>();

                public Packet(Stream BDL)
                {
                    long PausePosition = BDL.Position;
                    uint Offset = BitConverter.ToUInt32(BDL.ReadBigEndian(4), 0), Size = BitConverter.ToUInt32(BDL.ReadBigEndian(4), 0);
                    BDL.Position = PausePosition + Offset;
                    long PacketLimit = BDL.Position + Size;
                    while (BDL.Position < PacketLimit)
                    {
                        byte id = (byte)BDL.ReadByte();
                        GXCommand CurrentCommand = id == 0x61 ? new BPCommand(BDL) as GXCommand : (id == 0x10 ? new XFCommand(BDL) as GXCommand : null);
                        if (CurrentCommand == null)
                            break;
                        Commands.Add(CurrentCommand);
                    }
                    BDL.Position = PausePosition + 8;
                }
            }

            public abstract class GXCommand
            {
                public virtual byte Identifier => 0x00;
                public abstract int GetRegister();
                public abstract object GetData();
            }
            public class BPCommand : GXCommand
            {
                public override byte Identifier => 0x61;
                public BPRegister Register { get; set; }
                public UInt24 Value { get; set; }

                public BPCommand(Stream BDL)
                {
                    Register = (BPRegister)BDL.ReadByte();
                    Value = BitConverterEx.ToUInt24(BDL.ReadBigEndian(3), 0);
                }

                public override string ToString() => $"BP Command: {Register}, {Value.ToString()}";

                public override int GetRegister() => (int)Register;

                public override object GetData() => Value;
            }
            public class XFCommand : GXCommand
            {
                public override byte Identifier => 0x10;
                public XFRegister Register { get; set; }
                public List<IXFArgument> Arguments { get; set; } = new List<IXFArgument>();

                public XFCommand(Stream BDL)
                {
                    int DataLength = (BitConverter.ToInt16(BDL.ReadBigEndian(2), 0) + 1) * 4;
                    Register = (XFRegister)BitConverter.ToInt16(BDL.ReadBigEndian(2), 0);
                    switch (Register)
                    {
                        case XFRegister.SETTEXMTX0:
                        case XFRegister.SETTEXMTX1:
                            Arguments.Add(new XFTexMatrix(BDL));
                            break;
                        default:
                            BDL.Read(DataLength);
                            break;
                    }
                }
                public override string ToString() => $"XF Command: {Register}, {Arguments.Count}";

                public override int GetRegister() => (int)Register;

                public override object GetData() => Arguments;

                public interface IXFArgument
                {

                }
                public class XFTexMatrix : IXFArgument
                {
                    public Matrix2x4 CompiledMatrix { get; set; }

                    public XFTexMatrix(Stream BDL)
                    {
                        CompiledMatrix = new Matrix2x4(
                            BitConverter.ToSingle(BDL.ReadBigEndian(4), 0), BitConverter.ToSingle(BDL.ReadBigEndian(4), 0), BitConverter.ToSingle(BDL.ReadBigEndian(4), 0), BitConverter.ToSingle(BDL.ReadBigEndian(4), 0),
                            BitConverter.ToSingle(BDL.ReadBigEndian(4), 0), BitConverter.ToSingle(BDL.ReadBigEndian(4), 0), BitConverter.ToSingle(BDL.ReadBigEndian(4), 0), BitConverter.ToSingle(BDL.ReadBigEndian(4), 0)
                            );
                    }

                    public XFTexMatrix(MAT3.Material.TexMatrix Source)
                    {
                        double theta = Source.Rotation * 3.141592;
                        double sinR = Math.Sin(theta);
                        double cosR = Math.Cos(theta);

                        CompiledMatrix = Source.IsMaya ? new Matrix2x4(
                            (float)(Source.Scale.X * cosR), (float)(Source.Scale.X * -sinR), 0.0f, (float)(Source.Scale.X * (-0.5f * sinR - 0.5f * cosR + 0.5f - Source.Translation.X)),
                            (float)(-Source.Scale.Y * sinR), (float)(Source.Scale.Y * cosR), 0.0f, (float)(Source.Scale.Y * (0.5f * sinR - 0.5f * cosR - 0.5f + Source.Translation.Y) + 1.0f)
                            ) : new Matrix2x4(
                            (float)(Source.Scale.X * cosR), (float)(Source.Scale.X * -sinR), (float)(Source.Translation.X + Source.Center.X + Source.Scale.X * (sinR * Source.Center.Y - cosR * Source.Center.X)), 0.0f,
                            (float)(Source.Scale.Y * sinR), (float)(Source.Scale.Y * cosR), (float)(Source.Translation.Y + Source.Center.Y + -Source.Scale.Y * (-sinR * Source.Center.X + cosR * Source.Center.Y)), 0.0f
                            );

                        Matrix4 Test = Matrix4.Identity;
                        float[] temp = new float[4 * 4];
                        temp[0] = (float)(Source.Scale.X * cosR);
                        temp[4] = (float)(Source.Scale.X * -sinR);
                        temp[12] = (float)(Source.Translation.X + Source.Center.X + Source.Scale.X * (sinR * Source.Center.Y - cosR * Source.Center.X));
                    }
                }
            }
        }

        //=====================================================================
    }
}
