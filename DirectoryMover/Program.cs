using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LoggingNS;
using DirectoryHierachy;
using static DirectoryMover.SystemFunction;

namespace DirectoryMover
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryHierachyManager m = new DirectoryHierachyManager(@"D:\[temp\dir");
            m.loadXml();
            //m.build();
            //m.analyze();
            //m.display_actions();

            //m.writeXml();
            Console.ReadKey();
        }
    }
}
