using AuroraLib.Archives;
using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    /// <summary>
    /// Genius Sonority "GSW" file format.
    /// Based on Pokemon XD (GXXP01).
    /// </summary>
    public class GSW : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public const string Extension = ".GSW";

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
            => extension == Extension;

        protected override void Read(Stream stream)
        {
            Root = new ArchiveDirectory() { OwnerArchive = this };
            Header header = stream.Read<Header>(Endian.Big);

            uint SpriteDepth = 0;
            uint InstructionPointer = 0x30;

            while (true)
            {
                stream.Seek(InstructionPointer, SeekOrigin.Begin);
                InstructionStruct Instruction = stream.Read<InstructionStruct>(Endian.Big);

                if (Instruction.Opcode == Opcode.End)
                {
                    if (SpriteDepth == 0)
                        return;
                    SpriteDepth--;
                }
                else if (Instruction.Opcode == Opcode.DefineSprite)
                {
                    // Basically a jump label, but as an texture extractor we don't care but still need to match ends
                    SpriteDepth++;
                }
                else if (Instruction.Opcode == Opcode.DefineBitsLossless || Instruction.Opcode == Opcode.DefineBitsLossless2)
                {
                    uint aligned = (InstructionPointer + 8 + 32 - 1) & ~(32u - 1);
                    uint data_leftover = Instruction.DataSize - (aligned - (InstructionPointer + 8));

                    // TODO: How big is the struct? Looking at the file it appears to be 0x20, but trying to understand the code seems different?
                    Root.AddArchiveFile(stream, data_leftover - 0x20, aligned + 0x20, $"{aligned:X8}.GTX");
                }
                else if (Instruction.Opcode == Opcode.DefineBits || Instruction.Opcode == Opcode.DefineBitsJPEG2 || Instruction.Opcode == Opcode.DefineBitsJPEG3)
                {
                    // These aren't implemented in GXXP01
                    //throw new Exception($"Unimplemented GSW opcode: {Instruction.Opcode}");
                    Events.NotificationEvent.Invoke(NotificationType.Info, $"Unimplemented GSW opcode: {Instruction.Opcode}");
                }

                InstructionPointer += 8 + Instruction.DataSize;
            }
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }

        public struct Header
        {
            public uint Unknown00;
            public ushort Unknown04;
            public ushort NumFrames;
            public uint Unknown08;
            public uint Unknown0C;
            public uint Unknown10;
            public uint Unknown14;
            public ushort NumSprites;
            public ushort NumObjects;
            public uint Unknown1C;
            public uint Unknown20;
            public uint Unknown24;
            public uint Unknown28;
            public uint Unknown2C;
            // Instructions follow here
        }

        /// <summary>
        /// Opcodes (instructions) the file contains.
        /// For some reason the names are still included in the game binary, so they are used there (minus the <tt>stag</tt> prefix).
        /// </summary>
        public enum Opcode : uint
        {
            End = 0x00,
            ShowFrame = 0x01,
            DefineShape = 0x02,
            FreeCharacter = 0x03,
            PlaceObject = 0x04,
            RemoveObject = 0x05,
            DefineBits = 0x06,
            DefineButton = 0x07,
            JPEGTables = 0x08,
            SetBackgroundColor = 0x09,
            DefineFont = 0x0A,
            DefineText = 0x0B,
            DoAction = 0x0C,
            DefineFontInfo = 0x0D,
            DefineSound = 0x0E,
            DefineButtonSound = 0x11,
            SoundStreamHead = 0x12,
            SoundStreamBlock = 0x13,
            DefineBitsLossless = 0x14,
            DefineBitsJPEG2 = 0x15,
            DefineShape2 = 0x16,
            DefineButtonCxform = 0x17,
            Protect = 0x18,
            PlaceObject2 = 0x1A,
            RemoveObject2 = 0x1C,
            DefineShape3 = 0x20,
            DefineText2 = 0x21,
            DefineButton2 = 0x22,
            DefineBitsJPEG3 = 0x23,
            DefineBitsLossless2 = 0x24,
            DefineEditText = 0x25,
            DefineSprite = 0x27,
            NameCharacter = 0x28,
            FrameLabel = 0x2B,
            SoundStreamHead2 = 0x2D,
            DefineFont2 = 0x30,
            ExportAssets = 0x38,
            ImportAssets = 0x39,
        }

        /// <summary>
        /// An instruction, which determines what to do and its parameters as data.
        /// </summary>
        public struct InstructionStruct
        {
            public Opcode Opcode;
            public uint DataSize;
            // DataSize bytes follow here
        }
    }
}
