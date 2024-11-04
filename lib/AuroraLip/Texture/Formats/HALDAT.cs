using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    /// <summary>
    /// Extract texture from an HSD archive and scene.
    /// It appears it is usuall called the HAL DAT format by the community.
    /// This seems to come from a middleware called sysdolphin (by HAL Laboratory), but the "wrapper" format for the scene is called HSDArchive.
    /// </summary>
    /// <see href="https://wiki.raregamingdump.ca/index.php/sysdolphin"/>
    /// <see href="https://wiki.tockdom.com/wiki/HAL_DAT_(File_Format)"/>
    /// <see href="https://github.com/doldecomp/melee/blob/master/include/sysdolphin/baselib/"/>
    public abstract class HALDAT : JUTTexture, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public abstract bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default);

        protected override void Read(Stream stream)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Class for dealing with HSDArchives.
        /// Basically it contains a (big) file and pointers into that file for possibly various stuff such as the "entry point".
        /// Also contains relocation info because it contains pointers, that this implementation ignores because
        /// by default it is offset from the file, which is already "correct".
        /// </summary>
        public class ArchiveInfo
        {
            public Stream DataStream = null;
            public Dictionary<string, uint> PublicSymbols = new Dictionary<string, uint>();
            public Dictionary<string, uint> ExternSymbols = new Dictionary<string, uint>();
            public List<uint> Relocations = new List<uint>();
            public uint version;

            public struct Header
            {
                public uint FileSize;
                public uint DataSize;
                public uint NumRelocations;
                public uint NumPublicSymbols;
                public uint NumExternalSymbols;
                public uint Version;
                public uint Padding18;
                public uint Padding1C;
            }

            public virtual void Read(Stream stream)
            {
                Header header = stream.Read<Header>(Endian.Big);

                if (header.FileSize != stream.Length)
                {
                    throw new Exception($"Expected filesize {header.FileSize} does not match the given filesize {stream.Length}");
                }

                uint data_offset = 0x20;
                uint relocation_offset = data_offset + header.DataSize;
                uint public_offset = relocation_offset + header.NumRelocations * 4;
                uint extern_offset = public_offset + header.NumPublicSymbols * 8;
                uint symbols_offset = extern_offset + header.NumExternalSymbols * 8;

                DataStream = new SubStream(stream, header.DataSize);

                stream.Seek(header.DataSize, SeekOrigin.Current);

                if (stream.Position != relocation_offset)
                    throw new Exception("Internal error, expected relocation position isn't what we have calculated before");

                for (int i = 0; i < header.NumRelocations; ++i)
                {
                    Relocations.Add(stream.ReadUInt32(Endian.Big));
                }

                if (stream.Position != public_offset)
                    throw new Exception("Internal error, expected public symbols position isn't what we have calculated before");

                for (int i = 0; i < header.NumPublicSymbols; ++i)
                {
                    ReadSymbol(stream, PublicSymbols, symbols_offset);
                }

                if (stream.Position != extern_offset)
                    throw new Exception("Internal error, expected extern symbols position isn't what we have calculated before");

                for (int i = 0; i < header.NumExternalSymbols; ++i)
                {
                    ReadSymbol(stream, ExternSymbols, symbols_offset);
                }
            }

            protected void ReadSymbol(Stream stream, Dictionary<string, uint> dict, uint base_symbol_name_offset)
            {
                uint data_offset = stream.ReadUInt32(Endian.Big);
                uint symbol_offset = stream.ReadUInt32(Endian.Big);
                long prevPosition = stream.Position;
                stream.Seek(base_symbol_name_offset + symbol_offset, SeekOrigin.Begin);
                string symbol_name = stream.ReadCString();
                dict.Add(symbol_name, data_offset);
                stream.Seek(prevPosition, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// The file format uses "HSD" objects, where some can be overridden by user code for custom behaviour.
        /// </summary>
        public class HSDObjects
        {
            public class UnknownClassException : Exception
            {
                public UnknownClassException()
                {
                }

                public UnknownClassException(string message) : base(message)
                {
                }

                public UnknownClassException(string message, Exception inner) : base(message, inner)
                {
                }
            }

            /// <summary>
            /// Object that describes the class.
            /// Technically we would also have an HSDObject class to represent
            /// an instance of it, but that would just make stuff more
            /// complicated for an texture extraction tool, which doesn't need
            /// to reimplement those objects completely, just the parsing part.
            /// </summary>
            public abstract class HSDObjectClass
            {
            }

            protected Dictionary<string, HSDObjectClass> Classes = new Dictionary<string, HSDObjectClass>();

            protected HSDObjectClass GetClassFromName(string class_name)
            {
                HSDObjectClass value;
                Classes.TryGetValue(class_name, out value);
                return value;
            }

            protected T GetClassFromName<T>(string class_name) where T : HSDObjectClass
            {
                HSDObjectClass value;
                Classes.TryGetValue(class_name, out value);
                if (value is T)
                    return (T)value;
                throw new UnknownClassException(class_name);
            }

            protected T GetClassFromName<T>(string? class_name, string default_class_name) where T : HSDObjectClass
            {
                if (class_name == null)
                    return GetClassFromName<T>(default_class_name);

                T value = GetClassFromName<T>(class_name);
                if (value == null)
                    return GetClassFromName<T>(default_class_name);

                return value;
            }

            protected T GetClassFromName<T>(string? class_name, T default_instance) where T : HSDObjectClass
            {
                if (class_name == null)
                    return default_instance;

                T value = GetClassFromName<T>(class_name);
                if (value == null)
                    return default_instance;

                return value;
            }
        }

        /// <summary>
        /// The scene itself.
        /// It is a tree of joints, materials, textures, lights, camera objects et cetera.
        /// The "entry point"/the root seem to differ per game.
        /// TODO: Custom classes currently aren't implemented.
        /// </summary>
        public class Scene : HSDObjects
        {
            protected HashSet<TextureKey> processed_textures = new HashSet<TextureKey>();
            public List<TexEntry> textures = new List<TexEntry>();
            public Stream stream;

            /// <summary>
            /// A key for a dictionary to deduplicate textures based on parameters, just in case.
            /// </summary>
            protected class TextureKey
            {
                public uint Width = 0;
                public uint Height = 0;
                public GXImageFormat TextureFormat = GXImageFormat.I4;
                public GXPaletteFormat PaletteFormat = GXPaletteFormat.IA8;
                public uint TextureOffset = 0;
                public uint PaletteOffset = 0;

                public override bool Equals(object obj)
                {
                    return obj is TextureKey key &&
                           Width == key.Width &&
                           Height == key.Height &&
                           TextureFormat == key.TextureFormat &&
                           PaletteFormat == key.PaletteFormat &&
                           TextureOffset == key.TextureOffset &&
                           PaletteOffset == key.PaletteOffset;
                }

                public override int GetHashCode()
                {
                    int hashCode = -906676283;
                    hashCode = hashCode * -1521134295 + Width.GetHashCode();
                    hashCode = hashCode * -1521134295 + Height.GetHashCode();
                    hashCode = hashCode * -1521134295 + TextureFormat.GetHashCode();
                    hashCode = hashCode * -1521134295 + PaletteFormat.GetHashCode();
                    hashCode = hashCode * -1521134295 + TextureOffset.GetHashCode();
                    hashCode = hashCode * -1521134295 + PaletteOffset.GetHashCode();
                    return hashCode;
                }
            }

            public Scene(Stream stream)
            {
                this.stream = stream;
                Classes.Add("jobj", new JObjClass());
                Classes.Add("dobj", new DObjClass());
                Classes.Add("mobj", new MObjClass());
                Classes.Add("tobj", new TObjClass());
            }

            protected T GetClassFromStream<T>(uint offset, string default_class_name) where T : HSDObjectClass
            {
                stream.Seek(offset, SeekOrigin.Begin);
                uint class_name_offset = stream.ReadUInt32(Endian.Big);

                if (class_name_offset == 0)
                    return GetClassFromName<T>(null, default_class_name);

                stream.Seek(class_name_offset, SeekOrigin.Begin);
                string class_name = stream.ReadCString();
                return GetClassFromName<T>(class_name, default_class_name);
            }

            public class JObjClass : HSDObjectClass
            {
                /// <summary>
                /// Flag for a JObj (joint) indicating the children node is re-used for rendering.
                /// Attributes of that child joint is not used, and neither is it treated as a list (next is ignored).
                /// </summary>
                public const uint FLAG_INSTANCE = 0x1000;

                public const uint FLAG_SPLINE = 0x4000;
                public const uint FLAG_PTCL = 0x20;

                public struct JObjStruct
                {
                    public uint Flags;
                    public uint ChildOffset;
                    public uint NextOffset;
                    public uint SubObjectOffset;
                    // Ignoring attributes not relevant to extracing textures

                    public bool IsInstance
                    {
                        get => (Flags & FLAG_INSTANCE) != 0;
                    }

                    public bool IsSpline
                    {
                        get => (Flags & FLAG_SPLINE) != 0;
                    }

                    public bool IsPTCL
                    {
                        get => (Flags & FLAG_PTCL) != 0;
                    }
                }

                public virtual void Parse(Scene scene, uint jobj_offset)
                {
                    Stream stream = scene.stream;
                    Queue<uint> queue = new Queue<uint>();
                    queue.Enqueue(jobj_offset);

                    while (queue.Count != 0)
                    {
                        jobj_offset = queue.Dequeue();
                        if ((jobj_offset % 4) != 0)
                            throw new Exception($"JObj offset is not aligned! 0x{jobj_offset:X8}");

                        stream.Seek(jobj_offset, SeekOrigin.Begin);
                        JObjStruct jobj = stream.Read<JObjStruct>(Endian.Big);

                        if (jobj.ChildOffset != 0 && !jobj.IsInstance)
                        {
                            JObjClass NextClass = scene.GetClassFromStream<JObjClass>(jobj.ChildOffset, "jobj");
                            if (NextClass.GetType() == typeof(JObjClass))
                            {
                                queue.Enqueue(jobj.ChildOffset + 4);
                            }
                            else
                            {
                                NextClass.Parse(scene, jobj.ChildOffset + 4);
                            }
                        }

                        if (jobj.NextOffset != 0)
                        {
                            JObjClass NextClass = scene.GetClassFromStream<JObjClass>(jobj.NextOffset, "jobj");
                            if (NextClass.GetType() == typeof(JObjClass))
                            {
                                queue.Enqueue(jobj.NextOffset + 4);
                            }
                            else
                            {
                                NextClass.Parse(scene, jobj.NextOffset + 4);
                            }
                        }

                        if (jobj.SubObjectOffset != 0)
                        {
                            if (!jobj.IsPTCL && !jobj.IsSpline)
                            {
                                scene.GetClassFromStream<DObjClass>(jobj.SubObjectOffset, "dobj").Parse(scene, jobj.SubObjectOffset + 4);
                            }
                            else if (!jobj.IsPTCL && jobj.IsSpline)
                            {
                                //throw new NotImplementedException("spline suboject in jobj"); // TODO
                            }
                            else if (jobj.IsPTCL && !jobj.IsSpline)
                            {
                                //throw new NotImplementedException("ptcl suboject in jobj"); // TODO
                            }
                            else if (jobj.IsPTCL && jobj.IsSpline)
                            {
                                throw new Exception("Invalid combination: Can't be ptcl and spline at the same time");
                            }
                        }
                    }
                }
            }

            public class DObjClass : HSDObjectClass
            {
                public struct DObjStruct
                {
                    public uint NextOffset;
                    public uint MObjOffset;
                    public uint PObjOffset; // Uninteresting mesh data
                }

                public virtual void Parse(Scene scene, uint dobj_offset)
                {
                    Stream stream = scene.stream;
                    Queue<uint> queue = new Queue<uint>();
                    queue.Enqueue(dobj_offset);

                    while (queue.Count != 0)
                    {
                        dobj_offset = queue.Dequeue();
                        if ((dobj_offset % 4) != 0)
                            throw new Exception($"DObj offset is not aligned! 0x{dobj_offset:X8}");

                        stream.Seek(dobj_offset, SeekOrigin.Begin);
                        DObjStruct dobj = stream.Read<DObjStruct>(Endian.Big);

                        if (dobj.NextOffset != 0)
                        {
                            DObjClass NextClass = scene.GetClassFromStream<DObjClass>(dobj.NextOffset, "dobj");
                            if (NextClass.GetType() == typeof(TObjClass))
                            {
                                queue.Enqueue(dobj.NextOffset + 4);
                            }
                            else
                            {
                                NextClass.Parse(scene, dobj.NextOffset + 4);
                            }
                        }

                        if (dobj.MObjOffset != 0)
                        {
                            scene.GetClassFromStream<MObjClass>(dobj.MObjOffset, "mobj").Parse(scene, dobj.MObjOffset + 4);
                        }
                    }
                }
            }

            public class MObjClass : HSDObjectClass
            {
                public struct MObjStruct
                {
                    public uint RenderMode;
                    public uint TObjOffset;
                    // Ignoring attributes not relevant to extracing textures
                }

                public virtual void Parse(Scene scene, uint mobj_offset)
                {
                    Stream stream = scene.stream;
                    if ((mobj_offset % 4) != 0)
                        throw new Exception($"MObj offset is not aligned! 0x{mobj_offset:X8}");

                    stream.Seek(mobj_offset, SeekOrigin.Begin);
                    MObjStruct s = stream.Read<MObjStruct>(Endian.Big);

                    if (s.TObjOffset != 0)
                    {
                        scene.GetClassFromStream<TObjClass>(s.TObjOffset, "tobj").Parse(scene, s.TObjOffset + 4);
                    }
                }
            }

            public class TObjClass : HSDObjectClass
            {
                public struct TObjStruct
                {
                    public uint NextOffset;
                    public int ID; // TODO: Maybe use this to re-identify textures?
                    public uint Src;
                    public float RotationX;
                    public float RotationY;
                    public float RotationZ;
                    public float ScaleX;
                    public float ScaleY;
                    public float ScaleZ;
                    public float TranslationX;
                    public float TranslationY;
                    public float TranslationZ;
                    public uint WrapS;
                    public uint WrapT;
                    public byte RepeatS;
                    public byte RepeatT;
                    public ushort Padding;
                    public uint Flags;
                    public float Blending;
                    public uint MagFilter;
                    public uint ImageDescOffset;
                    public uint TLUTOffset;
                    public uint TexLODDescOffset;
                    public uint TObjTevDescOffset;
                }

                public struct ImageDescStruct
                {
                    public uint DataOffset;
                    public ushort Width;
                    public ushort Height;
                    public uint FormatRaw;
                    public uint Mipmap; // TODO: what is this exactly?
                    public float MinLOD;
                    public float MaxLOD;
                }

                public struct TLUTStruct
                {
                    public uint DataOffset;
                    public uint FormatRaw;
                    public uint Name;
                    public ushort NumEntries;
                }

                public virtual void Parse(Scene scene, uint tobj_offset)
                {
                    Stream stream = scene.stream;
                    Queue<uint> queue = new Queue<uint>();
                    queue.Enqueue(tobj_offset);

                    while (queue.Count != 0)
                    {
                        tobj_offset = queue.Dequeue();
                        if ((tobj_offset % 4) != 0)
                            throw new Exception($"TObj offset is not aligned! 0x{tobj_offset:X8}");

                        stream.Seek(tobj_offset, SeekOrigin.Begin);
                        TObjStruct tobj = stream.Read<TObjStruct>(Endian.Big);

                        if (tobj.NextOffset != 0)
                        {
                            TObjClass NextClass = scene.GetClassFromStream<TObjClass>(tobj.NextOffset, "tobj");
                            if (NextClass.GetType() == typeof(TObjClass))
                            {
                                queue.Enqueue(tobj.NextOffset + 4);
                            }
                            else
                            {
                                NextClass.Parse(scene, tobj.NextOffset + 4);
                            }
                        }

                        if (tobj.ImageDescOffset == 0)
                        {
                            throw new Exception($"TObj@{tobj_offset} has missing texture information data");
                        }

                        stream.Seek(tobj.ImageDescOffset, SeekOrigin.Begin);
                        ImageDescStruct ImageDesc = stream.Read<ImageDescStruct>(Endian.Big);
                        GXImageFormat texture_format = (GXImageFormat)ImageDesc.FormatRaw;

                        if (ImageDesc.Width > 2048 || ImageDesc.Height > 2048)
                        {
                            throw new Exception($"TObj@{tobj_offset} references a texture of size {ImageDesc.Width}x{ImageDesc.Height} which is bigger than the maximum 2048x2048");
                        }

                        TLUTStruct tlutStruct = new TLUTStruct();
                        GXPaletteFormat palette_format = (GXPaletteFormat)tlutStruct.FormatRaw;

                        if (texture_format.IsPaletteFormat())
                        {
                            if (tobj.TLUTOffset == 0)
                            {
                                throw new Exception($"TObj@{tobj_offset} requires palette (texture format: {texture_format}) but is missing");
                            }
                            else
                            {
                                stream.Seek(tobj.TLUTOffset, SeekOrigin.Begin);
                                tlutStruct = stream.Read<TLUTStruct>(Endian.Big);
                                palette_format = (GXPaletteFormat)tlutStruct.FormatRaw;
                            }
                        }

                        TextureKey textureKey = new TextureKey();
                        textureKey.TextureFormat = texture_format;
                        textureKey.PaletteFormat = palette_format;
                        textureKey.Width = ImageDesc.Width;
                        textureKey.Height = ImageDesc.Height;
                        textureKey.TextureOffset = tobj.TLUTOffset;
                        textureKey.PaletteOffset = tlutStruct.DataOffset;

                        if (!scene.processed_textures.Contains(textureKey))
                        {
                            byte[] palette_data = null;
                            if (texture_format.IsPaletteFormat())
                            {
                                stream.Seek(tlutStruct.DataOffset, SeekOrigin.Begin);
                                palette_data = new byte[tlutStruct.NumEntries * 2];
                                stream.Read(palette_data, 0, palette_data.Length);
                            }

                            stream.Seek(ImageDesc.DataOffset, SeekOrigin.Begin);
                            scene.textures.Add(new TexEntry(stream, palette_data, texture_format, palette_format, (int)tlutStruct.NumEntries, (int)ImageDesc.Width, (int)ImageDesc.Height, (int)ImageDesc.Mipmap)
                            {
                                WrapS = (GXWrapMode)tobj.WrapS,
                                WrapT = (GXWrapMode)tobj.WrapT,
                            });

                            scene.processed_textures.Add(textureKey);
                        }
                    }
                }
            }

            public void ParseJObj(uint offset)
            {
                GetClassFromStream<JObjClass>(offset, "jobj").Parse(this, offset + 4);
            }

            public void ParseDObj(uint offset)
            {
                GetClassFromStream<DObjClass>(offset, "dobj").Parse(this, offset + 4);
            }

            public void ParseMObj(uint offset)
            {
                GetClassFromStream<JObjClass>(offset, "mobj").Parse(this, offset + 4);
            }

            public void ParseTObj(uint offset)
            {
                GetClassFromStream<TObjClass>(offset, "tobj").Parse(this, offset + 4);
            }
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
