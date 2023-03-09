using System.Text;

namespace LibCPK
{
    public static class Tools
    {
        public static bool CheckListRedundant(List<FileEntry> input)
        {

            bool result = false;
            List<string> tmp = new List<string>();
            for (int i = 0; i < input.Count; i++)
            {
                string name = ((input[i].DirName != null) ?
                                        input[i].DirName + "/" : "") + input[i].FileName;
                if (!tmp.Contains(name))
                {
                    tmp.Add(name);
                }
                else
                {
                    result = true;
                    return result;
                }
            }
            return result;
        }

        public static Dictionary<string, string> ReadBatchScript(string batch_script_name)
        {
            //---------------------
            // TXT内部
            // original_file_name(in cpk),patch_file_name(in folder)
            // /HD_font_a.ftx,patch/BOOT.cpk_unpacked/HD_font_a.ftx
            // OTHER/ICON0.PNG,patch/BOOT.cpk_unpacked/OTHER/ICON0.PNG

            Dictionary<string, string> flist = new Dictionary<string, string>();

            StreamReader sr = new StreamReader(batch_script_name, Encoding.Default);
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.IndexOf(",") > -1)
                //只读取格式正确的行
                {
                    line = line.Replace("\n", "");
                    line = line.Replace("\r", "");
                    string[] currentValue = line.Split(',');
                    flist.Add(currentValue[0], currentValue[1]);
                }


            }
            sr.Close();

            return flist;
        }

        public static string ReadCString(BinaryReader br, int MaxLength = -1, long lOffset = -1, Encoding enc = null)
        {
            int Max;
            if (MaxLength == -1)
                Max = 255;
            else
                Max = MaxLength;

            long fTemp = br.BaseStream.Position;
            byte bTemp = 0;
            int i = 0;
            string result = "";

            if (lOffset > -1)
            {
                br.BaseStream.Seek(lOffset, SeekOrigin.Begin);
            }

            do
            {
                bTemp = br.ReadByte();
                if (bTemp == 0)
                    break;
                i += 1;
            } while (i < Max);

            if (MaxLength == -1)
                Max = i + 1;
            else
                Max = MaxLength;

            if (lOffset > -1)
            {
                br.BaseStream.Seek(lOffset, SeekOrigin.Begin);

                if (enc == null)
                    result = Encoding.UTF8.GetString(br.ReadBytes(i));
                else
                    result = enc.GetString(br.ReadBytes(i));

                br.BaseStream.Seek(fTemp, SeekOrigin.Begin);
            }
            else
            {
                br.BaseStream.Seek(fTemp, SeekOrigin.Begin);
                if (enc == null)
                    result = Encoding.ASCII.GetString(br.ReadBytes(i));
                else
                    result = enc.GetString(br.ReadBytes(i));

                br.BaseStream.Seek(fTemp + Max, SeekOrigin.Begin);
            }

            return result;
        }

        public static void DeleteFileIfExists(string sPath)
        {
            if (File.Exists(sPath))
                File.Delete(sPath);
        }

        public static string GetPath(string input)
        {
            return Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input);
        }

        public static byte[] GetData(BinaryReader br, long offset, int size)
        {
            byte[] result = null;
            long backup = br.BaseStream.Position;
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            result = br.ReadBytes(size);
            br.BaseStream.Seek(backup, SeekOrigin.Begin);
            return result;
        }

        public static string GetSafePath(string filename)
        {
            string dest = filename.Replace(@"\", "/");
            string fName = Path.GetFileName(dest);
            char[] invalids = Path.GetInvalidFileNameChars();
            string fixedName = fName;
            foreach (var t in invalids)
            {
                fixedName = fixedName.Replace(t, '_');
            }
            dest = dest.Replace(fName, fixedName);
            return dest;
        }

    }
}