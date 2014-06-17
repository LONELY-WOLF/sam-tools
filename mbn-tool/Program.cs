using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using FileLib;
using sam_unpack_lib;

namespace mbn_tool
{
    class Program
    {
        static void Main(string[] args)
        {
            string filename, folder;
            if (HasFlag(args, "/info"))
            {
                filename = GetArg(args, "/info", null);
                MBN mbn = new MBN(filename);
                Console.WriteLine("Firmware version: " + mbn.Version);
                Console.WriteLine("Firmware subversion: " + mbn.SubVersion);
                Console.WriteLine("Sections count: " + mbn.Sections.Count.ToString());
                //Sections
                foreach (MBN.Section section in mbn.Sections)
                {
                    Console.WriteLine("Section name: " + section.Name);
                    Console.WriteLine("Files count: " + section.Files.Count().ToString());
                    //Files
                    foreach (MBN.File file in section.Files)
                    {
                        Console.WriteLine("File: " + file.Name);
                        Console.WriteLine("Length: " + file.Length.ToString());
                    }
                }
                return;
            }
            if (HasFlag(args, "/u"))
            {
                filename = GetArg(args, "/u", null);
                folder = GetArg(args, "/d", "csc");
                MBN mbn = new MBN(filename);
                mbn.Extract(folder);
                return;
            }
            if (HasFlag(args, "/p"))
            {
                string ver, subver;
                folder = GetArg(args, "/p", null);
                filename = GetArg(args, "/f", folder + ".mbn");
                ver = GetArg(args, "/ver", "I8750OXXCMK2");
                subver = GetArg(args, "/subver", "OXX");
                if (!MBN.Pack(filename, folder, ver, subver))
                {
                    PrintError("No such directory");
                }
                return;
            }
            //usage
            PrintUsage();
        }

        static void PrintUsage()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(@"USAGE:

/u <file> [/d <path>]");
            Console.ResetColor();
            Console.WriteLine(@"    Unpack MBN file
    Default <path> is .\csc
");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("/p <path> [/f <file>] [/ver <version>] [/subver <subversion>]");
            Console.ResetColor();
            Console.WriteLine(@"    Pack files located at <path> into MBN file <file>
    Default <file> value is <path>.mbn
    Ex.: mbn-tool /p csc /f my.mbn /ver I8750OXXCMK2 /subver OXX
");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("/info <file>");
            Console.ResetColor();
            Console.WriteLine(@"    Parse header in <file> and display info

(c) -WOLF- 2013-2014");
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