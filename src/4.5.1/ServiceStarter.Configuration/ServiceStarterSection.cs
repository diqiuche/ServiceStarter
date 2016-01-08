using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CStarter.Configuration
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
        [ConfigurationCollection(typeof(ServiceStarterElement), AddItemName = "service", ClearItemsName = "clear", RemoveItemName = "remove")]
        public ServiceStarterElementCollection Services
        {
            get
            {
                return (ServiceStarterElementCollection)base["services"];
            }
        }
    }
}
