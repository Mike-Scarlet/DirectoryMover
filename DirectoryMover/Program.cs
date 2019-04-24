using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LoggingNS;
using static DirectoryMover.SystemFunction;
using HierarchyArchitecture;

namespace DirectoryMover
{
    class Program
    {
        static void Main(string[] args)
        {
            //DirectoryHierarchy h = new DirectoryHierarchy(@"D:\[temp\dir");
            //h.Build();
            //DirectoryHierachyManager m = new DirectoryHierachyManager(@"F:\[temp");
            //SystemFunction.ChangeFolderIcon(@"D:\[temp\dir\holy");
            //var a = File.GetAttributes(@"D:\[temp\dir\RecycleBin");
            //Directory.Delete(@"D:\[temp\dir\RecycleBin");
            //var a = File.GetAttributes(@"D:\[temp\dir\新建文件夹\新建文件夹 (2) - 副本");
            //var b = Directory.GetFiles(@"D:\[temp\dir\新建文件夹\新建文件夹 (2) - 副本");
            //var c = File.GetAttributes(@"D:\[temp\dir\新建文件夹\新建文件夹 (2) - 副本\desktop.ini");
            //m.build();
            //m.analyze();
            //m.display_actions();
            //m.writeXml();

            //DirectoryHierachyManager m = new DirectoryHierachyManager(@"G:\[tmp\1807");
            //m.run();

            //DirectoryHierarchy hir = new DirectoryHierarchy(@"H:\tst1");
            //hir.Build();
            //HierarchyAnalyzer ha = new HierarchyAnalyzer(hir.root, (1 << 4) - 1);
            //ha.run();

            //Console.WriteLine(Utils.RandomString.rndNameAppendix(10));
            //Directory.Move(@"H:\tstFile.txt", @"H:\re1.txt");

            ////////////////// Still have a bug
            /// when recovering
            /// a directory raised IO exception 
            /// it's parent will still be recovered 

            string analyzeDir = @"H:\re";
            string delDir = analyzeDir + "\\RecycleBin";

            if (File.Exists(analyzeDir + "\\RecycleBin\\backup.db"))
            {
                Logging.info("Find recovery file in \"" + delDir + "\"");
                Logging.info("do you want to run recover procedure?(y/n)");
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    Console.Write('\b');
                    Logging.info("Starting recovery");
                    ActionsManager am = new ActionsManager(analyzeDir);
                    am.Load();
                    am.Recover();
                    Logging.info("Finish recovery");
                    goto FLAGEND;
                }
                //else
                //{
                //    Console.Write('\b');
                //    Logging.info("Shutting Down");
                //    System.Threading.Thread.Sleep(1000);
                //}
            }

            Logging.info("building structure for \"" + analyzeDir + "\"");
            DirectoryHierarchy hir = new DirectoryHierarchy(analyzeDir);
            hir.Build();
            HierarchyAnalyzer ha = new HierarchyAnalyzer(hir.root, (1 << 4) - 1);
            ha.Run();
            Logging.info("do you want to run packing procedure?(y/n)");
            Console.Write('\b');
            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                Console.Write('\b');
                ActionsManager am = new ActionsManager(analyzeDir, ha.actions.ToList());
                am.Act();
            }
            else
            {
                Console.Write('\b');
                Logging.info("Shutting Down");
                System.Threading.Thread.Sleep(1000);
            }
            
            FLAGEND:
            Console.ReadKey();
        }
    }
}
