using ServiceStarter.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStarter
{
    class Program
    {
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        [STAThread]
        static void Main(string[] args)
        {
            var cmd = (Environment.GetCommandLineArgs().Skip(1).FirstOrDefault() ?? "").ToLower();

            log4net.Config.XmlConfigurator.Configure();

            if (Environment.UserInteractive)
            {
                BasicServiceStarter.Run(new string[] { cmd });

                "服务启动完毕，按任意键可以退出".Info();

                Console.ReadLine();

                string.Format("需要停止 {0} 个服务", ServiceContext.Current.Domains.Count).Info();

                if (0 != ServiceContext.Current.Domains.Count)
                {
                    foreach (KeyValuePair<string, AppDomain> appPair in ServiceContext.Current.Domains)
                    {
                        string.Format("正在停止服务：{0}", appPair.Key).Info();
                        AppDomain.Unload(appPair.Value);
                        string.Format("服务：{0} 已经被停止", appPair.Key).Info();
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
        }
    }
}
