using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using FileLib;
using sam_unpack_lib;

namespace image_rebase
{
    class Program
    {
        static int Main(string[] args)
        {
            string binFileName = null, imgFileName = null;
            long binFilePos = 8, imgFilePos, partSize;

            binFileName = GetArg(args, "/u", null);
            if (binFileName != null)
            {
                imgFileName = GetArg(args, "/o", null);
                if (imgFileName == null)
                {
                    if (binFileName.EndsWith(".bin"))
                    {
                        imgFileName = binFileName.Substring(0, binFileName.Length - 4) + ".img";
                    }
                    else
                    {
                        imgFileName = binFileName + ".img";
                    }
                }
                DiscreteImage image = new DiscreteImage(binFileName);
                Console.WriteLine("Parts count: {0}", image.Sections.Count);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("     №   From     To       Count");
                Console.ResetColor();
                int i = 0;
                foreach (DiscreteImage.Section section in image.Sections)
                {
                    imgFilePos = section.Position;
                    partSize = section.Length;
                    Console.WriteLine("Part {0:d3} {1:X8} {2:X8} {3:X8}", i, binFilePos, imgFilePos, partSize);
                    binFilePos += partSize;
                    i++;
                }
                image.Extract(imgFileName);
                return 0;
            }
            binFileName = GetArg(args, "/p", null);
            if (binFileName != null)
            {
                PrintError("Not implemented yet");
                return -1;
            }
            binFileName = GetArg(args, "/info", null);
            if (binFileName != null)
            {
                DiscreteImage image = new DiscreteImage(binFileName);
                Console.WriteLine("Parts count: {0}", image.Sections.Count);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("     №   From     To       Count");
                Console.ResetColor();
                int i = 0;
                foreach(DiscreteImage.Section section in image.Sections)
                {
                    imgFilePos = section.Position;
                    partSize = section.Length;
                    Console.WriteLine("Part {0:d3} {1:X8} {2:X8} {3:X8}", i, binFilePos, imgFilePos, partSize);
                    binFilePos += partSize;
                    i++;
                }
                return 0;
            }
            PrintUsage();
            return -1;
        }

        static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        static void PrintUsage()
        {
            //usage
            Console.WriteLine(@"
USAGE:

image-rebase /u <input file> [/o <output file>]
    Extract image from <input file> to <output file>

image-rebase /p <input file> [/o <output file>]
    Pack image <input file> to <output file>

image-rebase /info <file>
    Parse header in <file> and display info

(c) -WOLF- 2013
");
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
