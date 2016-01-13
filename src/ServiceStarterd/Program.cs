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
using log4net.Core;
using log4net.Filter;
using log4net.Repository;

namespace CStarterD
{
    class Program
    {
        static void Version()
        {
            "服务守护程序，版本号：1.0".Info();
        }

        static void ShowHelp(OptionSet p)
        {
            "用法：CStarterD [Options]+".Info();
            "对服务做出相应的操作指令".Info();

            p.WriteOptionDescriptions(Console.Out);
        }

        static ColoredConsoleAppender CreateColoredConsoleAppender(string domain)
        {
            ColoredConsoleAppender retValue = new ColoredConsoleAppender();

            retValue.Layout = new SyslogLayout()
            {
                StructuredDataPrefix = "CStarterD@" + domain
            };

            retValue.AddMapping(new ColoredConsoleAppender.LevelColors()
            {
                Level = Level.Debug,
                BackColor = ColoredConsoleAppender.Colors.White,
                ForeColor = ColoredConsoleAppender.Colors.Green
            });
            retValue.AddMapping(new ColoredConsoleAppender.LevelColors()
            {
                Level = Level.Info,
                BackColor = ColoredConsoleAppender.Colors.Green,
                ForeColor = ColoredConsoleAppender.Colors.White
            });
            retValue.AddMapping(new ColoredConsoleAppender.LevelColors()
            {
                Level = Level.Warn,
                BackColor = ColoredConsoleAppender.Colors.Cyan,
                ForeColor = ColoredConsoleAppender.Colors.White
            });
            retValue.AddMapping(new ColoredConsoleAppender.LevelColors()
            {
                Level = Level.Error,
                BackColor = ColoredConsoleAppender.Colors.Red,
                ForeColor = ColoredConsoleAppender.Colors.White
            });

            return retValue;
        }

        static void PrepareAppender(string domain, string minLevel)
        {
            string logRootFullName = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(new char[] { '\\' })).FullName,
                "logs", "CStarterd");

            foreach (ILoggerRepository respository in log4net.LogManager.GetAllRepositories())
            {
                Hierarchy hier = (Hierarchy)respository;

                foreach (var appender in hier.GetAppenders())
                {
                    MethodInfo addFilterMethod = appender.GetType().GetMethod("AddFilter", BindingFlags.Public | BindingFlags.Instance);

                    if (null != addFilterMethod)
                    {
                        var filter = new LogExceptionToFileFilter()
                        {
                            ExceptionLogFolder = Path.Combine(logRootFullName, appender.Name, "Exceptions")
                        };
                        addFilterMethod.Invoke(appender, new object[] { filter });
                        filter.ActivateOptions();
                    }

                    PropertyInfo fileProp = appender.GetType().GetProperty("File", BindingFlags.Public | BindingFlags.Instance);

                    if (null != fileProp)
                    {
                        fileProp.SetValue(appender, Path.Combine(logRootFullName, appender.Name, "logs.txt"), null);
                    }

                    PropertyInfo layoutProp = appender.GetType().GetProperty("Layout", BindingFlags.Public | BindingFlags.Instance);

                    if (null != layoutProp)
                    {
                        var layout = new SyslogLayout()
                        {
                            StructuredDataPrefix = "CStarterD@" + domain
                        };

                        layoutProp.SetValue(appender,
                            layout, null);

                        layout.ActivateOptions();
                    }

                    MethodInfo activeMethod = appender.GetType().GetMethod("ActivateOptions", BindingFlags.Public | BindingFlags.Instance);

                    if (null != activeMethod)
                        activeMethod.Invoke(appender, null);
                }

                PrepareLoggerLevel(respository, domain, minLevel);
            }

            PrepareRootLoggerLevel(minLevel);
        }

        static void PrepareLoggerLevel(ILoggerRepository repository, string domain, string level)
        {
            repository.Threshold = repository.LevelMap[level];
            log4net.Repository.Hierarchy.Hierarchy hier = (log4net.Repository.Hierarchy.Hierarchy)repository;
            log4net.Core.ILogger[] loggers = hier.GetCurrentLoggers();
            foreach (log4net.Core.ILogger logger in loggers)
            {
                Console.WriteLine(logger.Name);

                ((log4net.Repository.Hierarchy.Logger)logger).Level = hier.LevelMap[level];

                if(Environment.UserInteractive)
                {
                    ((log4net.Repository.Hierarchy.Logger)logger).RemoveAllAppenders();
                    ColoredConsoleAppender appender = CreateColoredConsoleAppender(domain);
                    ((log4net.Repository.Hierarchy.Logger)logger).AddAppender(appender);
                    (appender.Layout as SyslogLayout).ActivateOptions();
                    appender.ActivateOptions();
                }
            }
            hier.RaiseConfigurationChanged(EventArgs.Empty);
        }

        static void PrepareRootLoggerLevel(string level)
        {
            log4net.Repository.Hierarchy.Hierarchy h = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
            log4net.Repository.Hierarchy.Logger rootLogger = h.Root;
            rootLogger.Level = h.LevelMap[level];
        }

        static bool StartDaemons(ServiceStarterSection srvConfig)
        {
            bool retValue = false;

            try
            {
                "启动监听服务".Debug();

                CStarterDControlServiceDaemon.Current.Start(srvConfig);
                CStarterDNotifierServiceDaemon.Current.Start(srvConfig);

                "监听服务已经启动".Debug();

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
            ServiceContext.Current.IsDebug = param.IsDebug;

            PrepareAppender(srvConfig.ServiceInfo.Name, param.IsDebug ? "DEBUG" : "INFO");

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
                                string.Format("需要停止 {0} 个服务", ServiceContext.Current.ServiceSlots.Count).Debug();

                                if (0 != ServiceContext.Current.ServiceSlots.Count)
                                {
                                    ServiceSlot[] slots = new ServiceSlot[ServiceContext.Current.ServiceSlots.Count];

                                    ServiceContext.Current.ServiceSlots.CopyTo(slots);

                                    foreach (ServiceSlot slot in slots)
                                    {
                                        string.Format("正在停止服务：{0}", slot.Name).Debug();

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

                                "停止监听服务".Debug();

                                CStarterDControlServiceDaemon.Current.Stop();
                                CStarterDNotifierServiceDaemon.Current.Stop();

                                "服务完全停止".Info();

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
