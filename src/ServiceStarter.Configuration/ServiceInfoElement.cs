using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace ServiceStarter.Configuration
{
    public class ServiceInfoElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("displayName", IsRequired = false)]
        public string DisplayName
        {
            get { return (string)base["displayName"]; }
            set { base["displayName"] = value; }
        }

        [ConfigurationProperty("serviceName", IsRequired = true)]
        public string ServiceName
        {
            get { return (string)base["serviceName"]; }
            set { base["serviceName"] = value; }
        }

        [ConfigurationProperty("servicePaths", IsRequired = true)]
        public NameValueConfigurationCollection ServicePaths
        {
            get
            {
                return (NameValueConfigurationCollection)base["servicePaths"];
            }
            set
            {
                base["servicePaths"] = value;
            }
        }

        [ConfigurationProperty("description", IsRequired = false, DefaultValue = "")]
        public string Description
        {
            get
            {
                return (string)base["description"];
            }
            set
            {
                base["description"] = value;
            }
        }
    }
}
