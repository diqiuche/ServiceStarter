using CStarterD.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using CStarter.Utils;
using System.Threading.Tasks;

namespace CStarterD
{
    public class CStarterMonitor
    {
        private Task _MonitorTask;

        private CancellationTokenSource _MonitorToken;

        public CStarterMonitor()
        {
            _MonitorToken = new CancellationTokenSource();
            _MonitorTask = new Task(DoMonitor, _MonitorToken.Token);
        }

        private void DoMonitor()
        {
            while (!_MonitorToken.IsCancellationRequested)
            {
                Thread.Sleep(10 * 1000);

                ServiceSlot[] slots = ServiceContext.Current.GetServiceSlots();

                if (0 != slots.Length)
                {
                    foreach (ServiceSlot s in slots)
                    {
                        if (s.WorkProcess.HasExited)
                        {
                            if (0 != s.WorkProcess.ExitCode)
                            {
                                "服务进程 {0} 已经退出".Formate(s.Name).Error();
                                ServiceContext.Current.RemoveSlot(s.WorkProcess.Id);

                                if("Y" == s.Config.RestartOnError.ToUpper())
                                {
                                    "服务进程 {0} 配置为在错误退出后重新启动".Formate(s.Name).Info();

                                    string msg = "";

                                    if(BasicServiceStarter.RunServiceProcess(ServiceContext.Current.Configuration.ServiceInfo.Name,
                                        s.Config, out msg))
                                    {
                                        "服务进程 {0} 完成重启".Formate(s.Name).Info();
                                    }
                                    else
                                    {
                                        msg.Error();
                                        "服务进程 {0} 重启失败".Formate(s.Name).Error();
                                    }
                                }
                            }
                            else
                            {
                                "服务进程 {0} 已经退出".Formate(s.Name).Info();
                            }
                        }
                    }
                }
            }
        }

        public void StartMonitor()
        {
            _MonitorTask.Start();
        }

        public void StopMonitor()
        {
            _MonitorToken.Cancel();
        }
    }
}
