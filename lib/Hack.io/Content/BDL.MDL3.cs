using AuroraLib.Common;
using OpenTK.Mathematics;

//Heavily based on the SuperBMD Library.
namespace Hack.io
{

    public partial class BDL
    {
        public class MDL3
        {
            public List<Packet> Packets { get; set; } = new List<Packet>();
            private static readonly string Magic = "MDL3";
            public MDL3(Stream stream)
            {
                int ChunkStart = (int)stream.Position;
                if (!stream.ReadString(4).Equals(Magic))
                    throw new InvalidIdentifierException(Magic);

                int mdl3Size = stream.ReadInt32(Endian.Big);
                short EntryCount = stream.ReadInt16(Endian.Big);
                stream.Position += 0x02; //Skip the padding
                uint PacketListingOffset = stream.ReadUInt32(Endian.Big),
                    SubPacketOffset = stream.ReadUInt32(Endian.Big),
                    MatrixIDOffset = stream.ReadUInt32(Endian.Big),
                    UnknownOffset = stream.ReadUInt32(Endian.Big),
                    IndiciesOffset = stream.ReadUInt32(Endian.Big),
                    StringTableOFfset = stream.ReadUInt32(Endian.Big);

                stream.Position = ChunkStart + PacketListingOffset;
                for (int i = 0; i < EntryCount; i++)
                    Packets.Add(new Packet(stream));


                stream.Position = ChunkStart + mdl3Size;
            }

            public class Packet
            {
                public List<GXCommand> Commands { get; set; } = new List<GXCommand>();

                public Packet(Stream stream)
                {
                    long PausePosition = stream.Position;
                    uint Offset = stream.ReadUInt32(Endian.Big), Size = stream.ReadUInt32(Endian.Big);
                    stream.Position = PausePosition + Offset;
                    long PacketLimit = stream.Position + Size;
                    while (stream.Position < PacketLimit)
                    {
                        byte id = (byte)stream.ReadByte();
                        GXCommand CurrentCommand = id == 0x61 ? new BPCommand(stream) as GXCommand : (id == 0x10 ? new XFCommand(stream) as GXCommand : null);
                        if (CurrentCommand == null)
                            break;
                        Commands.Add(CurrentCommand);
                    }
                    stream.Position = PausePosition + 8;
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

                public BPCommand(Stream stream)
                {
                    Register = (BPRegister)stream.ReadByte();
                    Value = stream.ReadUInt24(Endian.Big);
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

                public XFCommand(Stream stream)
                {
                    int DataLength = (stream.ReadInt16(Endian.Big) + 1) * 4;
                    Register = (XFRegister)stream.ReadInt16(Endian.Big);
                    switch (Register)
                    {
                        case XFRegister.SETTEXMTX0:
                        case XFRegister.SETTEXMTX1:
                            Arguments.Add(new XFTexMatrix(stream));
                            break;
                        default:
                            stream.Read(DataLength);
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

                    public XFTexMatrix(Stream stream)
                    {
                        CompiledMatrix = new Matrix2x4(
                            stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big),
                            stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big), stream.ReadSingle(Endian.Big)
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
