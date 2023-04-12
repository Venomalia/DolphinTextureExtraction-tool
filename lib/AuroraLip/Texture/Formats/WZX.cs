using AuroraLib.Archives;
using AuroraLib.Common;
using System.ComponentModel;
using static AuroraLib.Texture.Formats.WZX;

namespace AuroraLib.Texture.Formats
{
    /// <summary>
    /// Genius Sonority "WZX" file format.
    /// Based on Pokémon XD Gale of Darkness (GXXP01).
    /// </summary>
    public class WZX : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public const string Extension = ".wzx";

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
            => extension.ToLower() == Extension;

        protected override void Read(Stream stream)
        {
            Root = new ArchiveDirectory() { OwnerArchive = this };
            Header header = stream.Read<Header>(Endian.Big);
            HeaderAfter header_after = stream.Read<HeaderAfter>(Endian.Big);

            stream.Seek(32);
            if (header_after.Size != 0)
            {
                Root.AddArchiveFile(stream, header_after.Size, stream.Position, $"{stream.Position:X8}.GSscene");
                stream.Seek(header_after.Size, SeekOrigin.Current);
                stream.Seek(32);
            }

            if (header_after.ChunksCount != 0)
            {
                for (uint i = 0; i < header_after.ChunksCount - 1; i++)
                {
                    Chunk chunk = stream.Read<Chunk>(Endian.Big);
                    switch (chunk.Type)
                    {
                        case 0x01:
                            ParseType01(stream, chunk, Root);
                            break;
                        case 0x02:
                            ParseType02(stream, chunk, Root);
                            break;
                        case 0x03:
                            ParseType03(stream, chunk, Root);
                            break;
                        case 0x04:
                            ParseType04(stream, chunk, Root);
                            break;
                        case 0x05:
                            ParseType05(stream, chunk, Root);
                            break;
                        case 0x06:
                            ParseType06(stream, chunk, Root);
                            break;
                        case 0x07:
                            ParseType07(stream, chunk, Root);
                            break;
                        default:
                            throw new Exception($"Unknown type: 0x{chunk.Type:X8}");
                    }
                }
            }
        }

        protected void ParseType01(Stream stream, Chunk chunk, ArchiveDirectory Root)
        {
            Type01 param = stream.Read<Type01>(Endian.Big);
            if (param.Type == 3)
            {
                Root.AddArchiveFile(stream, param.Size, stream.Position, $"{stream.Position:X8}.bin");
                stream.Seek(param.Size, SeekOrigin.Current);
            }
        }

        protected void ParseType02(Stream stream, Chunk chunk, ArchiveDirectory Root)
        {
            Type02 param = stream.Read<Type02>(Endian.Big);
            stream.Seek(32);
            if (param.Size != 0)
            {
                Root.AddArchiveFile(stream, param.Size, stream.Position, $"{stream.Position:X8}.GSscene");
                stream.Seek(param.Size, SeekOrigin.Current);
                stream.Seek(32);
            }
        }

        protected void ParseType03(Stream stream, Chunk chunk, ArchiveDirectory Root)
        {
            Type03 param = stream.Read<Type03>(Endian.Big);
            stream.Seek(32);
            if (chunk.Unknown6C == 0)
            {
                Root.AddArchiveFile(stream, param.Size, stream.Position, $"{stream.Position:X8}.GPT");
                stream.Seek(param.Size, SeekOrigin.Current);
                stream.Seek(32);
            }
        }

        protected void ParseType04(Stream stream, Chunk chunk, ArchiveDirectory Root)
        {
            Type04 param = stream.Read<Type04>(Endian.Big);
            switch (param.Subtype)
            {
                case 0x00:
                    ParseType04Subtype00(stream, chunk, Root);
                    break;
                case 0x01:
                    ParseType04Subtype01(stream, chunk, Root);
                    break;
                case 0x05:
                    ParseType04Subtype05(stream, chunk, Root);
                    break;
                case 0x06:
                    ParseType04Subtype06(stream, chunk, Root);
                    break;
                case 0x07:
                    ParseType04Subtype07(stream, chunk, Root);
                    break;
                case 0x09:
                    ParseType04Subtype09(stream, chunk, Root);
                    break;
                default:
                    throw new Exception($"Unknown type 0x04 subtype: 0x{param.Subtype:X8}");
            }
        }

        protected void ParseType04Subtype00(Stream stream, Chunk chunk, ArchiveDirectory Root)
        {
            Type04Subtype00 subparam = stream.Read<Type04Subtype00>(Endian.Big);
            stream.Seek(subparam.Count * 0x10, SeekOrigin.Current);
        }

