using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using FileLib;

namespace image_rebase
{
    class Program
    {
        static int Main(string[] args)
        {
            string binFileName = null, imgFileName = null;
            uint partCount = 0;
            long headerPosition = 0x10, binFilePos = 8, imgFilePos, partSize, zeroSize;
            long sectorSize = 512;

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
                FileStream binFile = new FileStream(binFileName, FileMode.Open);
                FileStream imgFile = new FileStream(imgFileName, FileMode.Create);
                byte[] buffer = new byte[8];
                binFile.Read(buffer, 0, 8);
                if (BitConverter.ToUInt64(buffer, 0) != 0x7260EACCEACC9442)
                {
                    PrintError("File signature mismatch!");
                    return -1;
                }
                binFile.Read(buffer, 0, 4);
                partCount = BitConverter.ToUInt32(buffer, 0);
                Console.WriteLine("Parts count: {0}", partCount);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("     №   From     To       Count");
                Console.ResetColor();
                for (int i = 0; i < partCount; i++)
                {
                    binFile.Position = headerPosition;
                    binFile.Read(buffer, 0, 8);
                    imgFilePos = BitConverter.ToUInt32(buffer, 0);
                    partSize = BitConverter.ToUInt32(buffer, 4);
                    Console.WriteLine("Part {0:d4} {1:X8} {2:X8} {3:X8}", i, binFilePos, imgFilePos, partSize);
                    zeroSize = (imgFilePos * sectorSize) - imgFile.Position;
                    if (zeroSize != 0)
                    {
                        FileIO.WriteZeroes(imgFile, imgFile.Position, zeroSize);
                    }
                    FileIO.StreamCopy(binFile, imgFile, binFilePos * sectorSize, partSize * sectorSize, 0x100000);
                    binFilePos += partSize;
                    headerPosition += 8;
                }
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
                FileStream binFile = new FileStream(binFileName, FileMode.Open);
                FileStream imgFile = new FileStream(imgFileName, FileMode.Create);
                byte[] buffer = new byte[8];
                binFile.Read(buffer, 0, 8);
                if (BitConverter.ToUInt64(buffer, 0) != 0x7260EACCEACC9442)
                {
                    PrintError("File signature mismatch!");
                    return -1;
                }
                binFile.Read(buffer, 0, 4);
                partCount = BitConverter.ToUInt32(buffer, 0);
                Console.WriteLine("Parts count: {0}", partCount);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("     №   From     To       Count");
                Console.ResetColor();
                for (int i = 0; i < partCount; i++)
                {
                    binFile.Position = headerPosition;
                    binFile.Read(buffer, 0, 8);
                    imgFilePos = BitConverter.ToUInt32(buffer, 0);
                    partSize = BitConverter.ToUInt32(buffer, 4);
                    Console.WriteLine("Part {0:d3} {1:X8} {2:X8} {3:X8}", i, binFilePos, imgFilePos, partSize);
                    zeroSize = (imgFilePos * sectorSize) - imgFile.Position;
                    binFilePos += partSize;
                    headerPosition += 8;
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
