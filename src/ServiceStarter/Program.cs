using ServiceStarter.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceStarterSection configs = (ServiceStarterSection)ConfigurationManager.GetSection("serviceStarters");

            if (null != configs)
            {
                if (Environment.UserInteractive)
                {
                    string.Format("发现服务配置。共配置了：{0}项服务", configs.Services.Count).WriteInfo();
                }

                ServiceStarterElement defaultElement = null;

                foreach (ServiceStarterElement ele in configs.Services)
                {
                    if (Environment.UserInteractive)
                    {
                        string.Format(
                            "配置：{0}，服务：{1} : {4}，服务组件：{2}.{3}",
                            ele.Name, ele.DisplayName, ele.AssemlyName, ele.TypeName, ele.ServiceName).WriteActionInfo();
                    }
                    if (ele.Name == configs.Default)
                    {
                        defaultElement = ele;
                    }
                }

                if (null != defaultElement)
                {
                    if (Environment.UserInteractive)
                    {
                        string.Format(
                            "使用配置：{0}", configs.Default).WriteActionInfo();
                    }

                    BasicServiceStarter.Run(defaultElement.AssemlyName, defaultElement.TypeName,
                        defaultElement.DisplayName, defaultElement.ServiceName);
                }
                else
                {
                    if (Environment.UserInteractive)
                    {
                        string.Format("找不到配置{0}的服务信息", configs.Default).WriteWarningInfo();
                        Console.ReadLine();
                    }
                    else
                    {
                        string.Format("找不到配置{0}的服务信息", configs.Default).WriteErrorEvent(20001);
                    }
                }
            }
            else
            {
                if (Environment.UserInteractive)
                {
                    "找不到服务配置信息，启动失败。按任意键关闭程序".WriteWarningInfo();
                    Console.ReadLine();
                }
                else
                {
                    "找不到服务配置信息，启动失败。".WriteErrorEvent(20002);
                }
            }
        }
    }
}
