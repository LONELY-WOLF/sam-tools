using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileLib;
using System.IO;

namespace sam_unpack_lib
{
    class MBN
    {
        public string Version, SubVersion;
        static const UInt32 StartMark = 0x5955C5C1, EndMark = 0x5955C5C2;
        BinaryReader br;
        BinaryWriter bw;
        FileStream mbnFile;

        static byte[] oSign = new byte[4] { 0xC1, 0xC5, 0x55, 0x59 }, cSign = new byte[4] { 0xC2, 0xC5, 0x55, 0x59 };

        public struct Section
        {
            public string Name;
            public UInt32 Offset, Length;
            public File[] Files;
        }

        public struct File
        {
            public string Name;
            public UInt32 Offset, Length, Checksum;

            public File()
            {
                Name = "";
                Offset = 0;
                Length = 0;
                Checksum = 0;
            }
        }

        public List<Section> Sections = new List<Section>();

        public void Load(string fileName)
        {
            Section section;
            string sectionName;
            UInt32 sectionsCount, filesCount, fileLength;
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
                curFilePointer = end;
                for (int f = 0; f < filesCount; f++)
                {
                    mbnFile.Position = fileRecordOffset;
                    section.Files[f].Offset = (UInt32)fileRecordOffset;
                    section.Files[f].Length = br.ReadUInt32();
                    mbnFile.Position += 0x08;
                    fileName = Encoding.ASCII.GetString(br.ReadBytes(0x40).TakeWhile(x => x != 0x00).ToArray());
                    curFilePointer += section.Files[f].Length;
                    fileRecordOffset += 0x50;
                }
                Sections.Add(section);
            }
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
