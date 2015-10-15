using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace ServiceStarter.Configuration
{
    public class DependenceServiceCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DependenceServiceConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as DependenceServiceConfigurationElement).ServiceName;
        }
    }
}
