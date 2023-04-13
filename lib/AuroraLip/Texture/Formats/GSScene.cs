﻿using AuroraLib.Common;
using AuroraLip.Texture.Formats;

namespace AuroraLib.Texture.Formats
{
    /// <summary>
    /// Extracts textures from a Scene in Pokémon XD Gale of Darkness (developed by Genius Sonority, hence the GS prefix).
    /// TODO: Find out if other Genius Sonority uses this format (such as Pokémon Colosseum and maybe other GameCube games).
    /// </summary>
    public class GSScene : HALDAT
    {
        public List<string> Extensions => new List<string>() {".gsscene", ".floordat", ".modeldat"};

        public override bool IsMatch(Stream stream, in string extension = "")
            => Extensions.Contains(extension.ToLower());

        public GSScene() { }

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

                scene.ParseJObj(model.JObjOffset);

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
