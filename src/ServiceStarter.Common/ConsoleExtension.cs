using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStarter.Common
{
    public static class ConsoleExtension
    {
        public static void WriteException(this Exception eX)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("程序发生错误：");
            Console.WriteLine(eX.Message);
            Console.ResetColor();
        }

        public static void WriteInfo(this string info)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(info);
            Console.ResetColor();
        }

        public static void WriteActionInfo(this string info)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(info);
            Console.ResetColor();
        }

        public static void WriteWarningInfo(this string info)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(info);
            Console.ResetColor();
        }
    }
}
