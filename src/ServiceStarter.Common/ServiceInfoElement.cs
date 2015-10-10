using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace ServiceStarter.Common
{
    public class ServiceInfoElement : ConfigurationElement
    {
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

        [ConfigurationProperty("appPath", IsRequired = false, DefaultValue = ".")]
        public string AppPath
        {
            get
            {
                return (string)base["appPath"];
            }
            set
            {
                base["appPath"] = value;
            }
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
    }
}
