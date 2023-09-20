namespace AuroraLib.Texture.Formats
{
    /// <summary>
    /// Extracts textures from a Scene in Pokémon XD Gale of Darkness (developed by Genius Sonority, hence the GS prefix).
    /// TODO: Find out if other Genius Sonority uses this format (such as Pokémon Colosseum and maybe other GameCube games).
    /// </summary>
    public class GSScene : HALDAT
    {
        public static readonly string[] Extensions = new string[] { ".gsscene", ".floordat", ".modeldat" };

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
        {
            for (int i = 0; i < Extensions.Length; i++)
            {
                if (extension.Contains(Extensions[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public GSScene()
        { }

        public GSScene(Stream stream) => Read(stream);

        public struct SceneDataStruct
        {
            public uint ModelArrayOffset;
            public uint CameraOffset;
            public uint LightArrayOffset;
            public uint Unknown0C;
        }

        public struct ModelStruct
        {
            public uint JObjOffset;
            public uint MotionOffset;
            public uint Unknown08;
            public uint Unknown0C;
        }

        protected override void Read(Stream stream)
        {
            ArchiveInfo archiveInfo = new ArchiveInfo();
            archiveInfo.Read(stream);

            uint scene_data_offset;
            if (!archiveInfo.PublicSymbols.TryGetValue("scene_data", out scene_data_offset))
            {
                throw new Exception("scene_data public symbol not found");
            }

            Stream sceneStream = archiveInfo.DataStream;
            Scene scene = new Scene(sceneStream);
            sceneStream.Seek(scene_data_offset, SeekOrigin.Begin);
            SceneDataStruct sceneDataStruct = sceneStream.Read<SceneDataStruct>(Endian.Big);

            uint model_array_offset = sceneDataStruct.ModelArrayOffset;
            if (model_array_offset == 0)
                return;

            sceneStream.Seek(model_array_offset, SeekOrigin.Begin);
            uint model_offset = sceneStream.ReadUInt32(Endian.Big);
            while (model_offset != 0)
            {
                sceneStream.Seek(model_offset, SeekOrigin.Begin);
                ModelStruct model = sceneStream.Read<ModelStruct>(Endian.Big);

                if (model.JObjOffset == 0)
                {
                    // Sometimes it valid (such as in the camera movement files), but sometimes it is not
                    // (only example in GXXP01: wzx_carde_bg03.fsys/0CE72000_carde_bg03.wzx at 0xA0).
                    // As such, check that the class name offset is at least still "valid" to determine whenever it is a valid JObj.
                    sceneStream.Seek(model.JObjOffset, SeekOrigin.Begin);
                    uint class_name_offset = sceneStream.ReadUInt32(Endian.Big);
                    if (class_name_offset != 0)
                    {
                        // Do nothing.
                        // TODO: Produce a log entry
                    }
                    else
                    {
                        scene.ParseJObj(model.JObjOffset);
                    }
                }
                else
                {
                    scene.ParseJObj(model.JObjOffset);
                }

                model_array_offset += 4;
                sceneStream.Seek(model_array_offset, SeekOrigin.Begin);
                model_offset = sceneStream.ReadUInt32(Endian.Big);
            }

            foreach (TexEntry tex_entry in scene.textures)
            {
                Add(tex_entry);
            }
        }
    }
}
