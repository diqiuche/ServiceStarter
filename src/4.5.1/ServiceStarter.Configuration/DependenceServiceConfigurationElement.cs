using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace CStarter.Configuration
{
    public class DependenceServiceConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("serviceName", IsRequired = true)]
        public string ServiceName
        {
            get
            {
                return (string)base["serviceName"];
            }
            set
            {
                base["serviceName"] = value;
            }
        }
    }
}
