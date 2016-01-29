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
                Console.WriteLine("     №   From     To       Length");
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
            binFileName = GetArg(args, "/s", null);
            if (binFileName != null)
            {
                string minZeroSectors = GetArg(args, "/z", "2000");
                List<DiscreteImage.Section> sections = DiscreteImage.Slice(binFileName, binFileName, Int32.Parse(minZeroSectors));
                StreamWriter template = new StreamWriter(binFileName + ".xml", false);
                template.WriteLine("<image-rebase>");
                template.WriteLine("\t<template>");
                Console.WriteLine("Sliced to:");
                foreach (DiscreteImage.Section item in sections)
                {
                    Console.WriteLine("{0:X8} {1:X8} {2}", item.Position, item.Length, item.File);
                    template.WriteLine("\t\t<section start=\"{0}\" length=\"{1}\" file=\"{2}\"/>", item.Position, item.Length, item.File);
                }
                template.WriteLine("\t</template>");
                template.WriteLine("</image-rebase>");
                template.Close();
                return 0;
            }
            binFileName = GetArg(args, "/p", null);
            if (binFileName != null)
            {
                string templateFileName = GetArg(args, "/t", null);
                if (templateFileName != null)
                {
                    DiscreteImage.Pack(binFileName, templateFileName);
                    return 0;
                }
                else
                {
                    PrintError("No template selected");
                    return -1;
                }
            }
            binFileName = GetArg(args, "/pseudo", null);
            if (binFileName != null)
            {
                DiscreteImage.Section sect = DiscreteImage.SectionFromFile(binFileName);
                StreamWriter template = new StreamWriter(binFileName + ".xml", false);
                template.WriteLine("<image-rebase>");
                template.WriteLine("\t<template>");
                Console.WriteLine("Sliced to:");
                Console.WriteLine("{0:X8} {1:X8} {2}", sect.Position, sect.Length, sect.File);
                template.WriteLine("\t\t<section start=\"{0}\" length=\"{1}\" file=\"{2}\"/>", sect.Position, sect.Length, sect.File);
                template.WriteLine("\t</template>");
                template.WriteLine("</image-rebase>");
                template.Close();
                return 0;
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

image-rebase /s <input file> /z <length>
    Slice file cutting out zeroes areas larger then <length> sectors

image-rebase /p <output file> /t <template file>
    Pack image <output file> using <template file> as a recipe

image-rebase /pseudo <input file>
    Make template file for entire <input file>

image-rebase /info <file>
    Parse header in <file> and display info

(c) -WOLF- 2013-2016");
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
