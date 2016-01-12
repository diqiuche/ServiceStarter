using log4net;
using CStarter.Configuration;
using CStarter.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CStarterD
{
    public static class BasicServiceStarter
    {
        public static bool RunServiceProcess(string domain, ServiceStarterElement eleConfig, out string msg)
        {
            bool retValue = false;

            string signal = Guid.NewGuid().ToString();

            msg = "";

            try
            {
                string contentFullPath = eleConfig.ContentPath;

                if(!Path.IsPathRooted(contentFullPath))
                {
                    contentFullPath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(new char[] { '\\' })).FullName, "services",
                        contentFullPath.TrimStart(new char[] { '~' }).TrimStart(new char[] { '/' }));
                }

                Process p = new Process();
                p.StartInfo.Arguments = string.Format(" -n={0} -g={1} -c=\"{2}\" -a={3} -t={4} -d={5}", eleConfig.Name, signal, contentFullPath,
                    eleConfig.AssemblyName, eleConfig.TypeName, domain);
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "cstarter.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.OutputDataReceived += new DataReceivedEventHandler(p_OutputDataReceived);
                p.ErrorDataReceived += new DataReceivedEventHandler(p_ErrorDataReceived);

                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();

                CancellationTokenSource token = new CancellationTokenSource();

                Task t = new Task(() =>
                {
                    if (p.HasExited)
                    {
                        "服务进程 {0} 已经退出".Formate(eleConfig.Name).Error();
                        ServiceContext.Current.ServiceStartedComplete();
                    }
                }, token.Token);

                string.Format("等待服务 {0} 启动完成", eleConfig.Name).Info();

                t.Start();

                if (ServiceContext.Current.WaitServiceStarting(30))
                {
                    token.Cancel();

                    if (!p.HasExited)
                    {
                        "服务{0}已经成功启动".Formate(eleConfig.Name).Info();

                        ServiceContext.Current.AddSlot(eleConfig, p, signal);

                        retValue = true;
                    }
                    else
                    {
                        switch (p.ExitCode)
                        {
                            case 0:
                                "程序已退出，但是未返回错误码".Error();
                                break;
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
                            case -100:
                                "服务进程启动过程中发生错误，请查看服务进程启动日志获取详细信息".Error();
                                break;
                            default:
                                ("启动发生未知错误：" + p.ExitCode.ToString()).Error();
                                break;
                        }
                    }
                }
                else
                {
                    token.Cancel();

                    "在限定的时间内，启动的服务无响应，服务{0}可能没有成功启动".Formate(eleConfig.Name).Warn();

                    ServiceContext.Current.AddSlot(eleConfig, p, signal);

                    retValue = true;
                }
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

        public static void Run(ServiceStarterSection serviceInfo)
        {
            if (null !=serviceInfo)
            {
                string.Format("服务正在使用 {0} 模式运行。", Environment.UserInteractive ? "交互" : "服务").Info();

                string.Format("共配置了：{0} 项服务", serviceInfo.Services.Count).Info();

                "解析服务启动顺序".Info();

                List<ServiceStarterElement> orderedConfigs = SortServices(serviceInfo);

                string.Format("服务按照如下顺序启动：{0}", string.Join("=>", (from n in orderedConfigs select n.Name).ToArray())).Info();

                try
                {
                    foreach (ServiceStarterElement eleConfig in orderedConfigs)
                    {
                        string msg = "";

                        "启动服务 {0} 的进程".Formate(eleConfig.Name).Info();

                        bool isContinued = true;

                        if (0 != eleConfig.DependenceServices.Count)
                        {
                            string.Format("服务 {0} 依赖服务 {1}", eleConfig.Name, 
                                string.Join(",", (from refSrv in eleConfig.DependenceServices.Cast<DependenceServiceConfigurationElement>()
                                                                                               select refSrv.ServiceName).ToArray())).Info();

                            foreach(DependenceServiceConfigurationElement dependedOnSrv in eleConfig.DependenceServices)
                            {
                                "检测依赖服务 {0}".Formate(dependedOnSrv.ServiceName).Info();
                                if (null == ServiceContext.Current.ServiceSlots.FirstOrDefault(s => s.Name == dependedOnSrv.ServiceName))
                                {
                                    "依赖服务 {0} 未启动，服务启动失败".Formate(dependedOnSrv.ServiceName).Error();
                                    isContinued = false;
                                    break;
                                }
                            }
                        }
                        
                        if(isContinued)
                        {
                            if (!RunServiceProcess(serviceInfo.ServiceInfo.Name, eleConfig, out msg))
                            {
                                msg.Error();
                            }
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

        private static List<ServiceStarterElement> SortServices(ServiceStarterSection serviceInfo)
        {
            List<ServiceStarterElement> retValue = new List<ServiceStarterElement>();

            List<RefCount> countList = new List<RefCount>();

            foreach (ServiceStarterElement ele in serviceInfo.Services)
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
