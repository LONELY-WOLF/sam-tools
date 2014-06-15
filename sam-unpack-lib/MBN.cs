using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileLib;
using System.IO;

namespace sam_unpack_lib
{
    public class MBN
    {
        public string Version, SubVersion;
        const UInt32 StartMark = 0x5955C5C1, EndMark = 0x5955C5C2;
        BinaryReader br;
        BinaryWriter bw;
        FileStream mbnFile;
        public List<Section> Sections = new List<Section>();

        static byte[] oSign = new byte[4] { 0xC1, 0xC5, 0x55, 0x59 }, cSign = new byte[4] { 0xC2, 0xC5, 0x55, 0x59 };
        

        public struct File
        {
            public string Name;
            public UInt32 Offset, Length, Checksum;
        }

        public struct Section
        {
            public string Name;
            public UInt32 Offset, Length;
            public File[] Files;

            public string FName
            {
                get
                {
                    return Name;
                }
            }

            public string FInfo
            {
                get
                {
                    string RetVal = "Files:\r\n";
                    foreach (MBN.File file in Files)
                    {
                        RetVal += file.Name + "\r\n";
                    }
                    return RetVal;
                }
            }
        }

        public void Load(string fileName)
        {
            Section section;
            UInt32 sectionsCount, filesCount;
            long start, end, fileRecordOffset, curFilePointer;
            mbnFile = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read);
            br = new BinaryReader(mbnFile, Encoding.ASCII);
            mbnFile.Position = 4;
            sectionsCount = br.ReadUInt32();
            mbnFile.Position = 0x20;
            Version= Encoding.ASCII.GetString(br.ReadBytes(0x10).TakeWhile(x => x != 0x00).ToArray());
            SubVersion = Encoding.ASCII.GetString(br.ReadBytes(0x3));
            end = FindEndMark(0);
            //Sections
            Sections.Clear();
            for (int i = 0; i < sectionsCount; i++)
            {
                section = new Section();
                start = FindStartMark(end);
                end = FindEndMark(start + 4);
                mbnFile.Position = start + 4;
                filesCount = br.ReadUInt32();
                section.Files = new File[filesCount];
                section.Name = Encoding.ASCII.GetString(br.ReadBytes(0x3));
                //Files
                fileRecordOffset = start + 0x20;
                curFilePointer = end + 4;
                for (int f = 0; f < filesCount; f++)
                {
                    mbnFile.Position = fileRecordOffset;
                    section.Files[f].Offset = (UInt32)curFilePointer;
                    section.Files[f].Length = br.ReadUInt32();
                    mbnFile.Position += 0x08;
                    section.Files[f].Name = Encoding.ASCII.GetString(br.ReadBytes(0x40).TakeWhile(x => x != 0x00).ToArray());
                    curFilePointer += section.Files[f].Length;
                    fileRecordOffset += 0x50;
                }
                Sections.Add(section);
            }
        }

        public void ExtractSection(MBN.Section section, string folder)
        {
            Directory.CreateDirectory(Path.Combine(folder, section.Name));
            foreach (MBN.File file in section.Files)
            {
                ExtractFile(file, Path.Combine(folder, section.Name));
            }
        }

        public void ExtractFile(MBN.File file, string folder)
        {
            FileStream destFile = new FileStream(Path.Combine(folder, file.Name), FileMode.Create);
            FileIO.StreamCopy(mbnFile, destFile, file.Offset, file.Length);
            destFile.Close();
        }

        private long FindStartMark(long start)
        {
            return FileIO.FileSearch(mbnFile, oSign, start);
        }

        private long FindEndMark(long start)
        {
            return FileIO.FileSearch(mbnFile, cSign, start);
        }
    }
}
