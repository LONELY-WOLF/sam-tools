using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace mbn_tool
{
    class Program
    {
        static void Main(string[] args)
        {
            string folder = "";
            if (args.Length > 1)
            {
                try
                {
                    if (args.Length > 2)
                    {
                        folder = args[2];
                    }
                    switch (args[0])
                    {
                        case "/u":
                            {
                                UInt32 sectionsCount;
                                string firmwareVersion;
                                string firmwareSubversion;

                                string sectionName;
                                UInt32 filesCount;
                                long fileRecordOffset, curFilePointer;

                                string fileName;
                                long fileLength;

                                byte[] oSign = new byte[4] { 0xC1, 0xC5, 0x55, 0x59 };
                                byte[] cSign = new byte[4] { 0xC2, 0xC5, 0x55, 0x59 };
                                long start, end;
                                FileStream mbnFile = new FileStream(args[1], FileMode.Open);
                                BinaryReader reader = new BinaryReader(mbnFile, Encoding.ASCII);
                                //Header
                                FileStream output = new FileStream(Path.Combine(folder, "mbn_header.bin"), FileMode.Create);
                                start = FileSearch(mbnFile, oSign, 0);
                                end = FileSearch(mbnFile, cSign, start) + 4;
                                StreamCopy(mbnFile, output, start, end - start);
                                output.Close();
                                mbnFile.Position = start + 4;
                                sectionsCount = reader.ReadUInt32();
                                mbnFile.Position = start + 0x20;
                                firmwareVersion = Encoding.ASCII.GetString(reader.ReadBytes(0x10).TakeWhile(x => x != 0x00).ToArray());
                                firmwareSubversion = Encoding.ASCII.GetString(reader.ReadBytes(0x3));
                                Console.WriteLine("Firmware version: " + firmwareVersion);
                                Console.WriteLine("Firmware subversion: " + firmwareSubversion);
                                Console.WriteLine("Sections count: " + sectionsCount.ToString());
                                //Sections
                                for (int i = 0; i < sectionsCount; i++)
                                {
                                    start = FileSearch(mbnFile, oSign, end);
                                    end = FileSearch(mbnFile, cSign, start) + 4;
                                    mbnFile.Position = start + 4;
                                    filesCount = reader.ReadUInt32();
                                    sectionName = Encoding.ASCII.GetString(reader.ReadBytes(0x3));
                                    Console.WriteLine("Section name: " + sectionName);
                                    Console.WriteLine("Files count: " + filesCount.ToString());
                                    output = new FileStream(folder + sectionName + "_header.bin", FileMode.Create);
                                    StreamCopy(mbnFile, output, start, end - start);
                                    output.Close();
                                    //Files
                                    Directory.CreateDirectory(Path.Combine(folder, sectionName));
                                    fileRecordOffset = start + 0x20;
                                    curFilePointer = end;
                                    for (int f = 0; f < filesCount; f++)
                                    {
                                        mbnFile.Position = fileRecordOffset;
                                        fileLength = reader.ReadUInt32();
                                        mbnFile.Position += 0x08;
                                        fileName = Encoding.ASCII.GetString(reader.ReadBytes(0x40).TakeWhile(x => x != 0x00).ToArray());
                                        output = new FileStream(Path.Combine(folder, sectionName, fileName), FileMode.Create);
                                        StreamCopy(mbnFile, output, curFilePointer, fileLength);
                                        output.Close();
                                        curFilePointer += fileLength;
                                        fileRecordOffset += 0x50;
                                        Console.WriteLine("File: " + fileName);
                                        Console.WriteLine("Length: " + fileLength.ToString());
                                    }
                                }
                                break;
                            }
                        case "/p":
                            {
                                PrintError("Packing isn't implemented yet");
                                break;
                            }
                        case "/h":
                            {
                                UInt32 sectionsCount;
                                string firmwareVersion;
                                string firmwareSubversion;

                                string sectionName;
                                UInt32 filesCount;
                                long fileRecordOffset, curFilePointer;

                                string fileName;
                                long fileLength;

                                byte[] oSign = new byte[4] { 0xC1, 0xC5, 0x55, 0x59 };
                                byte[] cSign = new byte[4] { 0xC2, 0xC5, 0x55, 0x59 };
                                long start, end;
                                FileStream mbnFile = new FileStream(args[1], FileMode.Open);
                                BinaryReader reader = new BinaryReader(mbnFile, Encoding.ASCII);
                                //Header
                                start = FileSearch(mbnFile, oSign, 0);
                                end = FileSearch(mbnFile, cSign, start) + 4;
                                mbnFile.Position = start + 4;
                                sectionsCount = reader.ReadUInt32();
                                mbnFile.Position = start + 0x20;
                                firmwareVersion = Encoding.ASCII.GetString(reader.ReadBytes(0x10).TakeWhile(x => x != 0x00).ToArray());
                                firmwareSubversion = Encoding.ASCII.GetString(reader.ReadBytes(0x3));
                                Console.WriteLine("Firmware version: " + firmwareVersion);
                                Console.WriteLine("Firmware subversion: " + firmwareSubversion);
                                Console.WriteLine("Sections count: " + sectionsCount.ToString());
                                //Sections
                                for (int i = 0; i < sectionsCount; i++)
                                {
                                    start = FileSearch(mbnFile, oSign, end);
                                    end = FileSearch(mbnFile, cSign, start) + 4;
                                    mbnFile.Position = start + 4;
                                    filesCount = reader.ReadUInt32();
                                    sectionName = Encoding.ASCII.GetString(reader.ReadBytes(0x3));
                                    Console.WriteLine("Section name: " + sectionName);
                                    Console.WriteLine("Files count: " + filesCount.ToString());
                                    //Files
                                    Directory.CreateDirectory(Path.Combine(folder, sectionName));
                                    fileRecordOffset = start + 0x20;
                                    curFilePointer = end;
                                    for (int f = 0; f < filesCount; f++)
                                    {
                                        mbnFile.Position = fileRecordOffset;
                                        fileLength = reader.ReadUInt32();
                                        mbnFile.Position += 0x08;
                                        fileName = Encoding.ASCII.GetString(reader.ReadBytes(0x40).TakeWhile(x => x != 0x00).ToArray());
                                        curFilePointer += fileLength;
                                        fileRecordOffset += 0x50;
                                        Console.WriteLine("File: " + fileName);
                                        Console.WriteLine("Length: " + fileLength.ToString());
                                    }
                                }
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                //usage
                Console.WriteLine(@"
USAGE:

smd-tool /u <file> [<path>]
    Unpack MBN file

smd-tool /p <file> [<path>]
    Pack files located at <path> into MBN file <file>

smd-tool /h <file>
    Parse header in <file> and display info

Default <path> is current directory

(c) -WOLF- 2013
");
            }
        }

        static long FileSearch(FileStream source, byte[] data, long startPos)
        {
            int c, i = 0;
            source.Position = startPos;
            while ((c = source.ReadByte()) != -1)
            {
                if (data[i] == (byte)c)
                {
                    i++;
                    if (i == data.Length)
                    {
                        return source.Position - i;
                    }
                }
            }
            return -1;
        }

        static void StreamCopy(Stream source, Stream dest, long start, long count, int bufferSize = 4096)
        {
            long counter = 0;
            int bytesRead, bytesToRead;
            byte[] buffer = new byte[bufferSize];
            source.Seek(start, SeekOrigin.Begin);
            while (counter < count)
            {
                bytesToRead = (int)((count - counter > bufferSize) ? bufferSize : count - counter);
                bytesRead = source.Read(buffer, 0, bytesToRead);
                dest.Write(buffer, 0, bytesRead);
                counter += bytesRead;
            }
        }

        static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
