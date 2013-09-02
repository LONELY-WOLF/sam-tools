using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace smd_tool
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Count() > 0)
            {
                switch (args[0])
                {
                    case "/u":
                        {
                            //Unpack
                            if (args.Count() >= 2)
                            {
                                if (File.Exists(args[1]))
                                {
                                    string dir = "";
                                    if (args.Count() >= 3)
                                    {
                                        if (Directory.Exists(args[2]))
                                        {
                                            dir = args[2];
                                        }
                                        else
                                        {
                                            PrintError("Directory not found!");
                                            return -1;
                                        }
                                    }
                                    if (dir == "") dir = Environment.CurrentDirectory;
                                    FileStream smd = File.OpenRead(args[1]);
                                    byte[] buffer = new byte[0x800];
                                    smd.Read(buffer, 0, 0x800);
                                    File.WriteAllBytes(Path.Combine(dir, "\\header.bin"), buffer);
                                    SMDPart part = new SMDPart();
                                    int offset = 0x200;
                                    Console.ForegroundColor = ConsoleColor.White;
                                    Console.WriteLine("Partition name   N Offset N Length R Offset R Length Part. ID FS ID");
                                    Console.ResetColor();
                                    while (buffer[offset] != 0)
                                    {
                                        part.ReadHeader(buffer, offset);
                                        Console.Write(part.Name.PadRight(17));
                                        Console.Write("{0:X8} {1:X8} {2:X8} {3:X8} ", part.NandOffset, part.NandLenth, part.FileOffset, part.FileLenth);
                                        Console.Write("{0:X8} {1:X8} [      ]", part.ID, part.FSType);
                                        if (part.Extract(smd, dir))
                                        {
                                            Console.CursorLeft -= 5;
                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.Write("OK");
                                            Console.ResetColor();
                                            Console.CursorLeft += 3;
                                        }
                                        else
                                        {
                                            Console.CursorLeft -= 7;
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.Write("Failed");
                                            Console.ResetColor();
                                            Console.CursorLeft += 1;
                                        }
                                        Console.WriteLine();
                                        offset += 0x40;
                                    }
                                    return 0;
                                }
                                else
                                {
                                    PrintError("SMD file not found!");
                                    return -1;
                                }
                            }
                            break;
                        }
                    case "/p":
                        {
                            //Pack
                            Console.WriteLine("Not implelemented yet");
                            return -1;
                            break;
                        }
                    case "/h":
                        {
                            //Parse header
                            if (args.Count() >= 2)
                            {
                                if (File.Exists(args[1]))
                                {
                                    FileStream smd = File.OpenRead(args[1]);
                                    byte[] buffer = new byte[0x800];
                                    smd.Read(buffer, 0, 0x800);
                                    SMDPart part = new SMDPart();
                                    int offset = 0x200;
                                    Console.ForegroundColor = ConsoleColor.White;
                                    Console.WriteLine("Partition name   N Offset N Length R Offset R Length Part. ID FS ID");
                                    Console.ResetColor();
                                    while (buffer[offset] != 0)
                                    {
                                        part.ReadHeader(buffer, offset);
                                        Console.Write(part.Name.PadRight(17));
                                        Console.Write("{0:X8} {1:X8} {2:X8} {3:X8} ", part.NandOffset, part.NandLenth, part.FileOffset, part.FileLenth);
                                        Console.Write("{0:X8} {1:X8} [      ]", part.ID, part.FSType);
                                        if ((part.FileOffset != 0) & (part.FileLenth != 0))
                                        {
                                            Console.CursorLeft -= 5;
                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.Write("OK");
                                            Console.ResetColor();
                                            Console.CursorLeft += 3;
                                        }
                                        else
                                        {
                                            Console.CursorLeft -= 7;
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.Write("Failed");
                                            Console.ResetColor();
                                            Console.CursorLeft += 1;
                                        }
                                        Console.WriteLine();
                                        offset += 0x40;
                                    }
                                    return 0;
                                }
                                else
                                {
                                    PrintError("SMD file not found!");
                                    return -1;
                                }
                            }
                            break;
                        }
                    default:
                        {
                            PrintHelp();
                            return -1;
                            break;
                        }
                }
            }
            //Unknown arguments
            PrintHelp();
            return -1;
        }

        static void PrintHelp()
        {
            Console.Write(@"
USAGE:

smd-tool /u <file> [<path>]
    Unpack SMD file

smd-tool /p <file> [<path>]
    Pack partitions located at <path> into SMD file <file>

smd-tool /h <file>
    Parse header in <file> and display info

Default <path> is current directory

(c) -WOLF- 2013
");
        }

        static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
