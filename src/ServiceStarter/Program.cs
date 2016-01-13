using CStarter.OptionsSharp;
using CStarter.SDK;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CStarter.Utils;
using System.IO;
using log4net.Repository.Hierarchy;
using log4net.Appender;
using syslog4net.Filter;
using syslog4net.Layout;
using System.Security.AccessControl;
using log4net.Core;
using log4net.Filter;
using log4net.Repository;
using System.Reflection;

namespace CStarter
{
    class Program
    {
        static ColoredConsoleAppender CreateColoredConsoleAppender(string serviceName)
        {
            ColoredConsoleAppender retValue = new ColoredConsoleAppender();
            
            retValue.Layout = new SyslogLayout()
            {
                StructuredDataPrefix = "CStarter@" + serviceName
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

            //retValue.Target = "Console.Error";

            return retValue;
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

                if (Environment.UserInteractive)
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

        static void PrepareAppender(string serviceName, string minLevel)
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
                            StructuredDataPrefix = "CStarter@" + serviceName
                        };

                        layoutProp.SetValue(appender,
                            layout, null);

                        layout.ActivateOptions();
                    }

                    MethodInfo activeMethod = appender.GetType().GetMethod("ActivateOptions", BindingFlags.Public | BindingFlags.Instance);

                    if (null != activeMethod)
                        activeMethod.Invoke(appender, null);
                }

                PrepareLoggerLevel(respository, serviceName, minLevel);
            }

            PrepareRootLoggerLevel(minLevel);
        }

        static void Version()
        {
            "服务进程启动程序，版本号：1.0".Info();
        }

        static int CheckParams()
        {
            if (string.IsNullOrEmpty(ServiceContext.Current.Name))
                return -1;

            if (string.IsNullOrEmpty(ServiceContext.Current.Domain))
                return -2;

            if (string.IsNullOrEmpty(ServiceContext.Current.Signal))
                return -3;

            if (string.IsNullOrEmpty(ServiceContext.Current.ContentPath))
                return -4;

            if (!Path.IsPathRooted(ServiceContext.Current.ContentPath))
                return -5;

            if (string.IsNullOrEmpty(ServiceContext.Current.AssemblyName))
                return -6;

            if (string.IsNullOrEmpty(ServiceContext.Current.TargetType))
                return -7;

            return 0;
        }

        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        [STAThread]
        static void Main(string[] args)
        {
            List<string> paramValues = new List<string>();

            log4net.Config.XmlConfigurator.Configure();

            bool isDebug = false;

            var p = new OptionSet(){
                { "n|name=", "服务名称", v => ServiceContext.Current.Name = v },
                { "m|domain=", "服务域名称", v => ServiceContext.Current.Domain = v },
                { "g|signal=", "进程通讯的信号量", v => ServiceContext.Current.Signal = v },
                { "c|contentpath=", "运行路径", v => ServiceContext.Current.ContentPath = v },
                { "a|assemlyname=", "要运行的组件名称（不包含后缀）", v => { ServiceContext.Current.AssemblyName = v; } },
                { "t|type=", "要运行的类全名", v => { ServiceContext.Current.TargetType = v; } },
                { "d|debug=", "Debug模式(Y/N)，默认是N", v => isDebug = "Y" == v.ToUpper() },
                { "v|version=", "显示版本号", v => { } },
                { "h|help", "显示帮助", v => { } }
            };

            List<string> extra = null;

            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException eX)
            {
                "cstarter:".Error();
                eX.Message.Error();
                eX.Exception();
                "使用命令cstarter --help获取更多命令帮助".Info();
            }

            int result = CheckParams();

            if (0 != result)
            {
                if (Environment.UserInteractive)
                {
                    switch (result)
                    {
                        case -1:
                            "必须指定一个服务名称".Error();
                            break;
                        case -2:
                            "必须给定服务域".Error();
                            break;
                        case -3:
                            "必须指定一个信号量".Error();
                            break;
                        case -4:
                            "必须指定运行的路径".Error();
                            break;
                        case -5:
                            "运行路径必须是绝对路径".Error();
                            break;
                        case -6:
                            "必须指定入口类库".Error();
                            break;
                        case -7:
                            "必须指定入口类".Error();
                            break;
                        default:
                            ("启动发生未知错误：" + result.ToString()).Error();
                            break;
                    }

                    ShowHelp(p);
                }
                else
                {
                    Environment.Exit(result);
                }
            }
            else
            {
                PrepareAppender(ServiceContext.Current.Name, isDebug ? "DEBUG" : "INFO");

                bool isStarted = false;

                try
                {
                    string msg = "";

                    if (!BasicServiceStarter.CreateDomain(out msg))
                    {
                        "创建服务域失败，服务无法启动".Error();
                    }
                    else
                    {
                        if (!BasicServiceStarter.RunService(out msg))
                        {
                            msg.Error();
                        }
                        else
                        {
                            isStarted = true;
                        }
                    }
                }
                catch(Exception eX)
                {
                    eX.Exception();
                }

                if (isStarted)
                {
                    CStarterControlServiceDaemon.Current.Start();

                    (new CStarterDNotifyClient()).StartComplete(ServiceContext.Current.Domain,
                        ServiceContext.Current.Signal);

                    ServiceContext.Current.WaitForStopSemaphore();

                    string.Format("服务 {0} 正在退出", ServiceContext.Current.Name).Info();

                    string.Format("正在停止服务：{0}", ServiceContext.Current.TargetType).Info();

                    try
                    {
                        ServiceContext.Current.Service.Instance.Stop();
                        string.Format("服务：{0} 已经被停止", ServiceContext.Current.TargetType).Info();
                    }
                    catch (AppDomainUnloadedException eX)
                    {
                        string.Format("卸载服务：{0} 发生错误：{1}", ServiceContext.Current.TargetType, eX.Message).Error();
                        eX.Exception();
                    }
                    catch (CannotUnloadAppDomainException eX)
                    {
                        string.Format("无法卸载服务：{0}。发生错误：{1}", ServiceContext.Current.TargetType, eX.Message).Error();
                        eX.Exception();
                    }
                    catch (Exception eX)
                    {
                        string.Format("卸载服务：{0} 发生错误：{1}", ServiceContext.Current.TargetType, eX.Message).Error();
                        eX.Exception();
                    }

                    CStarterControlServiceDaemon.Current.Stop();

                    string.Format("服务 {0} 已经完全退出", ServiceContext.Current.Name).Info();

                    (new CStarterDNotifyClient()).StopComplete(ServiceContext.Current.Domain,
                        ServiceContext.Current.Signal);

                    Environment.Exit(0);
                }
                else
                {
                    Environment.Exit(-100);
                }
            }
        }

        static void ServiceDomain_DomainUnload(object sender, EventArgs e)
        {
            "服务进程 {0} 的服务域被卸载".Formate(ServiceContext.Current.Name).Error();
        }

        static void ShowHelp(OptionSet p)
        {
            "用法：cstarter [Options]+".Info();
            "对服务做出相应的操作指令".Info();

            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
