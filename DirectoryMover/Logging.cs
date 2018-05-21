using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggingNS
{
    public class Logging
    {
        static public bool ExportTime = true;
        static public void info(params string[] str)
        {
            if (ExportTime == true)
                OutputTime();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Info:");
            Console.ForegroundColor = ConsoleColor.Gray;
            foreach (var s in str)
            {
                Console.Write(s);
            }
            Console.WriteLine();
        }

        static public void warning(params string[] str)
        {
            if (ExportTime == true)
                OutputTime();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Warning:");
            Console.ForegroundColor = ConsoleColor.Gray;
            foreach (var s in str)
            {
                Console.Write(s);
            }
            Console.WriteLine();
        }

        static public void error(params string[] str)
        {
            if (ExportTime == true)
                OutputTime();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Error:");
            Console.ForegroundColor = ConsoleColor.Gray;
            foreach (var s in str)
            {
                Console.Write(s);
            }
            Console.WriteLine();
            Console.ReadKey();
            Environment.Exit(-1);
        }

        static private void OutputTime()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(DateTime.Now.ToString("HH:mm:ss:"));
        }
    }
}
