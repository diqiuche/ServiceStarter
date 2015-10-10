using log4net;
using ServiceStarter.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStarter
{
    public static class BasicServiceStarter
    {
        public static void Run(string[] args)
        {
            ServiceInfoElement config = ServiceContext.Current.ServiceInfo;

            var cmd = 0 != args.Length ? args[0] : "";

            switch (cmd)
            {
                case "i":
                case "install":
                    string.Format("安装服务：{0}:{1}", ServiceContext.Current.ServiceInfo.DisplayName,
                        ServiceContext.Current.ServiceInfo.ServiceName).Info();
                    BasicServiceInstaller.Install(ServiceContext.Current.ServiceInfo.DisplayName,
                        ServiceContext.Current.ServiceInfo.ServiceName);
                    break;
                case "u":
                case "uninstall":
                    string.Format("卸载服务：{0}:{1}", ServiceContext.Current.ServiceInfo.DisplayName,
                        ServiceContext.Current.ServiceInfo.ServiceName).Info();
                    BasicServiceInstaller.Uninstall(ServiceContext.Current.ServiceInfo.DisplayName,
                        ServiceContext.Current.ServiceInfo.ServiceName);
                    break;
                default:
                    if (null != ServiceContext.Current.ServiceInfo)
                    {
                        string.Format("服务正在使用 {0} 模式运行。", Environment.UserInteractive ? "交互" : "服务").Info();

                        string.Format("发现服务配置。共配置了：{0} 项服务", ServiceContext.Current.ServiceStarters.Count).Info();
                        try
                        {
                            foreach (ServiceStarterElement eleConfig in ServiceContext.Current.ServiceStarters)
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
                                    oSetup.ConfigurationFile = Path.Combine(oSetup.ApplicationBase, "service.config");
                                    oSetup.PrivateBinPath = oSetup.ApplicationBase;

                                    string.Format("基础路径：{0}", oSetup.ApplicationBase).Info();
                                    string.Format("BIN路径：{0}", oSetup.PrivateBinPath).Info();
                                    string.Format("镜像路径：{0}", oSetup.ShadowCopyDirectories).Info();
                                    string.Format("缓存路径：{0}", oSetup.CachePath).Info();
                                    string.Format("配置文件：{0}", oSetup.ConfigurationFile).Info();

                                    EvidenceBase[] hostEvidence = { new Zone(SecurityZone.MyComputer) };
                                    Evidence e = new Evidence(hostEvidence, null);

                                    AppDomain newDomain = AppDomain.CreateDomain(eleConfig.Name,
                                        e, //AppDomain.CurrentDomain.Evidence,
                                        oSetup);

                                    IService service = newDomain.CreateInstanceAndUnwrap(eleConfig.AssemblyName, eleConfig.TypeName) as IService;

                                    service.Start();

                                    ServiceContext.Current.Domains.Add(eleConfig.Name, newDomain);
                                }
                                else
                                {
                                    string.Format("位于 {0} 位置的 {1} 服务未配置成可启动", eleConfig.ContentPath, eleConfig.Name).Warn();
                                }
                            }
                        }
                        catch (Exception eX)
                        {
                            eX.Exception();
                        }
                    }
                    else
                    {
                        "找不到服务配置信息，启动失败。".Error();

                        if (Environment.UserInteractive)
                        {
                            "按任意键关闭程序".Info();
                            Console.ReadLine();
                        }
                    }
                    break;
            }
        }
    }
}
