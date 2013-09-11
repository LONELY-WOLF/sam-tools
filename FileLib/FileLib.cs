using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FileLib
{
    public static class FileIO
    {
        public static long FileSearch(FileStream source, byte[] data, long startPos)
        {
            int c, i = 0;
            source.Position = startPos;
            while ((c = source.ReadByte()) != -1)
            {
                if (data[i] == (byte)c)
                {
                    i++;
                    if (i == data.Length)
                    {
                        return source.Position - i;
                    }
                }
            }
            return -1;
        }

        public static void StreamCopy(Stream source, Stream dest, long start, long count, int bufferSize = 4096)
        {
            long counter = 0;
            int bytesRead, bytesToRead;
            byte[] buffer = new byte[bufferSize];
            source.Seek(start, SeekOrigin.Begin);
            while (counter < count)
            {
                bytesToRead = (int)((count - counter > bufferSize) ? bufferSize : count - counter);
                bytesRead = source.Read(buffer, 0, bytesToRead);
                dest.Write(buffer, 0, bytesRead);
                counter += bytesRead;
            }
        }

        public static void WriteZeroes(Stream dest, long start, long count, int bufferSize = 0x100000)
        {
            long counter = 0;
            int bytesToWrite;
            byte[] buffer = new byte[bufferSize];
            for (int i = 0; i < bufferSize; i++)
            {
                buffer[i] = 0;
            }
            while (counter < count)
            {
                bytesToWrite = (int)((count - counter > bufferSize) ? bufferSize : count - counter);
                dest.Write(buffer, 0, bytesToWrite);
                counter += bytesToWrite;
            }
        }
    }
}
