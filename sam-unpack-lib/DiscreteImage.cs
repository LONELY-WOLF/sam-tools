using FileLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace sam_unpack_lib
{
    public class DiscreteImage
    {
        public class Section
        {
            public long Position, Length;

            public Section(long pos, long len)
            {
                Position = pos;
                Length = len;
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
    }
}
