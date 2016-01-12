using CStarter.Configuration;
using CStarterD.Common;
using CStarter.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace CStarterD
{
    public class WindowsService : ServiceBase
    {
        private ServiceStarterSection _Config;

        private CStarterMonitor _StarterMonitor;

        public void Initialize(ServiceStarterSection srvConfig)
        {
            _Config = srvConfig;
        }

        protected override void OnStart(string[] args)
        {
            "启动监听服务".Info();

            CStarterDControlServiceDaemon.Current.Start(_Config);
            CStarterDNotifierServiceDaemon.Current.Start(_Config);

            "监听服务已经启动".Info();

            BasicServiceStarter.Run(_Config);

            _StarterMonitor = new CStarterMonitor();

            _StarterMonitor.StartMonitor();
        }

        protected override void OnStop()
        {
            _StarterMonitor.StopMonitor();

            if (0 != ServiceContext.Current.ServiceSlots.Count)
            {
                ServiceSlot[] slots = new ServiceSlot[ServiceContext.Current.ServiceSlots.Count];

                ServiceContext.Current.ServiceSlots.CopyTo(slots);

                foreach (ServiceSlot slot in slots)
                {
                    (new CStarterClient()).Stop(ServiceContext.Current.Configuration.ServiceInfo.Name, slot.Name, slot.Signal);

                    if (!ServiceContext.Current.WaitServiceStopping(10))
                    {
                        if (!slot.WorkProcess.WaitForExit(10 * 1000))
                        {
                            slot.WorkProcess.Kill();
                        }
                    }

                    ServiceSlot tSlot = ServiceContext.Current.ServiceSlots.FirstOrDefault(s => s.Name == slot.Name);

                    if (null != tSlot)
                    {
                        ServiceContext.Current.ServiceSlots.Remove(tSlot);
                    }
                }
            }

            CStarterDControlServiceDaemon.Current.Stop();
            CStarterDNotifierServiceDaemon.Current.Stop();
        }
    }
}
