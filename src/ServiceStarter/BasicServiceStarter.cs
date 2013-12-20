using ServiceStarter.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStarter
{
    public static class BasicServiceStarter
    {
        public static void Run<T>(string displayName, string serviceName) where T : IService
        {
            if (Environment.UserInteractive)
            {
                var cmd = (Environment.GetCommandLineArgs().Skip(1).FirstOrDefault() ?? "").ToLower();

                switch (cmd)
                {
                    case "i":
                    case "install":
                        Console.WriteLine("安装服务：{0}:{1}", displayName, serviceName);
                        BasicServiceInstaller.Install(displayName, serviceName);
                        break;
                    case "u":
                    case "uninstall":
                        Console.WriteLine("卸载服务：{0}:{1}", displayName, serviceName);
                        BasicServiceInstaller.Uninstall(displayName, serviceName);
                        break;
                    default:
                        try
                        {
                            using (var service = (IService)Activator.CreateInstance(typeof(T)))
                            {
                                service.Start();
                                string.Format("服务：{0} 开始运行，按任意键停止", displayName).WriteInfo();
                                Console.ReadLine();
                            }
                        }
                        catch (Exception eX)
                        {
                            eX.WriteException();
                            "按任意键终止程序".WriteInfo();
                            Console.ReadLine();
                        }
                        break;
                }
            }
            else
            {
                ServiceBase.Run(new BasicService<T>() { ServiceName = serviceName });
            }
        }

        public static void Run(string assemblyName, string typeName, string displayName, string serviceName)
        {
            if (Environment.UserInteractive)
            {
                var cmd = (Environment.GetCommandLineArgs().Skip(1).FirstOrDefault() ?? "").ToLower();

                switch (cmd)
                {
                    case "i":
                    case "install":
                        Console.WriteLine("安装服务：{0}:{1}", displayName, serviceName);
                        BasicServiceInstaller.Install(displayName, serviceName);
                        break;
                    case "u":
                    case "uninstall":
                        Console.WriteLine("卸载服务：{0}:{1}", displayName, serviceName);
                        BasicServiceInstaller.Uninstall(displayName, serviceName);
                        break;
                    default:
                        try
                        {
                            Assembly asm = Assembly.LoadFrom(assemblyName + ".dll");

                            if (null == asm)
                            {
                                string.Format("无法加载服务组件：{0}", assemblyName + ".dll").WriteWarningInfo();
                            }
                            else
                            {
                                Type srvType = asm.GetType(assemblyName + "." + typeName, true, true);

                                if (null == srvType)
                                {
                                    string.Format("无法找到服务类型：{0}" + assemblyName + "." + typeName).WriteWarningInfo();
                                }
                                else
                                {
                                    using (var service = (IService)Activator.CreateInstance(srvType))
                                    {
                                        service.Start();
                                        string.Format("服务：{0} 开始运行，按任意键停止", displayName).WriteInfo();
                                        Console.ReadLine();
                                    }
                                }
                            }
                        }
                        catch (Exception eX)
                        {
                            eX.WriteException();
                            "按任意键终止程序".WriteInfo();
                            Console.ReadLine();
                        }
                        break;
                }
            }
            else
            {
                ServiceBase.Run(new BasicService(assemblyName, typeName) { ServiceName = serviceName });
            }
        }
    }
}
