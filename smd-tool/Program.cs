using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using sam_unpack_lib;

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
                                    List<SMD.Section> sections = SMD.GetSections(args[1]);
                                    foreach (SMD.Section part in sections)
                                    {
                                        Console.Write("{0:X8} {1:X8} {2:X8} {3:X8} ", part.ROMOffset, part.ROMLength, part.FileOffset, part.FileLength);
                                        Console.Write("{0:X8} {1:X8} [      ]", part.ID, part.FS);
                                        if (part.Extract(args[1], dir))
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
                                    }
                                    return 0;
                                }
                                else
                                {
                                    PrintError("SMD file not found!");
                                    return -1;
                                }
                            }
                            return 0;
                        }
                    case "/p":
                        {
                            //Pack
                            Console.WriteLine("Not implelemented yet");
                            return -1;
                        }
                    case "/h":
                        {
                            //Parse header
                            if (args.Count() >= 2)
                            {
                                if (File.Exists(args[1]))
                                {
                                    List<SMD.Section> sections = SMD.GetSections(args[1]);
                                    foreach (SMD.Section part in sections)
                                    {
                                        Console.Write("{0:X8} {1:X8} {2:X8} {3:X8} ", part.ROMOffset, part.ROMLength, part.FileOffset, part.FileLength);
                                        Console.Write("{0:X8} {1:X8} [      ]", part.ID, part.FS);
                                        if ((part.IsPresentSignature == 0x1F1F1F1F) & (part.FileOffset != 0) & (part.FileLength != 0))
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
                                            Console.Write("NoData");
                                            Console.ResetColor();
                                            Console.CursorLeft += 1;
                                        }
                                        Console.WriteLine();
                                    }
                                    return 0;
                                }
                                else
                                {
                                    PrintError("SMD file not found!");
                                    return -1;
                                }
                            }
                            return 0;
                        }
                    default:
                        {
                            PrintHelp();
                            return -1;
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