        protected void ParseType04Subtype01(Stream stream, Chunk chunk, ArchiveDirectory Root)
        {
            Type04Subtype01 subparam = stream.Read<Type04Subtype01>(Endian.Big);
            stream.Seek(32);
            if (chunk.Unknown6C == 0)
            {
                // TODO: Doesn't post-process, so maybe don't include?
                Root.AddArchiveFile(stream, subparam.Size, stream.Position, $"{stream.Position:X8}.GPT");
            }
            stream.Seek(subparam.Size, SeekOrigin.Current);
            stream.Seek(32);
        }

        protected void ParseType04Subtype05(Stream stream, Chunk chunk, ArchiveDirectory Root)
        {
            Type04Subtype05 subparam = stream.Read<Type04Subtype05>(Endian.Big);
            stream.Seek(32);
            if (subparam.Size != 0)
            {
                Root.AddArchiveFile(stream, subparam.Size, stream.Position, $"{stream.Position:X8}.GSscene");
                stream.Seek(subparam.Size, SeekOrigin.Current);
                stream.Seek(32);
            }
        }

        protected void ParseType04Subtype06(Stream stream, Chunk chunk, ArchiveDirectory Root)
        {
            Type04Subtype06 subparam = stream.Read<Type04Subtype06>(Endian.Big);
            stream.Seek(subparam.Count * 0x10, SeekOrigin.Current);
        }

        protected void ParseType04Subtype07(Stream stream, Chunk chunk, ArchiveDirectory Root)
        {
            Type04Subtype07 subparam = stream.Read<Type04Subtype07>(Endian.Big);
            stream.Seek(subparam.Count * 0x10, SeekOrigin.Current);
        }

        protected void ParseType04Subtype09(Stream stream, Chunk chunk, ArchiveDirectory Root)
        {
            Type04Subtype09 subparam = stream.Read<Type04Subtype09>(Endian.Big);
            stream.Seek(32);

            if (subparam.ArchiveSize != 0)
            {
                Root.AddArchiveFile(stream, subparam.ArchiveSize, stream.Position, $"{stream.Position:X8}.GSscene");
                stream.Seek(subparam.ArchiveSize, SeekOrigin.Current);
                stream.Seek(32);
            }

            if (subparam.ParticleSize != 0)
            {
                Root.AddArchiveFile(stream, subparam.ParticleSize, stream.Position, $"{stream.Position:X8}.GPT");
                stream.Seek(subparam.ParticleSize, SeekOrigin.Current);
                stream.Seek(32);
            }
        }

        protected void ParseType05(Stream stream, Chunk chunk, ArchiveDirectory Root)
        {
            Type05 param = stream.Read<Type05>(Endian.Big);
            // Does nothing at loading, so we do nothing.
        }

        protected void ParseType06(Stream stream, Chunk chunk, ArchiveDirectory Root)
        {
            Type06 param = stream.Read<Type06>(Endian.Big);
            // Does nothing at loading, so we do nothing.
        }

        protected void ParseType07(Stream stream, Chunk chunk, ArchiveDirectory Root)
        {
            Type07 param = stream.Read<Type07>(Endian.Big);
            stream.Seek(32);
            // TODO: Doesn't post-process, so maybe don't include?
            Root.AddArchiveFile(stream, param.Size, stream.Position, $"{stream.Position:X8}.bin");
            stream.Seek(param.Size, SeekOrigin.Current);
            stream.Seek(32);
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }

        public struct Header
        {
            public uint Unknown00;
            public uint Unknown04;
            public uint Unknown08;
            public uint Unknown0C;
            public uint Unknown10;
            public uint Unknown14;
            public uint Unknown18;
            public uint Unknown1C;
            public uint Unknown20;
            public uint Unknown24;
            public uint Unknown28;
            public uint Unknown2C;
            public uint Unknown30;
            public uint Unknown34;
            public uint Unknown38;
            public uint Unknown3C;
            public uint Unknown40;
            public uint Unknown44;
            public uint Unknown48;
            public uint Unknown4C;
            public uint Unknown50;
            public uint Unknown54;
            public uint Unknown58;
            public uint Unknown5C;
            public uint Unknown60;
            public uint Unknown64;
            public uint Unknown68;
            public uint Unknown6C;
        }

        public struct HeaderAfter
        {
            public uint Unknown00;
            public uint ChunksCount;
            public uint Unknown08;
            public uint Unknown0C;
            public uint Unknown10;
            public uint Size;
            public uint Unknown18;
            public uint Ptr; // Filled at runtime
            // Padding to 32 bytes
            // Size bytes of HSD Archive
            // Padding to 32 bytes
        }

