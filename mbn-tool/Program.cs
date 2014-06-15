using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using FileLib;

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
                                FileStream output; //= new FileStream(Path.Combine(folder, "mbn_header.bin"), FileMode.Create);
                                start = FileIO.FileSearch(mbnFile, oSign, 0);
                                end = FileIO.FileSearch(mbnFile, cSign, start) + 4;
                                //FileIO.StreamCopy(mbnFile, output, start, end - start);
                                //output.Close();
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
                                    start = FileIO.FileSearch(mbnFile, oSign, end);
                                    end = FileIO.FileSearch(mbnFile, cSign, start) + 4;
                                    mbnFile.Position = start + 4;
                                    filesCount = reader.ReadUInt32();
                                    sectionName = Encoding.ASCII.GetString(reader.ReadBytes(0x3));
                                    Console.WriteLine("Section name: " + sectionName);
                                    Console.WriteLine("Files count: " + filesCount.ToString());
                                    //output = new FileStream(Path.Combine(folder, sectionName + "_header.bin"), FileMode.Create);
                                    //FileIO.StreamCopy(mbnFile, output, start, end - start);
                                    //output.Close();
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
                                        FileIO.StreamCopy(mbnFile, output, curFilePointer, fileLength);
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
                                if (Directory.Exists(args[1]))
                                {
                                    string filename;
                                    if (args.Count() > 2)
                                    {
                                        filename = args[2];
                                    }
                                    else
                                    {
                                        filename = Path.GetFileName(args[1]) + ".mbn";
                                    }
                                    int recordOffset, dataOffset, filesCount, i, sum;
                                    FileStream mbnFile = new FileStream(filename, FileMode.Create);
                                    FileStream inputFile;
                                    BinaryWriter writer = new BinaryWriter(mbnFile, Encoding.ASCII);
                                    writer.Write(0x5955C5C1);
                                    writer.Write(Directory.GetDirectories(args[1]).Count());
                                    FileIO.WriteZeroes(mbnFile, 8, 0x34, 0x34);
                                    writer.Write(0x5955C5C2);
                                    mbnFile.Position = 0x20;
                                    writer.Write(Encoding.ASCII.GetBytes("I8750OXXCMK2"));
                                    mbnFile.Position = 0x30;
                                    writer.Write(Encoding.ASCII.GetBytes("OXX"));
                                    mbnFile.Position = 0x40;
                                    foreach (string cscDir in Directory.GetDirectories(args[1]))
                                    {
                                        if (Path.GetFileName(cscDir).Length == 3)
                                        {
                                            Console.WriteLine("\r\nPacking: " + Path.GetFileName(cscDir));
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
                                                Console.Write("{0,-69}{1,11}", file, FileIO.FileSizeToString(inputFile.Length));
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
                                }
                                else
                                {
                                    PrintError("No such directory");
                                }
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
                                start = FileIO.FileSearch(mbnFile, oSign, 0);
                                end = FileIO.FileSearch(mbnFile, cSign, start) + 4;
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
                                    start = FileIO.FileSearch(mbnFile, oSign, end);
                                    end = FileIO.FileSearch(mbnFile, cSign, start) + 4;
                                    mbnFile.Position = start + 4;
                                    filesCount = reader.ReadUInt32();
                                    sectionName = Encoding.ASCII.GetString(reader.ReadBytes(0x3));
                                    Console.WriteLine("Section name: " + sectionName);
                                    Console.WriteLine("Files count: " + filesCount.ToString());
                                    //Files
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

smd-tool /p <path> [<file>]
    Pack files located at <path> into MBN file <file>

smd-tool /h <file>
    Parse header in <file> and display info

Default <path> is current directory

(c) -WOLF- 2013
");
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
