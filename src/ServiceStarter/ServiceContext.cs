using ServiceStarter.Common;
using ServiceStarter.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

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

        private Dictionary<string, Sponsor<IService>> _Services;

        private List<ServiceSlot> _Slots;

        private ServiceContext()
        {
            _AppDomains = new Dictionary<string, AppDomain>();
            _Services = new Dictionary<string, Sponsor<IService>>();
            _Slots = new List<ServiceSlot>();

            _Configs = (ServiceStarterSection)ConfigurationManager.GetSection("serviceStarters");

            Location = _Configs.ServiceInfo.ServicePaths["root"].Value;
        }

        public void AddSlot(ServiceStarterElement eleConfig, Process p, string signal)
        {
            ServiceSlot slot = new ServiceSlot()
            {
                Name = eleConfig.Name,
                Config = eleConfig,
                WorkProcess = p,
                Signal = signal
            };
            
            _Slots.Add(slot);
        }

        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            e.Data.Info();
        }

        void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            e.Data.Error();
        }

        void p_Exited(object sender, EventArgs e)
        {
            RemoveSlot((sender as Process).Id);
        }

        public void RemoveSlot(int pid)
        {
            if(0 != _Slots.Count)
            {
                ServiceSlot target = null;

                foreach(ServiceSlot slot in _Slots)
                {
                    if(slot.WorkProcess.Id == pid)
                    {
                        target = slot;
                        break;
                    }
                }

                if (null != target)
                {
                    string.Format("{0} 已经退出", target.Name);
                    _Slots.Remove(target);
                }
            }
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

        public Dictionary<string, Sponsor<IService>> Services
        {
            get
            {
                return _Services;
            }
        }

        public List<ServiceSlot> ServiceSlots
        {
            get
            {
                return _Slots;
            }
        }

        public string Name
        {
            get
            {
                return _Configs.ServiceInfo.Name;
            }
        }

        public string PipeUri
        {
            get
            {
                return "net.pipe://localhost/" + _Configs.ServiceInfo.Name;
            }
        }

        public string PipeEndPointName
        {
            get
            {
                return "control";
            }
        }

        public string RunMode
        {
            get;
            set;
        }
    }
}
