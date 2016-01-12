using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CStarter.Utils;
using CStarter.OptionsSharp;
using System.Configuration;
using System.IO;
using CStarter.Configuration;
using CStarterD.Common;
using System.Threading;
using System.ServiceProcess;
using log4net.Repository.Hierarchy;
using log4net.Appender;
using syslog4net.Filter;
using syslog4net.Layout;
using System.Reflection;

namespace CStarterD
{
    class Program
    {
        static void Version()
        {
            "服务守护程序，版本号：1.0".Info(false);
        }

        static void ShowHelp(OptionSet p)
        {
            "用法：CStarterD [Options]+".Info(false);
            "对服务做出相应的操作指令".Info(false);

            p.WriteOptionDescriptions(Console.Out);
        }

        static void PrepareLogger(string domain)
        {
            Hierarchy hier = log4net.LogManager.GetRepository() as Hierarchy;

            string logRootFullName = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(new char[] { '\\' })).FullName,
                "logs", "CStarterd");

            if(null != hier)
            {
                RollingFileAppender commonAppender = (RollingFileAppender)hier.GetAppenders().Where(a => a.Name.Equals("CommonAppender")).FirstOrDefault();

                if (null != commonAppender)
                {
                    commonAppender.File = Path.Combine(logRootFullName, "Common", "logs.txt");

                    var filter = new LogExceptionToFileFilter()
                    {
                        ExceptionLogFolder = Path.Combine(logRootFullName, "Common", "Exceptions")
                    };
                    commonAppender.AddFilter(filter);
                    filter.ActivateOptions();

                    var layout = new SyslogLayout()
                    {
                        StructuredDataPrefix = "CStarterD@" + domain
                    };
                    commonAppender.Layout = layout;
                    layout.ActivateOptions();

                    commonAppender.ActivateOptions();
                }

                RollingFileAppender starterAppender = (RollingFileAppender)hier.GetAppenders().Where(a => a.Name.Equals("StarterAppender")).FirstOrDefault();

                if(null != starterAppender)
                {
                    starterAppender.File = Path.Combine(logRootFullName, "Starterd", "logs.txt");

                    var filter = new LogExceptionToFileFilter()
                    {
                        ExceptionLogFolder = Path.Combine(logRootFullName, "Starterd", "Exceptions")
                    };
                    starterAppender.AddFilter(filter);
                    filter.ActivateOptions();

                    var layout = new SyslogLayout()
                    {
                        StructuredDataPrefix = "CStarterD@" + domain
                    };
                    starterAppender.Layout = layout;
                    layout.ActivateOptions();

                    starterAppender.ActivateOptions();
                }
            }
        }

        static bool StartDaemons(ServiceStarterSection srvConfig)
        {
            bool retValue = false;

            try
            {
                "启动监听服务".Info();

                CStarterDControlServiceDaemon.Current.Start(srvConfig);
                CStarterDNotifierServiceDaemon.Current.Start(srvConfig);

                "监听服务已经启动".Info();

                retValue = true;
            }
            catch(Exception eX)
            {
                eX.Exception();
            }

            return retValue;
        }

        static bool StartServiceProccess(ServiceStarterSection srvConfig)
        {
            bool retValue = false;
            try
            {
                "启动注册的服务".Info();
                BasicServiceStarter.Run(srvConfig);
                "服务都已经启动".Info();

                retValue = true;
            }
            catch (Exception eX)
            {
                retValue = false;
            }

            return retValue;
        }

        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        [STAThread]
        static void Main(string[] args)
        {
            ServiceStarterParams param = new ServiceStarterParams();

            List<string> extra = param.Parse(args);

            extra = param.Parse(args);

            Configuration config;

            if (!string.IsNullOrEmpty(param.ConfigurationFileName))
            {
                string fileFullName = param.ConfigurationFileName;

                if (!Path.IsPathRooted(fileFullName))
                {
                    fileFullName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, param.ConfigurationFileName);
                }

                ExeConfigurationFileMap map = new ExeConfigurationFileMap();
                map.ExeConfigFilename = fileFullName;
                config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

                log4net.Config.XmlConfigurator.Configure(new FileInfo(fileFullName));
            }
            else
            {
                config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                log4net.Config.XmlConfigurator.Configure();
            }

            var srvConfig = (ServiceStarterSection)config.GetSection("serviceStarters");
            ServiceContext.Current.Configuration = srvConfig;

            PrepareLogger(srvConfig.ServiceInfo.Name);

            if (Environment.UserInteractive)
            {
                try
                {
                    if (param.IsShowHelp)
                    {
                        ShowHelp(param.Options);
                        (string.Join(",", extra.ToArray())).Info();
                        return;
                    }
                    else if (param.IsShowVersion)
                    {
                        Version();
                    }
                    else
                    {
                        CStarterMonitor monitor = new CStarterMonitor();

                        if(StartDaemons(srvConfig))
                        {
                            if(StartServiceProccess(srvConfig))
                            {
                                monitor.StartMonitor();

                                "按任意键关闭程序".Info();
                                Console.ReadLine();

                                monitor.StopMonitor();

                                "程序正在退出，请不要关闭窗口".Info();
                                string.Format("需要停止 {0} 个服务", ServiceContext.Current.ServiceSlots.Count).Info();

                                if (0 != ServiceContext.Current.ServiceSlots.Count)
                                {
                                    ServiceSlot[] slots = new ServiceSlot[ServiceContext.Current.ServiceSlots.Count];

                                    ServiceContext.Current.ServiceSlots.CopyTo(slots);

                                    foreach (ServiceSlot slot in slots)
                                    {
                                        string.Format("正在停止服务：{0}", slot.Name).Info();

                                        (new CStarterClient()).Stop(srvConfig.ServiceInfo.Name, slot.Name, slot.Signal);

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

                                "停止监听服务".Info();

                                CStarterDControlServiceDaemon.Current.Stop();
                                CStarterDNotifierServiceDaemon.Current.Stop();

                                Environment.Exit(0);
                            }
                            else
                            {
                                CStarterDControlServiceDaemon.Current.Stop();
                                CStarterDNotifierServiceDaemon.Current.Stop();

                                Environment.Exit(-1);
                            }
                        }
                        else
                        {
                            "监听服务启动失败，无法继续启动".Error();
                            Environment.Exit(-1);
                        }
                    }
                }
                catch (Exception eX)
                {
                    "servicestarter:".Error();
                    eX.Message.Error();
                    eX.Exception();
                    "使用命令cstarterd --help获取更多命令帮助".Info();
                }
            }
            else
            {
                var srv = new WindowsService()
                {
                    ServiceName = srvConfig.ServiceInfo.DisplayName
                };

                srv.Initialize(srvConfig);

                ServiceBase.Run(srv);
            }
        }
    }
}
