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
        [ConfigurationProperty("serviceInfo", IsRequired = true)]
        public ServiceInfoElement ServiceInfo
        {
            get
            {
                return (ServiceInfoElement)base["serviceInfo"];
            }
            set
            {
                base["serviceInfo"] = value;
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
