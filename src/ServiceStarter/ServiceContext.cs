using ServiceStarter.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ServiceStarter
{
    internal class ServiceContext
    {
        private static object _Locker = new object();

        private static ServiceContext _Instance = null;

        private ServiceStarterSection _Configs = null;

        public static ServiceContext Current
        {
            get
            {
                if (null == _Instance)
                {
                    lock (_Locker)
                    {
                        if (null == _Instance)
                        {
                            _Instance = new ServiceContext();
                        }
                    }
                }

                return _Instance;
            }
        }
        public string Location { get; private set; }

        private Dictionary<string, AppDomain> _AppDomains;

        private ServiceContext()
        {
            _AppDomains = new Dictionary<string, AppDomain>();

            _Configs = (ServiceStarterSection)ConfigurationManager.GetSection("serviceStarters");

            Location = _Configs.ServiceInfo.ServicePaths["root"].Value;
        }

        public ServiceInfoElement ServiceInfo
        {
            get
            {
                return _Configs.ServiceInfo;
            }
        }

        public ServiceStarterElementCollection ServiceStarters
        {
            get
            {
                return _Configs.Services;
            }
        }

        public Dictionary<string, AppDomain> Domains
        {
            get
            {
                return _AppDomains;
            }
        }
    }
}
