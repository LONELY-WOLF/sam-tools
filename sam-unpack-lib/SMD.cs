using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace sam_unpack_lib
{
    public static class SMD
    {
        public struct Section
        {
            public string Name;
            public uint FileOffset, FileLength, ROMOffset, ROMLength, ID, FS, IsPresentSignature;
            public byte[] MD5;

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
                    return  "Offset in file: 0x" + FileOffset.ToString("X8") +
                            "\nLength in file: 0x" + FileLength.ToString("X8") +
                            "\nOffset in ROM: 0x" + ROMOffset.ToString("X8") +
                            "\nLength in ROM: 0x" + ROMLength.ToString("X8") +
                            "\nPartition ID: 0x" + ID.ToString("X8") +
                            "\nAttributes: 0x" + FS.ToString("X8");
                }
            }
        }

        public static List<Section> GetSections(string fileName, bool onlyAvaible)
        {
            FileStream smd = File.OpenRead(fileName);
            List<Section> sections = new List<Section>();
            byte[] buffer = new byte[0x800];
            smd.Read(buffer, 0, 0x800);
            Section part;
            int offset = 0x200;
            while (buffer[offset] != 0)
            {
                part = new Section();
                part.Name = Encoding.ASCII.GetString(buffer, offset, 0x10).TrimEnd(new char[] { '\0' });
                part.ROMOffset = BitConverter.ToUInt32(buffer, offset + 0x10);
                part.ROMLength = BitConverter.ToUInt32(buffer, offset + 0x14);
                part.FileOffset = BitConverter.ToUInt32(buffer, offset + 0x18);
                part.FileLength = BitConverter.ToUInt32(buffer, offset + 0x1C);
                part.IsPresentSignature = BitConverter.ToUInt32(buffer, offset + 0x20);
                part.ID = BitConverter.ToUInt32(buffer, offset + 0x24);
                part.FS = BitConverter.ToUInt32(buffer, offset + 0x28);
                part.MD5 = buffer.Skip(offset + 0x30).Take(0x10).ToArray();
                if (onlyAvaible)
                {
                    if ((part.IsPresentSignature == 0x1F1F1F1F) & (part.FileOffset != 0) & (part.FileLength != 0))
                    {
                        sections.Add(part);
                    }
                }
                else
                {
                    sections.Add(part);
                }
                offset += 0x40;
            }
            return sections;
        }

        public static void Extract(string fileName, string path, List<Section> sections)
        {
            foreach (Section part in sections)
            {
                if ((part.IsPresentSignature == 0x1F1F1F1F) & (part.FileOffset != 0) & (part.FileLength != 0))
                {
                    FileStream smd = new FileStream(fileName, FileMode.Open);
                    FileStream outFile = File.Open(Path.Combine(path, part.Name + ".bin"), FileMode.Create);
                    byte[] buffer = new byte[1024 * 1024];
                    smd.Position = part.FileOffset;
                    int count;
                    while (true)
                    {
                        count = (int)(part.FileOffset + part.FileLength - smd.Position);
                        if (count == 0)
                        {
                            break;
                        }
                        if (count > buffer.Length) count = buffer.Length;
                        int read = smd.Read(buffer, 0, count);
                        if (read <= 0)
                        {
                            break;
                        }
                        outFile.Write(buffer, 0, read);
                    }
                    outFile.Close();
                }
            }
        }

        public static void Extract(string fileName, string path)
        {
            Extract(fileName, path, GetSections(fileName, true));
        }
    }
}
