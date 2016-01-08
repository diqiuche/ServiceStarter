using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CStarter.Configuration
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

        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = "true")]
        public string Enabled
        {
            get
            {
                return (string)base["enabled"];
            }
            set
            {
                base["enabled"] = value;
            }
        }

        [ConfigurationProperty("dependenceServices", IsRequired = false)]
        [ConfigurationCollection(typeof(DependenceServiceConfigurationElement), AddItemName = "refService", ClearItemsName = "clear", RemoveItemName = "remove")]
        public DependenceServiceCollection DependenceServices
        {
            get
            {
                return (DependenceServiceCollection)base["dependenceServices"];
            }
        }
    }
}
