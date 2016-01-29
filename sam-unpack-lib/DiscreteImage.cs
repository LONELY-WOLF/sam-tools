using FileLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace sam_unpack_lib
{
    public class DiscreteImage
    {
        public class Section
        {
            public long Position, Length;
            public string File;

            public Section(long pos, long len)
            {
                Position = pos;
                Length = len;
                File = null;
            }

            public Section(long pos, long len, string file)
            {
                Position = pos;
                Length = len;
                File = file;
            }
        }

        public List<DiscreteImage.Section> Sections;
        string binFileName;

        public DiscreteImage(string fileName)
        {
            binFileName = fileName;
            uint partCount = 0;
            long headerPosition = 0x10, imgFilePos, partSize;
            Sections = new List<Section>();
            FileStream binFile = new FileStream(fileName, FileMode.Open);
            byte[] buffer = new byte[8];
            binFile.Read(buffer, 0, 8);
            if (BitConverter.ToUInt64(buffer, 0) != 0x7260EACCEACC9442)
            {
                throw new Exception("File signature mismatch!");
            }
            binFile.Read(buffer, 0, 4);
            partCount = BitConverter.ToUInt32(buffer, 0);
            for (int i = 0; i < partCount; i++)
            {
                binFile.Position = headerPosition;
                binFile.Read(buffer, 0, 8);
                imgFilePos = BitConverter.ToUInt32(buffer, 0);
                partSize = BitConverter.ToUInt32(buffer, 4);
                headerPosition += 8;
                Sections.Add(new Section(imgFilePos, partSize));
            }
            binFile.Close();
        }

        public void Extract(string imgFileName)
        {
            long binFilePos = 8, imgFilePos, partSize, zeroSize;
            long sectorSize = 512;

            FileStream binFile = new FileStream(binFileName, FileMode.Open);
            FileStream imgFile = new FileStream(imgFileName, FileMode.Create);
            byte[] buffer = new byte[8];
            binFile.Read(buffer, 0, 8);
            if (BitConverter.ToUInt64(buffer, 0) != 0x7260EACCEACC9442)
            {
                throw new Exception("File signature mismatch!");
            }
            foreach (DiscreteImage.Section section in Sections)
            {
                imgFilePos = section.Position;
                partSize = section.Length;
                zeroSize = (imgFilePos * sectorSize) - imgFile.Position;
                if (zeroSize != 0)
                {
                    FileIO.WriteZeroes(imgFile, imgFile.Position, zeroSize);
                }
                FileIO.StreamCopy(binFile, imgFile, binFilePos * sectorSize, partSize * sectorSize, 0x100000);
                binFilePos += partSize;
            }
            binFile.Close();
        }

        public static List<Section> Slice(string input, string output, int minZeroSectors = 2000)
        {
            List<Section> sections = new List<Section>();
            byte[] sector = new byte[512];
            FileStream inFile = new FileStream(input, FileMode.Open, FileAccess.Read);
            long start = 0, zStart = 0;
            bool eof = false;
            int fileNum = 0;
            int zCount = 0;
            FileStream outFile = new FileStream(output + String.Format(".{0:d3}", fileNum), FileMode.Create);
            while (!eof)
            {
                int bytesRead = inFile.Read(sector, 0, 512);
                if (bytesRead < 512)
                {
                    eof = true;
                }
                bool isZeroSector = sector.All(x => x == 0);
                if (isZeroSector)
                {
                    if (zCount > 0)
                    {
                        zCount++;
                    }
                    else
                    {
                        zCount = 1;
                        zStart = inFile.Position - 512L;
                    }
                }
                else
                {
                    if (zCount > 0)
                    {
                        if (zCount < minZeroSectors)
                        {
                            FileLib.FileIO.WriteZeroes(outFile, outFile.Position, zCount * 512L);
                            outFile.Write(sector, 0, 512);
                        }
                        else
                        {
                            outFile.Close();
                            sections.Add(new Section(start, zStart - start, output + String.Format(".{0:d3}", fileNum)));
                            //Console.WriteLine("{0} {1}", start, zStart - start);
                            start = inFile.Position - 512L;
                            fileNum++;
                            outFile = new FileStream(output + String.Format(".{0:d3}", fileNum), FileMode.Create);
                            outFile.Write(sector, 0, 512);
                        }
                        zCount = 0;
                    }
                    else
                    {
                        outFile.Write(sector, 0, 512);
                    }
                }
            }
            outFile.Close();
            sections.Add(new Section(start, zStart - start, output + String.Format(".{0:d3}", fileNum)));
            return sections;
        }

        public static void Pack(string outputFile, string templateFile)
        {
            FileStream output = new FileStream(outputFile, FileMode.Create);
            FileStream input;
            XDocument template = XDocument.Load(templateFile);
            if (template.Root.Name != "image-rebase")
            {
                throw new Exception("Invalid template file");
            }
            XElement slist = template.Root.Element(XName.Get("template"));
            if (slist == null)
            {
                throw new Exception("Invalid template file");
            }
            BinaryWriter bw = new BinaryWriter(output);
            //Write header
            bw.Write(0x7260EACCEACC9442);
            bw.Write(slist.Elements(XName.Get("section")).Count());
            bw.Write(0);
            foreach (XElement section in slist.Elements(XName.Get("section")))
            {
                long start = long.Parse(section.Attribute(XName.Get("start")).Value);
                long length = long.Parse(section.Attribute(XName.Get("length")).Value);
                if ((start % 512 != 0) || (length % 512 != 0))
                {
                    throw new Exception("Incomplete sector");
                }
                bw.Write((int)(start / 512L));
                bw.Write((int)(length / 512L));
            }
            FileIO.WriteZeroes(output, output.Position, 0x1000 - output.Position, 0x1000);
            //Write payload
            foreach (XElement section in slist.Elements(XName.Get("section")))
            {
                input = new FileStream(section.Attribute(XName.Get("file")).Value, FileMode.Open);
                long length = long.Parse(section.Attribute(XName.Get("length")).Value);
                FileIO.StreamCopy(input, output, 0, length, 1024 * 1024);
                input.Close();
            }
        }

        public static Section SectionFromFile(string filename)
        {
            FileInfo fi = new FileInfo(filename);
            return new Section(0, fi.Length, filename);
        }
    }
}
