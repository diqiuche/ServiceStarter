﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CStarter.Configuration
{
    public class ServiceStarterElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ServiceStarterElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as ServiceStarterElement).Name;
        }
    }
}
