﻿using System;
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
            string dir, filename;
            if (HasFlag(args, "/u"))
            {
                //Unpack
                filename = GetArg(args, "/u", null);
                dir = GetArg(args, "/d", ".\\");
                if (File.Exists(filename))
                {
                    if (!Directory.Exists(dir))
                    {
                        PrintError("Directory not found!");
                    }
                    List<SMD.Section> sections = SMD.GetSections(filename);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Partition name   NAND off N size   ROM off  R size   Part. ID Type     Status");
                    Console.ResetColor();
                    foreach (SMD.Section part in sections)
                    {
                        Console.Write(part.Name.PadRight(17));
                        Console.Write("{0:X8} {1:X8} {2:X8} {3:X8} ", part.ROMOffset, part.ROMLength, part.FileOffset, part.FileLength);
                        Console.Write("{0:X8} {1:X8} [ .... ]", part.ID, part.FS);
                        if (part.Extract(filename, dir))
                        {
                            Console.CursorLeft -= 7;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("  OK  ");
                            Console.ResetColor();
                            Console.CursorLeft += 1;
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
            if (HasFlag(args, "/p"))
            {
                //Pack
                filename = GetArg(args, "/p", null);
                string template = GetArg(args, "/t", null);
                SMD.Pack(filename, template);
                return -1;
            }
            if (HasFlag(args, "/info"))
            {
                filename = GetArg(args, "/info", null);
                //Parse header
                if (File.Exists(filename))
                {
                    List<SMD.Section> sections = SMD.GetSections(filename);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Partition name   NAND off N size   ROM off  R size   Part. ID Type     Status");
                    Console.ResetColor();
                    foreach (SMD.Section part in sections)
                    {
                        Console.Write(part.Name.PadRight(17));
                        Console.Write("{0:X8} {1:X8} {2:X8} {3:X8} ", part.ROMOffset, part.ROMLength, part.FileOffset, part.FileLength);
                        Console.Write("{0:X8} {1:X8} [ .... ]", part.ID, part.FS);
                        if ((part.IsPresent) & (part.FileOffset != 0) & (part.FileLength != 0))
                        {
                            Console.CursorLeft -= 7;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("  OK  ");
                            Console.ResetColor();
                            Console.CursorLeft += 1;
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
            //Unknown arguments
            PrintHelp();
            return -1;
        }

        static void PrintHelp()
        {
            Console.Write(@"
USAGE:

smd-tool /u <file> [/d <path>]
    Unpack SMD file

smd-tool /p <file> /t <template>
    Pack partitions into SMD <file> using <template>

smd-tool /info <file>
    Parse header in <file> and display info

Default <path> is current directory

(c) -WOLF- 2013-2016
");
        }

        static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        static string GetArg(string[] args, string name, string defaultValue)
        {
            int index = -2;
            int count = args.Count();
            for (int i = 0; i < count; i++)
            {
                if (args[i] == name)
                {
                    index = i;
                    break;
                }
            }
            if ((index + 1 == count) || (index < 0))
            {
                return defaultValue;
            }
            else
            {
                return args[index + 1];
            }
        }

        static bool HasFlag(string[] args, string name)
        {
            int count = args.Count();
            for (int i = 0; i < count; i++)
            {
                if (args[i] == name)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
