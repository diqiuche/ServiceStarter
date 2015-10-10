using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceStarter.Common
{
    public static class Logger
    {
        static ILog _Log = LogManager.GetLogger("starterLogger");

        public static void Info(this string msg)
        {
            if (Environment.UserInteractive)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("{0}\t:{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg);
                Console.ResetColor();
            }
            else
            {
                _Log.Info(msg);
            }
        }

        public static void Debug(this string msg)
        {
            if (Environment.UserInteractive)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("{0}\t:{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg);
                Console.ResetColor();
            }
            else
            {
                _Log.Debug(msg);
            }
        }

        public static void Warn(this string msg)
        {
            if (Environment.UserInteractive)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("{0}\t:{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg);
                Console.ResetColor();
            }
            else
            {
                _Log.Warn(msg);
            }
        }

        public static void Error(this string msg)
        {
            if (Environment.UserInteractive)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0}\t:{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "程序发生错误：");
                Console.WriteLine("{0}\t:{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg);
                Console.ResetColor();
            }
            else
            {
                _Log.Error(msg);
            }
        }

        public static void Exception(this Exception eX)
        {
            if (Environment.UserInteractive)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0}\t:{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "程序发生错误：");
                Console.WriteLine("{0}\t:{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), eX.Message);
                Console.WriteLine("{0}\t:{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), eX.StackTrace);
                Exception innerEx = eX.InnerException;
                while (null != innerEx)
                {
                    Console.WriteLine("{0}\t:{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), eX.Message);
                    Console.WriteLine("{0}\t:{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), eX.StackTrace);

                    innerEx = innerEx.InnerException;
                }
                Console.ResetColor();
            }
            else
            {
                _Log.Error(eX.Message, eX);
            }
        }

        public static void Domain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            (e.ExceptionObject as Exception).Exception();
        }
    }
}
