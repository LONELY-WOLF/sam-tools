﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sam_unpack_lib;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            MBN mbnFile = new MBN("1.mbn");
            foreach (MBN.Section item in mbnFile.Sections)
            {
                //item.Extract(item, "csc");
            }
        }
    }
}
