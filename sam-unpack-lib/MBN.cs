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
        string fileName;
        public string Version, SubVersion;
        const UInt32 StartMark = 0x5955C5C1, EndMark = 0x5955C5C2;
        BinaryReader br;
        BinaryWriter bw;
        FileStream mbnFile;
        public List<Section> Sections;

        static byte[] oSign = new byte[4] { 0xC1, 0xC5, 0x55, 0x59 }, cSign = new byte[4] { 0xC2, 0xC5, 0x55, 0x59 };
        private string p;

        public class File
        {
            public string Name;
            public UInt32 Offset, Length, Checksum;

            public void Extract(FileStream mbnFile, string folder)
            {
                FileStream destFile = new FileStream(Path.Combine(folder, Name), FileMode.Create);
                FileIO.StreamCopy(mbnFile, destFile, Offset, Length);
                destFile.Close();
            }
        }

        public class Section
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

            public void Extract(FileStream mbnFile, string folder)
            {
                Directory.CreateDirectory(Path.Combine(folder, Name));
                foreach (MBN.File file in Files)
                {
                    file.Extract(mbnFile, Path.Combine(folder, Name));
                }
            }
        }

        public MBN(string fileName)
        {
            Section section;
            UInt32 sectionsCount, filesCount;
            long start, end, fileRecordOffset, curFilePointer;
            mbnFile = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read);
            br = new BinaryReader(mbnFile, Encoding.ASCII);
            mbnFile.Position = 4;
            sectionsCount = br.ReadUInt32();
            mbnFile.Position = 0x20;
            Version = Encoding.ASCII.GetString(br.ReadBytes(0x10).TakeWhile(x => x != 0x00).ToArray());
            SubVersion = Encoding.ASCII.GetString(br.ReadBytes(0x3));
            end = FindEndMark(0);
            //Sections
            Sections = new List<Section>();
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
            this.fileName = fileName;
        }

        public void Extract(string folder)
        {
            foreach (Section section in Sections)
            {
                section.Extract(mbnFile, folder);
            }
        }

        public static bool Pack(string filename, string folder, string version, string subversion)
        {
            if (Directory.Exists(folder))
            {
                int recordOffset, dataOffset, filesCount, i, sum;
                FileStream mbnFile = new FileStream(filename, FileMode.Create);
                FileStream inputFile;
                BinaryWriter writer = new BinaryWriter(mbnFile, Encoding.ASCII);
                writer.Write(0x5955C5C1);
                writer.Write(Directory.GetDirectories(folder).Count());
                FileIO.WriteZeroes(mbnFile, 8, 0x34, 0x34);
                writer.Write(0x5955C5C2);
                mbnFile.Position = 0x20;
                writer.Write(Encoding.ASCII.GetBytes(version));
                mbnFile.Position = 0x30;
                writer.Write(Encoding.ASCII.GetBytes(subversion));
                mbnFile.Position = 0x40;
                foreach (string cscDir in Directory.GetDirectories(folder))
                {
                    if (Path.GetFileName(cscDir).Length == 3)
                    {
                        writer.Write(0x5955C5C1);
                        recordOffset = (int)mbnFile.Position;
                        FileIO.WriteZeroes(mbnFile, mbnFile.Position, 0x298, 0x298);
                        writer.Write(0x5955C5C2);
                        dataOffset = (int)mbnFile.Position;
                        mbnFile.Position = recordOffset;
                        filesCount = Directory.GetFiles(cscDir).Count();
                        writer.Write(filesCount);
                        writer.Write(Encoding.ASCII.GetBytes(Path.GetFileName(cscDir)));
                        mbnFile.Position += 17;
                        i = 1;
                        foreach (string file in Directory.GetFiles(cscDir))
                        {
                            inputFile = new FileStream(file, FileMode.Open);
                            switch (Path.GetExtension(file))
                            {
                                case ".ini":
                                case ".reg":
                                    {
                                        writer.Write(0x0B);
                                        break;
                                    }
                                case ".provxml":
                                    {
                                        writer.Write(0x1A);
                                        break;
                                    }
                                case ".xml":
                                    {
                                        writer.Write(0x0F);
                                        break;
                                    }
                                default:
                                    {
                                        writer.Write(0x00);
                                        break;
                                    }
                            }
                            writer.Write((int)inputFile.Length);
                            recordOffset = (int)mbnFile.Position;
                            mbnFile.Position = dataOffset;
                            FileIO.StreamCopy(inputFile, mbnFile, 0, inputFile.Length, out sum);
                            dataOffset = (int)mbnFile.Position;
                            mbnFile.Position = recordOffset;
                            writer.Write(sum);
                            writer.Write(i);
                            recordOffset = (int)mbnFile.Position;
                            writer.Write(Encoding.ASCII.GetBytes(Path.GetFileName(file)));
                            mbnFile.Position = (recordOffset + 0x40);
                            if (i != filesCount - 1)
                            {
                                i++;
                            }
                            else
                            {
                                i = 0;
                            }
                        }
                        mbnFile.Position = dataOffset;
                    }
                }
                return true;
            }
            else
            {
                return false;
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
