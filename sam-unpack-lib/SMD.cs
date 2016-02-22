using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using FileLib;
using System.Security.Cryptography;

namespace sam_unpack_lib
{
    public static class SMD
    {
        public const long TABLE_START = 0x200, DATA_START = 0x200C00;
        public const uint IS_PRESENT_SIGNATURE = 0x1F1F1F1F, IS_PACKED_SIGNATURE = 0xEACCE221;

        public class Section
        {
            public string Name;
            internal string FileName;
            public uint FileOffset, FileLength, ROMOffset, ROMLength, ID, FS, IsPresentSignature;
            public byte[] MD5;

            public bool IsPacked
            {
                get
                {
                    return FS == IS_PACKED_SIGNATURE;
                }
                set
                {
                    FS = value ? IS_PACKED_SIGNATURE : 0;
                }
            }

            public bool IsPresent
            {
                get
                {
                    return IsPresentSignature == IS_PRESENT_SIGNATURE;
                }
                set
                {
                    IsPresentSignature = value ? IS_PRESENT_SIGNATURE : 0;
                }
            }

            /*public string FName
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
            }*/

            public bool Extract(string fileName, string path)
            {
                if ((IsPresent) & (FileOffset != 0) & (FileLength != 0))
                {
                    FileStream smd = File.OpenRead(fileName);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    FileStream outFile = File.Open(Path.Combine(path, Name + ".bin"), FileMode.Create);
                    byte[] buffer = new byte[1024 * 1024];
                    smd.Position = FileOffset;
                    long count;
                    while (true)
                    {
                        count = FileOffset + FileLength - smd.Position;
                        if (count == 0)
                        {
                            break;
                        }
                        if (count > buffer.Length) count = buffer.Length;
                        int read = smd.Read(buffer, 0, (int)count);
                        if (read <= 0)
                        {
                            break;
                        }
                        outFile.Write(buffer, 0, read);
                    }
                    outFile.Close();
                    smd.Close();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void WriteHeader(Stream dest)
            {
                long start = dest.Position;
                BinaryWriter bw = new BinaryWriter(dest, Encoding.ASCII, true);
                bw.Write(Encoding.ASCII.GetBytes(Name));
                dest.Position = start + 0x10;
                bw.Write(ROMOffset);
                bw.Write(ROMLength);
                bw.Write(FileOffset);
                bw.Write(FileLength);
                bw.Write(IsPresentSignature);
                bw.Write(ID);
                bw.Write(FS);
                bw.Flush();
            }
        }

        public static List<Section> GetSections(string fileName, bool onlyAvaible = false)
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
                part.Name = part.Name.Replace('\0', ' ');
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
                    if ((part.IsPresent) & (part.FileOffset != 0) & (part.FileLength != 0))
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
            smd.Close();
            return sections;
        }

        public static void Extract(string fileName, string path, List<Section> sections)
        {
            foreach (Section part in sections)
            {
                part.Extract(fileName, path);
            }
        }

        public static void Extract(string fileName, string path)
        {
            Extract(fileName, path, GetSections(fileName, true));
        }

        public static void Pack(string smdFile, string templateFile)
        {
            FileStream output = new FileStream(smdFile, FileMode.Create, FileAccess.ReadWrite);
            FileStream input;
            XDocument template = XDocument.Load(templateFile);
            if (template.Root.Name != "smd-tool")
            {
                throw new Exception("Invalid template file");
            }
            XElement slist = template.Root.Element(XName.Get("template"));
            if (slist == null)
            {
                throw new Exception("Invalid template file");
            }
            BinaryWriter bw = new BinaryWriter(output);
            //Write header
            XElement header = slist.Element(XName.Get("header"));
            if (header == null)
            {
                throw new Exception("Header is missing");
            }
            input = new FileStream(header.Attribute(XName.Get("file")).Value, FileMode.Open, FileAccess.Read);
            FileIO.StreamCopy(input, output, 0, 0x50);
            input.Close();
            FileIO.WriteZeroes(output, 0x50, DATA_START - 0x50);
            output.Position = 0x200;
            List<Section> sections = new List<Section>();
            uint fileOffset = (uint)DATA_START;
            foreach (XElement xsection in slist.Elements(XName.Get("section")))
            {
                Section section = new Section();
                section.IsPresent = true;
                section.FileOffset = fileOffset;
                section.Name = xsection.Attribute(XName.Get("name")).Value;
                section.FileName = xsection.Attribute(XName.Get("file")).Value;
                section.ROMOffset = Convert.ToUInt32(xsection.Attribute(XName.Get("offset")).Value, 16);
                FileStream stream = new FileStream(section.FileName, FileMode.Open, FileAccess.Read);
                section.FileLength = (uint)stream.Length;
                BinaryReader br = new BinaryReader(stream);
                if (br.ReadUInt64() != DiscreteImage.IS_PACKED_SIGNATURE)
                {
                    section.ROMLength = section.FileLength / 512;
                    section.FileLength += DiscreteImage.HEADER_SIZE;
                    section.IsPacked = false;
                }
                else
                {
                    DiscreteImage image = new DiscreteImage(section.FileName);
                    section.ROMLength = (uint)image.RealSize / 512;
                    section.IsPacked = true;
                }
                fileOffset += section.FileLength;
                br.Close();
                sections.Add(section);
            }
            for (int i = 0; i < sections.Count; i++)
            {
                //Write header
                Section sec = sections[i];
                output.Position = 0x200 + (0x40 * i);
                if (!sec.IsPacked)
                {
                    sec.IsPacked = true; //it will be packed anyway
                    sec.WriteHeader(output);
                    sec.IsPacked = false; //return flag back
                }
                else
                {
                    sec.WriteHeader(output);
                }
                //Write data
                input = new FileStream(sec.FileName, FileMode.Open, FileAccess.Read);
                output.Position = sec.FileOffset;
                if (!sec.IsPacked)
                {
                    //Write header
                    bw.Write(0x7260EACCEACC9442);
                    bw.Write(1u); //count
                    bw.Write(0u);
                    bw.Write(0u); //start
                    bw.Write((uint)input.Length / 512);
                    FileIO.WriteZeroes(output, sec.FileOffset + 24, DiscreteImage.HEADER_SIZE - 24);
                }
                FileIO.StreamCopy(input, output, 0, input.Length, 1024 * 1024);
                input.Close();
            }
            //Compute MD5
            output.Position = 0;
            MD5 hash = MD5.Create();
            hash.ComputeHash(output);
            output.Position = 0x50;
            bw.Write(hash.Hash);
        }
    }
}
