using log4net;
using ServiceStarter.Common;
using ServiceStarter.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStarter
{
    public static class BasicServiceStarter
    {
        public static bool RunService(string name, out string msg)
        {
            bool retValue = false;

            try
            {
                string.Format("查找服务配置：{0}", name).Info();

                ServiceStarterElement eleConfig = ServiceContext.Current.ServiceStarters.Cast<ServiceStarterElement>().FirstOrDefault(e => e.Name == name);

                if (null == eleConfig)
                {
                    msg = "找不到服务配置";
                }
                else
                {
                    retValue = RunService(eleConfig, out msg);
                }
            }
            catch(Exception eX)
            {
                msg = eX.Message;
            }

            return retValue;
        }

        public static bool RunService(ServiceStarterElement eleConfig, out string msg)
        {
            bool retValue = false;

            string.Format("服务 {0} 依赖服务 {1}", eleConfig.Name, string.Join(",", (from refSrv in eleConfig.DependenceServices.Cast<DependenceServiceConfigurationElement>()
                                                                               select refSrv.ServiceName).ToArray())).Info();

            msg = "";

            try
            {
                if ("true" == eleConfig.Enabled.ToLower())
                {
                    string.Format("启动位于 {0} 位置的 {1} 服务，并启动实例： {2}", eleConfig.ContentPath, eleConfig.Name, eleConfig.TypeName)
                        .Info();
                    AppDomainSetup oSetup = new AppDomainSetup();
                    oSetup.ApplicationName = eleConfig.Name;
                    oSetup.ApplicationBase = Path.IsPathRooted(eleConfig.ContentPath) ? eleConfig.ContentPath :
                        Path.Combine(ServiceContext.Current.Location,
                        ServiceContext.Current.ServiceInfo.ServicePaths["services"].Value,
                        eleConfig.ContentPath);
                    //oSetup.PrivateBinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "private");
                    oSetup.CachePath = Path.Combine(ServiceContext.Current.Location,
                        ServiceContext.Current.ServiceInfo.ServicePaths["cache"].Value);
                    oSetup.ShadowCopyFiles = "true"; //启用影像复制程序集
                    oSetup.ShadowCopyDirectories = oSetup.ApplicationBase;
                    oSetup.ConfigurationFile = Path.Combine(oSetup.ApplicationBase, eleConfig.AssemblyName + ".dll.config");
                    oSetup.PrivateBinPath = oSetup.ApplicationBase;

                    string.Format("基础路径：{0}", oSetup.ApplicationBase).Info();
                    string.Format("BIN路径：{0}", oSetup.PrivateBinPath).Info();
                    string.Format("镜像路径：{0}", oSetup.ShadowCopyDirectories).Info();
                    string.Format("缓存路径：{0}", oSetup.CachePath).Info();
                    string.Format("配置文件：{0}", oSetup.ConfigurationFile).Info();

                    AppDomain newDomain = AppDomain.CreateDomain(eleConfig.Name,
                        AppDomain.CurrentDomain.Evidence,
                        oSetup);

                    IService service = newDomain.CreateInstanceAndUnwrap(eleConfig.AssemblyName, eleConfig.TypeName) as IService;

                    Sponsor<IService> s = new Sponsor<IService>(service);

                    service.Start();

                    ServiceContext.Current.Domains.Add(eleConfig.Name, newDomain);
                    ServiceContext.Current.Services.Add(eleConfig.Name, s);

                    retValue = true;
                }
                else
                {
                    msg = string.Format("位于 {0} 位置的 {1} 服务未配置成可启动", eleConfig.ContentPath, eleConfig.Name);
                    msg.Warn();
                }
            }
            catch(Exception eX)
            {
                msg = eX.Message;
                msg.Error();
                eX.Exception();
            }

            return retValue;
        }

        public static bool RunServiceProcess(string name, out string msg)
        {
            bool retValue = false;

            try
            {
                string.Format("查找服务配置：{0}", name).Info();

                ServiceStarterElement eleConfig = ServiceContext.Current.ServiceStarters.Cast<ServiceStarterElement>().FirstOrDefault(e => e.Name == name);

                if (null == eleConfig)
                {
                    msg = "找不到服务配置";
                }
                else
                {
                    retValue = RunServiceProcess(eleConfig, out msg);
                }
            }
            catch (Exception eX)
            {
                msg = eX.Message;
            }

            return retValue;
        }

        public static bool RunServiceProcess(ServiceStarterElement eleConfig, out string msg)
        {
            string.Format("服务 {0} 依赖服务 {1}", eleConfig.Name, string.Join(",", (from refSrv in eleConfig.DependenceServices.Cast<DependenceServiceConfigurationElement>()
                                                                               select refSrv.ServiceName).ToArray())).Info();

            string.Format("启动位于 {0} 位置的 {1} 服务，并启动实例： {2}", eleConfig.ContentPath, eleConfig.Name, eleConfig.TypeName)
                        .Info();

            bool retValue = false;

            string signal = Guid.NewGuid().ToString();

            msg = "";

            try
            {
                Process p = new Process();
                p.StartInfo.Arguments = string.Format(" -s={0} -g={1}", eleConfig.Name, signal);
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "servicestarter.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.OutputDataReceived += new DataReceivedEventHandler(p_OutputDataReceived);
                p.ErrorDataReceived += new DataReceivedEventHandler(p_ErrorDataReceived);

                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();

                string.Format("等待服务 {0} 启动", eleConfig.Name).Info();
                EventWaitHandle successSignal = new EventWaitHandle(false, EventResetMode.ManualReset, "complete_" + signal);
                successSignal.WaitOne();

                ServiceContext.Current.AddSlot(eleConfig, p, signal);

                retValue = true;
            }
            catch(Exception eX)
            {
                msg = eX.Message;
                eX.Exception();
            }

            return retValue;
        }

        private static void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            e.Data.Info(false);
        }

        private static void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            e.Data.Error(false);
        }

        public static void Run(string[] args)
        {
            if (null != ServiceContext.Current.ServiceInfo)
            {
                string.Format("服务正在使用 {0} 模式运行。", Environment.UserInteractive ? "交互" : "服务").Info();

                string.Format("共配置了：{0} 项服务", ServiceContext.Current.ServiceStarters.Count).Info();

                "解析服务启动顺序".Info();

                List<ServiceStarterElement> orderedConfigs = SortServices();

                string.Format("服务按照如下顺序启动：{0}", string.Join("=>", (from n in orderedConfigs select n.Name).ToArray())).Info();

                try
                {
                    foreach (ServiceStarterElement eleConfig in orderedConfigs)
                    {
                        string msg = "";
                        if (!RunServiceProcess(eleConfig, out msg))
                        {
                            msg.Error();
                        }
                    }

                    "服务启动完毕".Info();
                }
                catch (Exception eX)
                {
                    eX.Exception();
                }
            }
            else
            {
                "找不到服务配置信息，启动失败。".Error();
            }
        }

        private static List<ServiceStarterElement> SortServices()
        {
            List<ServiceStarterElement> retValue = new List<ServiceStarterElement>();

            List<RefCount> countList = new List<RefCount>();

            foreach (ServiceStarterElement ele in ServiceContext.Current.ServiceStarters)
            {
                if ("true" == ele.Enabled.ToLower())
                {
                    RefCount found = countList.FirstOrDefault(c => c.Name == ele.Name);

                    if (null == found)
                    {
                        found = new RefCount()
                        {
                            Name = ele.Name,
                            Count = 0,
                            Element = ele
                        };

                        countList.Add(found);
                    }
                    else
                    {
                        if (null == found.Element)
                            found.Element = ele;
                    }

                    if (0 != ele.DependenceServices.Count)
                    {
                        foreach (DependenceServiceConfigurationElement dEle in ele.DependenceServices)
                        {
                            RefCount refFound = countList.FirstOrDefault(c => c.Name == dEle.ServiceName);

                            if (null != refFound)
                            {
                                refFound.Count++;
                            }
                            else
                            {
                                refFound = new RefCount()
                                {
                                    Name = dEle.ServiceName,
                                    Count = 1,
                                    Element = null
                                };

                                countList.Add(refFound);
                            }
                        }
                    }
                }
                else
                {
                    string.Format("位于 {0} 位置的 {1} 服务未配置成可启动", ele.ContentPath, ele.Name).Warn();
                }
            }

            countList.Sort(new Comparison<RefCount>((left, right) =>
            {
                return right.Count.CompareTo(left.Count);
            }));

            retValue.AddRange((from c in countList select c.Element).ToArray());

            return retValue;
        }
    }

    internal class RefCount
    {
        public string Name { get; set; }
        public int Count { get; set; }

        public ServiceStarterElement Element { get; set; }
    }
}
