using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStarter.Common
{
    public class ServiceStarterSection : ConfigurationSection
    {
        [ConfigurationProperty("default", IsRequired = true)]
        public string Default
        {
            get
            {
                return (string)base["default"];
            }
            set
            {
                base["default"] = value;
            }
        }

        [ConfigurationProperty("services", IsRequired = true)]
        [ConfigurationCollection(typeof(ServiceStarterElementCollection))]
        public ServiceStarterElementCollection Services
        {
            get
            {
                return (ServiceStarterElementCollection)base["services"];
            }
        }
    }
}
