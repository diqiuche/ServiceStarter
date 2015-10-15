using OptionsSharp;
using ServiceStarter.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStarter
{
    class Program
    {
        static void Install()
        {
            string.Format("安装服务：{0}:{1}", ServiceContext.Current.ServiceInfo.DisplayName,
                        ServiceContext.Current.ServiceInfo.ServiceName).Info();

            try
            {
                BasicServiceInstaller.Install(ServiceContext.Current.ServiceInfo.DisplayName,
                    ServiceContext.Current.ServiceInfo.ServiceName,
                    ServiceContext.Current.ServiceInfo.Description);
            }
            catch(Exception eX)
            {
                eX.Exception();
            }
        }

        static void UnInstall()
        {
            string.Format("卸载服务：{0}:{1}", ServiceContext.Current.ServiceInfo.DisplayName,
                        ServiceContext.Current.ServiceInfo.ServiceName).Info();
            try
            {
                BasicServiceInstaller.Uninstall(ServiceContext.Current.ServiceInfo.DisplayName,
                    ServiceContext.Current.ServiceInfo.ServiceName,
                    ServiceContext.Current.ServiceInfo.Description);
            }
            catch(Exception eX)
            {
                eX.Exception();
            }
        }

        static void Version()
        {
            "服务启动通用程序，版本号：1.0".Info(false);
        }

        static void RestartService(string name)
        {
            string.Format("尝试重新启动服务：{0}", name).Info();
        }

        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        [STAThread]
        static void Main(string[] args)
        {
            bool isShowHelp = false;
            bool isInstall = false;
            bool isUninstall = false;
            bool isStop = false;
            bool isRun = false;
            bool isShowVersion = false;
            bool isListServices = false;
            bool isShowServiceInfo = false;

            string restartName = string.Empty;

            string runMode = "Process";

            string targetService = string.Empty;

            string signal = string.Empty;

            List<string> paramValues = new List<string>();

            log4net.Config.XmlConfigurator.Configure();

            var p = new OptionSet(){
                { "i|install", "安装服务", v => isInstall = true},
                { "u|uninstall", "卸载服务", v => isUninstall = true},
                { "v|version", "显示版本号", v => isShowVersion = true},
                { "o|stop=", "停止指定的服务", v => { isStop = true; restartName = v; } },
                { "r|run=", "运行指定的服务", v => { isRun = true; restartName = v; } },
                { "l|list", "列出服务列表", v => isListServices = true },
                { "n|info=", "列出服务信息", v => { targetService = v; isShowServiceInfo = true; } },
                { "m|mode=", "服务运行模式（Daemon守护进程|Process服务进程）", v => runMode = v??"Process" },
                { "s|service=", "运行指定名称的服务（该服务必须已经配置在配置文件中）", v => targetService = v },
                { "g|signal=", "进程通讯的信号量", v => signal = v },
                { "h|help", "显示帮助", v=>isShowHelp=true}
            };

            List<string> extra;

            try
            {
                extra = p.Parse(args);

                if (isShowHelp)
                {
                    ShowHelp(p);
                    (string.Join(",", extra.ToArray())).Info();
                    return;
                } else if(isShowVersion)
                {
                    Version();
                }
                else if(isInstall)
                {
                    Install();
                }
                else if(isUninstall)
                {
                    UnInstall();
                }
                else if(isListServices)
                {
                    string names = ServiceControllerClient.GetServices();

                    names.Info(false);
                }
                else if(isShowServiceInfo)
                {
                    string info = ServiceControllerClient.GetServiceInfo(targetService);

                    info.Info(false);
                }
                else if(isStop)
                {
                    if(!string.IsNullOrEmpty(restartName))
                    {
                        ServiceControllerClient.StopService(restartName);
                    }
                }
                else if(isRun)
                {
                    if(!string.IsNullOrEmpty(restartName))
                    {
                        ServiceControllerClient.StartService(restartName);
                    }
                }
                else
                {
                    switch(runMode)
                    {
                        case "Daemon":
                            ServiceContext.Current.RunMode = "Daemon";

                            "启动监听服务".Info();

                            ServiceControllerDaemon.Current.Start();

                            "监听服务已经启动".Info();

                            if (Environment.UserInteractive)
                            {
                                BasicServiceStarter.Run(new string[] { });

                                "按任意键关闭程序".Info();
                                Console.ReadLine();

                                "程序正在退出，请不要关闭窗口".Info();
                                string.Format("需要停止 {0} 个服务", ServiceContext.Current.ServiceSlots.Count).Info();

                                "停止监听服务".Info();
                                ServiceControllerDaemon.Current.Stop();

                                if(0 != ServiceContext.Current.ServiceSlots.Count)
                                {
                                    ServiceSlot[] slots = new ServiceSlot[ServiceContext.Current.ServiceSlots.Count];

                                    ServiceContext.Current.ServiceSlots.CopyTo(slots);

                                    foreach (ServiceSlot slot in slots)
                                    {
                                        string.Format("正在停止服务：{0}", slot.Name).Info();
                                        EventWaitHandle doneSignal = new EventWaitHandle(false, EventResetMode.ManualReset, slot.Signal);
                                        doneSignal.Set();

                                        EventWaitHandle waitExitSignal = new EventWaitHandle(false, EventResetMode.ManualReset, "exit_" + slot.Signal);
                                        waitExitSignal.WaitOne();
                                    }
                                }
                            }
                            else
                            {
                                ServiceBase.Run(new WindowsService()
                                {
                                    ServiceName = ServiceContext.Current.ServiceInfo.DisplayName
                                });
                            }
                            break;
                        case "Process":
                            ServiceContext.Current.RunMode = "Process";
                            if(string.IsNullOrEmpty(targetService))
                            {
                                "进程模式下，必须指定一个要运行的服务".Error();
                            }
                            else if(string.IsNullOrEmpty(signal))
                            {
                                "进程模式下，必须指定一个协调运行的信号量".Error();
                            }
                            else
                            {
                                string msg = "";

                                if (!BasicServiceStarter.RunService(targetService, out msg))
                                {
                                    msg.Error();
                                }

                                EventWaitHandle completeSignal = new EventWaitHandle(false, EventResetMode.ManualReset, "complete_" + signal);
                                completeSignal.Set();

                                EventWaitHandle waitForSignal = new EventWaitHandle(false, EventResetMode.ManualReset, signal);
                                waitForSignal.WaitOne();

                                string.Format("服务 {0} 正在退出", targetService).Info();

                                if (0 != ServiceContext.Current.Domains.Count)
                                {
                                    Stack<string> unloadStack = new Stack<string>();

                                    foreach (string k in ServiceContext.Current.Domains.Keys)
                                    {
                                        unloadStack.Push(k);
                                    }

                                    while (0 != unloadStack.Count)
                                    {
                                        string appName = unloadStack.Pop();
                                        string.Format("正在停止服务：{0}", appName).Info();
                                        try
                                        {
                                            if (ServiceContext.Current.Services.ContainsKey(appName))
                                            {
                                                ServiceContext.Current.Services[appName].Stop();
                                            }

                                            //AppDomain.Unload(ServiceContext.Current.Domains[appName]);

                                            ServiceContext.Current.Services.Remove(appName);
                                            ServiceContext.Current.Domains.Remove(appName);

                                            string.Format("服务：{0} 已经被停止", appName).Info();
                                        }
                                        catch (AppDomainUnloadedException eX)
                                        {
                                            string.Format("卸载服务：{0} 发生错误：{1}", appName, eX.Message).Error();
                                            eX.Exception();
                                        }
                                        catch (CannotUnloadAppDomainException eX)
                                        {
                                            string.Format("无法卸载服务：{0}。发生错误：{1}", appName, eX.Message).Error();
                                            eX.Exception();
                                        }
                                    }
                                }

                                string.Format("服务 {0} 已经完全退出", targetService).Info();
                                EventWaitHandle waitExitSignal = new EventWaitHandle(false, EventResetMode.ManualReset, "exit_" + signal);
                                waitExitSignal.Set();
                            }
                            break;
                    }
                }
            }
            catch (OptionException eX)
            {
                "servicestarter:".Error();
                eX.Message.Error();
                eX.Exception();
                "使用命令servicestarter --help获取更多命令帮助".Info();
            }
            
        }

        static void ShowHelp(OptionSet p)
        {
            "用法：servicestarter [Options]+".Info(false);
            "对服务做出相应的操作指令".Info(false);
            "如果不指定任何参数，那么将启动服务".Info(false);

            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
