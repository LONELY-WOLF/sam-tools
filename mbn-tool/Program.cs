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
                                MBN mbn = new MBN(args[1]);
                                mbn.Extract(folder);
                                break;
                            }
                        case "/p":
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
                                if (!MBN.Pack(filename, args[1], "I8750OXXCMK2", "OXX"))
                                {
                                    PrintError("No such directory");
                                }
                                break;
                            }
                        case "/h":
                            {
                                MBN mbn = new MBN(args[1]);
                                Console.WriteLine("Firmware version: " + mbn.Version);
                                Console.WriteLine("Firmware subversion: " + mbn.SubVersion);
                                Console.WriteLine("Sections count: " + mbn.Sections.Count.ToString());
                                //Sections
                                foreach (MBN.Section section in mbn.Sections)
                                {
                                    Console.WriteLine("Section name: " + section.Name);
                                    Console.WriteLine("Files count: " + section.Files.Count().ToString());
                                    //Files
                                    foreach(MBN.File file in section.Files)
                                    {
                                        Console.WriteLine("File: " + file.Name);
                                        Console.WriteLine("Length: " + file.Length.ToString());
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
