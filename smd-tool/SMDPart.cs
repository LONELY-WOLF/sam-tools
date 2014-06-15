using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace smd_tool
{
    /*class SMDPart
    {
        public UInt32 NandOffset, NandLenth, FileOffset, FileLenth, FSType, ID, IsPresentSignature;
        public string Name;
        public byte[] MD5;

        public SMDPart() { }

        public void ReadHeader(byte[] header, int offset)
        {
            Name = Encoding.ASCII.GetString(header, offset, 0x10).TrimEnd(new char[] { '\0' });
            NandOffset = BitConverter.ToUInt32(header, offset + 0x10);
            NandLenth = BitConverter.ToUInt32(header, offset + 0x14);
            FileOffset = BitConverter.ToUInt32(header, offset + 0x18);
            FileLenth = BitConverter.ToUInt32(header, offset + 0x1C);
            IsPresentSignature = BitConverter.ToUInt32(header, offset + 0x20);
            ID = BitConverter.ToUInt32(header, offset + 0x24);
            FSType = BitConverter.ToUInt32(header, offset + 0x28);
            MD5 = header.Skip(offset + 0x30).Take(0x10).ToArray();
        }

        public bool Extract(FileStream smd, string path)
        {
            if ((IsPresentSignature == 0x1F1F1F1F) & (FileOffset != 0) & (FileLenth != 0))
            {
                try
                {
                    FileStream outFile = File.Open(Path.Combine(path, Name + ".bin"), FileMode.Create);
                    byte[] buffer = new byte[1024 * 1024];
                    smd.Position = FileOffset;
                    long count;
                    while (true)
                    {
                        count = (long)(FileOffset + FileLenth - smd.Position);
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
                }
                catch (Exception ex)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }*/
}
