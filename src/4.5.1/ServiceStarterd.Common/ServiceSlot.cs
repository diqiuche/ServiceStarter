using CStarter.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CStarterD.Common
{
    public class ServiceSlot
    {
        public string Name { get; set; }

        public long ProcessID { get; set; }

        public Process WorkProcess { get; set; }

        public ServiceStarterElement Config { get; set; }

        public string Signal { get; set; }
    }
}
