using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CStarter.Utils
{
    public static class Logger
    {
        public static void Info(this string msg)
        {
            LogManager.GetLogger("starterLogger").Info(msg);
        }

        public static void Debug(this string msg)
        {
            LogManager.GetLogger("starterLogger").Debug(msg);
        }

        public static void Warn(this string msg)
        {
            LogManager.GetLogger("starterLogger").Warn(msg);
        }

        public static void Error(this string msg)
        {
            LogManager.GetLogger("starterLogger").Error(msg);
        }

        public static void Exception(this Exception eX)
        {
            LogManager.GetLogger("starterLogger").Error(eX.Message, eX);
        }
    }
}
