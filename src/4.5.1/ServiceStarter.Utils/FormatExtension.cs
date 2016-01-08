using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CStarter.Utils
{
    public static class FormatExtension
    {
        public static string Formate(this string template, params string[] args)
        {
            return string.Format(template, args);
        }
    }
}
