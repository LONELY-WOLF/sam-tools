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
        static void Main(string[] args)
        {
            string binFileName = null, imgFileName = null;
            uint partCount = 0;
            long headerPosition = 0x10, binFilePos = 8, imgFilePos, partSize, zeroSize;
            long sectorSize = 512;

            if (args.Length >= 1)
            {
                if (args.Length >= 2)
                {
                    binFileName = args[1];
                    if (args.Length >= 3)
                    {
                        imgFileName = args[3];
                    }
                    else
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
                }
                switch (args[0])
                {
                    case "/u":
                        {
                            if (string.IsNullOrWhiteSpace(binFileName))
                            {
                                PrintError("Invalid input file name!");
                                return;
                            }
                            FileStream binFile = new FileStream(binFileName, FileMode.Open);
                            FileStream imgFile = new FileStream(imgFileName, FileMode.Create);
                            byte[] buffer = new byte[8];
                            binFile.Read(buffer, 0, 8);
                            if (BitConverter.ToUInt64(buffer, 0) != 0x7260EACCEACC9442)
                            {
                                PrintError("File signature mismatch!");
                                return;
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
                            break;
                        }
                    case "/p":
                        {
                            PrintError("Not implemented yet");
                            break;
                        }
                    case "/h":
                        {
                            if (string.IsNullOrWhiteSpace(binFileName))
                            {
                                PrintError("Invalid input file name!");
                                return;
                            }
                            FileStream binFile = new FileStream(binFileName, FileMode.Open);
                            FileStream imgFile = new FileStream(imgFileName, FileMode.Create);
                            byte[] buffer = new byte[8];
                            binFile.Read(buffer, 0, 8);
                            if (BitConverter.ToUInt64(buffer, 0) != 0x7260EACCEACC9442)
                            {
                                PrintError("File signature mismatch!");
                                return;
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
                            break;
                        }
                    default:
                        {
                            //usage
                            Console.WriteLine(@"
USAGE:

image-rebase /u <input file> [<output file>]
    Extract image from <input file> to <output file>

image-rebase /p <input file> [<output file>]
    Pack image <input file> to <output file>

image-rebase /h <file>
    Parse header in <file> and display info

(c) -WOLF- 2013
");
                            break;
                        }
                }
            }
            else
            {
                //usage
                Console.WriteLine(@"
USAGE:

image-rebase /u <input file> [<output file>]
    Extract image from <input file> to <output file>

image-rebase /p <input file> [<output file>]
    Pack image <input file> to <output file>

image-rebase /h <file>
    Parse header in <file> and display info

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
