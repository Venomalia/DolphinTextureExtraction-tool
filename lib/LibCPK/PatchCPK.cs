using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibCPK
{
    public class PatchCPK
    {
        private CPK cpk;
        private string cpkContentName;
        private Action<float> onProgressChanged;
        private Action<string> onMsgUpdateChanged;
        private Action onCompleteChanged;
        public PatchCPK(CPK pCpk, string oldContentName)
        {
            this.cpk = pCpk;
            //MainApp.Instance.currentPackage.CpkContentName;
            this.cpkContentName = oldContentName;
        }

        public void SetListener(Action<float> onProgressChangedEvent, Action<string> onMsgUpdateEvent, Action onCompleteEvent)
        {
            this.onProgressChanged = onProgressChangedEvent;
            this.onMsgUpdateChanged = onMsgUpdateEvent;
            this.onCompleteChanged = onCompleteEvent;
        }

        public void Patch(string outputFilePath, bool bForceCompress, Dictionary<string, string> batch_file_list)
        {
            string msg;
            BinaryReader oldFile = new BinaryReader(File.OpenRead(this.cpkContentName));
            string outputName = outputFilePath;

            BinaryWriter newCPK = new BinaryWriter(File.OpenWrite(outputName));

            List<FileEntry> entries = cpk.fileTable.OrderBy(x => x.FileOffset).ToList();

            int id;
            bool bFileRepeated = Tools.CheckListRedundant(entries);
            for (int i = 0; i < entries.Count; i++)
            {
                onProgressChanged?.Invoke((float)i / (float)entries.Count * 100f);
                if (entries[i].FileType != FileTypeFlag.CONTENT)
                {
                    id = Convert.ToInt32(entries[i].ID);
                    string currentName;

                    if (id > 0 && bFileRepeated)
                    {

                        if (((string)entries[i].DirName) == "<NULL>" && ((string)entries[i].FileName) == "<NULL>")
                        {
                            currentName = id + ".bin";
                        }
                        else
                        {

                            currentName = (((entries[i].DirName != null) ?
                                            entries[i].DirName + "/" : "") + string.Format("[{0}]", id.ToString()) + entries[i].FileName);
                        }
                    }
                    else
                    {
                        if (((string)entries[i].DirName) == "<NULL>" && ((string)entries[i].FileName) == "<NULL>")
                        {
                            currentName = id + ".bin";
                        }
                        else
                        {

                            currentName = ((entries[i].DirName != null) ? entries[i].DirName + "/" : "") + entries[i].FileName;

                        }
                    }
                    if (!currentName.Contains("/"))
                    {
                        currentName = "/" + currentName;
                    }
                    if (entries[i].FileType == FileTypeFlag.FILE)
                    {
                        // I'm too lazy to figure out how to update the ContextOffset position so this works :)
                        if ((ulong)newCPK.BaseStream.Position < cpk.ContentOffset)
                        {
                            ulong padLength = cpk.ContentOffset - (ulong)newCPK.BaseStream.Position;
                            for (ulong z = 0; z < padLength; z++)
                            {
                                newCPK.Write((byte)0);
                            }
                        }

                        
                        Debug.Print("Got File:" + currentName.ToString());

                        if (!batch_file_list.Keys.Contains(currentName.ToString()))
                        //如果不在表中，复制原始数据
                        {
                            oldFile.BaseStream.Seek((long)entries[i].FileOffset, SeekOrigin.Begin);
                            entries[i].FileOffset = (ulong)newCPK.BaseStream.Position;
                            if (entries[i].FileName.ToString() == "ETOC_HDR")
                            {

                                cpk.EtocOffset = entries[i].FileOffset;
                                onMsgUpdateChanged?.Invoke(string.Format("Fix ETOC_OFFSET to {0:x8}", cpk.EtocOffset));
                            }
                            onMsgUpdateChanged?.Invoke(string.Format("Update Entry: {0}, {1:x8}", entries[i].FileName, entries[i].FileOffset));
                            cpk.UpdateFileEntry(entries[i]);

                            byte[] chunk = oldFile.ReadBytes(Int32.Parse(entries[i].FileSize.ToString()));
                            newCPK.Write(chunk);

                            if ((newCPK.BaseStream.Position % 0x800) > 0 && i < entries.Count - 1)
                            {
                                long cur_pos = newCPK.BaseStream.Position;
                                for (int j = 0; j < (0x800 - (cur_pos % 0x800)); j++)
                                {
                                    newCPK.Write((byte)0);
                                }
                            }

                        }
                        else
                        {
                            string replace_with = batch_file_list[currentName.ToString()];
                            //Got patch file name

                            onMsgUpdateChanged?.Invoke(string.Format("Patching: {0}", currentName.ToString()));

                            byte[] newbie = File.ReadAllBytes(replace_with);
                            entries[i].FileOffset = (ulong)newCPK.BaseStream.Position;
                            int o_ext_size = Int32.Parse((entries[i].ExtractSize).ToString());
                            int o_com_size = Int32.Parse((entries[i].FileSize).ToString());
                            if ((o_com_size < o_ext_size) && entries[i].FileType == FileTypeFlag.FILE && bForceCompress == true)
                            {
                                // is compressed
                                msg = string.Format("Compressing data:{0:x8}", newbie.Length);
                                onMsgUpdateChanged?.Invoke(msg);

                                byte[] dest_comp = CPK.CompressCRILAYLA(newbie);

                                entries[i].FileSize = Convert.ChangeType(dest_comp.Length, entries[i].FileSizeType);
                                entries[i].ExtractSize = Convert.ChangeType(newbie.Length, entries[i].FileSizeType);
                                cpk.UpdateFileEntry(entries[i]);
                                newCPK.Write(dest_comp);
                                onMsgUpdateChanged?.Invoke(string.Format("Update Entry: {0}, {1:x8}", entries[i].FileName, entries[i].FileOffset));
                                onMsgUpdateChanged?.Invoke(string.Format(">> {0:x8}\r\n", dest_comp.Length));
                            }

                            else
                            {
                                msg = string.Format("Storing data:{0:x8}\r\n", newbie.Length);
                                onMsgUpdateChanged?.Invoke(msg);
                                entries[i].FileSize = Convert.ChangeType(newbie.Length, entries[i].FileSizeType);
                                entries[i].ExtractSize = Convert.ChangeType(newbie.Length, entries[i].FileSizeType);
                                cpk.UpdateFileEntry(entries[i]);
                                newCPK.Write(newbie);
                                onMsgUpdateChanged?.Invoke(string.Format("Update Entry: {0}, {1:x8}", entries[i].FileName, entries[i].FileOffset));
                            }


                            if ((newCPK.BaseStream.Position % 0x800) > 0 && i < entries.Count - 1)
                            {
                                long cur_pos = newCPK.BaseStream.Position;
                                for (int j = 0; j < (0x800 - (cur_pos % 0x800)); j++)
                                {
                                    newCPK.Write((byte)0);
                                }
                            }
                        }
                    }
                    else
                    {
                        //Update HDR:
                        Debug.Print("Got HDR:" + currentName.ToString());
                        oldFile.BaseStream.Seek((long)entries[i].FileOffset, SeekOrigin.Begin);
                        entries[i].FileOffset = (ulong)newCPK.BaseStream.Position;
                        if (entries[i].FileName.ToString() == "CPK_HDR")
                        {

                        }
                        if (entries[i].FileName.ToString() == "TOC_HDR")
                        {
                            cpk.EtocOffset = entries[i].FileOffset;
                            onMsgUpdateChanged?.Invoke(string.Format("Fix ETOC_OFFSET to {0:x8}", cpk.EtocOffset));
                        }
                        if (entries[i].FileName.ToString() == "ETOC_HDR")
                        {
                            cpk.EtocOffset = entries[i].FileOffset;
                            onMsgUpdateChanged?.Invoke(string.Format("Fix ETOC_OFFSET to {0:x8}", cpk.EtocOffset));
                        }
                        if (entries[i].FileName.ToString() == "ITOC_HDR")
                        {
                            cpk.ItocOffset = entries[i].FileOffset;
                            onMsgUpdateChanged?.Invoke(string.Format("Fix ITOC_OFFSET to {0:x8}", cpk.ItocOffset));
                        }
                        if (entries[i].FileName.ToString() == "GTOC_HDR")
                        {
                            cpk.GtocOffset = entries[i].FileOffset;
                            onMsgUpdateChanged?.Invoke(string.Format("Fix ITOC_OFFSET to {0:x8}", cpk.GtocOffset));
                        }
                        onMsgUpdateChanged?.Invoke(string.Format("Update HDR Entry: {0}, {1:x8}", entries[i].FileName, entries[i].FileOffset));
                        cpk.UpdateFileEntry(entries[i]);

                        byte[] chunk = oldFile.ReadBytes(Int32.Parse(entries[i].FileSize.ToString()));
                        newCPK.Write(chunk);

                        if ((newCPK.BaseStream.Position % 0x800) > 0 && i < entries.Count - 1)
                        {
                            long cur_pos = newCPK.BaseStream.Position;
                            for (int j = 0; j < (0x800 - (cur_pos % 0x800)); j++)
                            {
                                newCPK.Write((byte)0);
                            }
                        }
                        if (entries[i].FileName.ToString() == "TOC_HDR")
                        {
                            //IF TOC ,WRITE MORE 0x800
                            for (int j = 0; j < 0x800; j++)
                            {
                                newCPK.Write((byte)0);
                            }
                        }
                    }
                }
                else
                {
                    // Content is special.... just update the position
                    onMsgUpdateChanged?.Invoke(string.Format("Update Special Entry: {0}, {1:x8}", entries[i].FileName, entries[i].FileOffset));
                    cpk.UpdateFileEntry(entries[i]);
                }
            }

            cpk.WriteCPK(newCPK);
            msg = string.Format("Writing TOC....");

            onMsgUpdateChanged?.Invoke(msg);

            cpk.WriteITOC(newCPK);
            cpk.WriteTOC(newCPK);
            cpk.WriteETOC(newCPK);
            cpk.WriteGTOC(newCPK);

            newCPK.Close();
            oldFile.Close();
            msg = string.Format("Saving CPK to {0}....", outputName);
            onMsgUpdateChanged?.Invoke(msg);
            Debug.Print(msg);
            onCompleteChanged.Invoke();
        }
    }
}
