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

namespace CStarter
{
    class Program
    {
        static void PrepareLogger(string serviceName)
        {
            Hierarchy hier = log4net.LogManager.GetRepository() as Hierarchy;

            string logRootFullName = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(new char[] { '\\' })).FullName,
                "logs", "CStarter");

            if (null != hier)
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
                        StructuredDataPrefix = "CStarter@" + serviceName
                    };
                    commonAppender.Layout = layout;
                    layout.ActivateOptions();

                    commonAppender.ActivateOptions();
                }

                RollingFileAppender starterAppender = (RollingFileAppender)hier.GetAppenders().Where(a => a.Name.Equals("StarterAppender")).FirstOrDefault();

                if (null != starterAppender)
                {
                    starterAppender.File = Path.Combine(logRootFullName, "CStarter", "logs.txt");

                    var filter = new LogExceptionToFileFilter()
                    {
                        ExceptionLogFolder = Path.Combine(logRootFullName, "CStarter", "Exceptions")
                    };
                    starterAppender.AddFilter(filter);
                    filter.ActivateOptions();

                    var layout = new SyslogLayout()
                    {
                        StructuredDataPrefix = "CStarter@" + serviceName
                    };
                    starterAppender.Layout = layout;
                    layout.ActivateOptions();

                    starterAppender.ActivateOptions();
                }
            }
        }

        static void Version()
        {
            "服务进程启动程序，版本号：1.0".Info(false);
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

            var p = new OptionSet(){
                { "n|name=", "服务名称", v => ServiceContext.Current.Name = v },
                { "d|domain=", "服务域名称", v => ServiceContext.Current.Domain = v },
                { "g|signal=", "进程通讯的信号量", v => ServiceContext.Current.Signal = v },
                { "c|contentpath=", "运行路径", v => ServiceContext.Current.ContentPath = v },
                { "a|assemlyname=", "要运行的组件名称（不包含后缀）", v => { ServiceContext.Current.AssemblyName = v; } },
                { "t|type=", "要运行的类全名", v => { ServiceContext.Current.TargetType = v; } },
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
                PrepareLogger(ServiceContext.Current.Name);

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
            "用法：cstarter [Options]+".Info(false);
            "对服务做出相应的操作指令".Info(false);

            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