        public struct Chunk
        {
            public uint Unknown00;
            public uint Type;
            public uint Unknown08;
            public uint Unknown0C;
            public uint Unknown10;
            public uint Unknown14;
            public uint Unknown18;
            public uint Unknown1C;
            public uint Unknown20;
            public uint Unknown24;
            public uint Unknown28;
            public uint Unknown2C;
            public uint Unknown30;
            public uint Unknown34;
            public uint Unknown38;
            public uint Unknown3C;
            public uint Unknown40;
            public uint Unknown44;
            public uint Unknown48;
            public uint Unknown4C;
            public uint Unknown50;
            public uint Unknown54;
            public uint Unknown58;
            public uint Unknown5C;
            public uint Unknown60;
            public uint Unknown64;
            public uint Unknown68;
            public uint Unknown6C;
        }

        struct Type01
        {
            public uint Type; // Maybe?
            public uint Size; // Not always?
            public uint Unknown08;
            // If Type == 3, then Size bytes of data follows
        }

        struct Type02
        {
            public uint Unknown00;
            public uint Unknown04;
            public uint Unknown08;
            public uint Unknown0C;
            public uint Unknown10;
            public uint Unknown14;
            public uint Unknown18;
            public uint Size;
            public uint Ptr; // Filled at runtime
            public uint Unknown24;
            // Padding to 32 bytes
            // Size bytes of HSD Archive
            // Padding to 32 bytes
        }

        struct Type03
        {
            public uint Unknown00;
            public uint Unknown04;
            public uint Size;
            public uint Ptr; // Filled at runtime
            public uint Unknown10;
            // Padding to 32 bytes
            // Size bytes of GSparticle (GPT)
            // Padding to 32 bytes
        }

        struct Type04
        {
            public uint Subtype;
            public uint Unknown04;
            public uint Unknown08;
            // Subtype data follows here
        }

        struct Type04Subtype00
        {
            public uint Unknown00;
            public uint Count;
            public uint Unknown08;
            public uint Unknown0C;
            // Count * 0x10 bytes follow, decrypted with itself? XOR within a 0x10 chunk
        }

        struct Type04Subtype01
        {
            public uint Unknown00;
            public uint Unknown04;
            public uint Unknown08;
            public uint Size;
            public uint Ptr; // Filled at runtime
            public uint Unknown14;
            // Padding to 32 bytes
            // Size bytes of GSparticle (GPT)
            // Padding to 32 bytes
        }

        struct Type04Subtype05
        {
            public uint Unknown00;
            public uint Unknown04;
            public uint Unknown08;
            public uint Unknown0C;
            public uint Unknown10;
            public uint Unknown14;
            public uint Unknown18;
            public uint Unknown1C;
            public uint Unknown20;
            public uint Unknown24;
            public uint Unknown28;
            public uint Unknown2C;
            public uint Unknown30;
            public uint Size;
            public uint Ptr; // Filled at runtime
            public uint Unknown3C;
            // Padding to 32 bytes
            // Size bytes of HSD Archive
            // Padding to 32 bytes
        }

        struct Type04Subtype06
        {
            public uint Unknown00;
            public uint Unknown04;
            public uint Count;
            public uint Unknown0C;
            // Count * 0x10 bytes follow
        }

        struct Type04Subtype07
        {
            public uint Count;
            public uint Unknown04;
            public uint Unknown08;
            // Count * 0x10 bytes follow
        }
        struct Type04Subtype09
        {
            public uint ArchiveSize;
            public uint ArchivePtr; // Filled at runtime
            public uint ParticleSize;
            public uint ParticlePtr; // Filled at runtime
            // Padding to 32 bytes
            // Size bytes of HSD Archive
            // Padding to 32 bytes
            // Size bytes of GSparticle (GPT)
            // Padding to 32 bytes
        }

        struct Type05
        {
            public uint Unknown00;
            public uint Unknown04;
            public uint Unknown08;
            public uint Unknown0C;
            public uint Unknown10;
        }

        struct Type06
        {
            public uint Unknown00;
            public uint Unknown04;
        }

        struct Type07
        {
            public uint Size;
            public uint Unknown04;
            public uint Unknown08;
            public uint Unknown0C;
            public uint Unknown10;
            // Padding to 32 bytes
            // Size bytes of data (unknown?)
            // Padding to 32 bytes
        }
    }
}
