using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceStarter
{
    public static class FormatExtension
    {
        public static string Formate(this string template, params string[] args)
        {
            return string.Format(template, args);
        }
    }
}
