﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStarter.Common
{
    public class ServiceStarterElement : ConfigurationElement
    {
        public ServiceStarterElement()
        {
        }

        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return (string)base["name"];
            }
            set
            {
                base["name"] = value;
            }
        }

        [ConfigurationProperty("assemblyName", IsRequired = true)]
        public string AssemblyName
        {
            get
            {
                return (string)base["assemblyName"];
            }
            set
            {
                base["assemblyName"] = value;
            }
        }

        [ConfigurationProperty("typeName", IsRequired = true)]
        public string TypeName
        {
            get { return (string)base["typeName"]; }
            set { base["typeName"] = value; }
        }

        [ConfigurationProperty("contentPath", IsRequired = true)]
        public string ContentPath
        {
            get
            {
                return (string)base["contentPath"];
            }
            set
            {
                base["contentPath"] = value;
            }
        }
    }
}