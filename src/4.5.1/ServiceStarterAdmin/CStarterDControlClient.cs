using CStarter.Configuration;
using CStarterD.Common;
using CStarter.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace CStarterAdmin
{
    public class CStarterDControlClient : NamedPipeClient<ICStarterDControl>
    {
        public string GetServices(ServiceStarterSection srvConfig)
        {
            EndpointAddress ep = new EndpointAddress(string.Format("net.pipe://localhost/{0}/{1}", srvConfig.ServiceInfo.Name,
                "control"));

            string retValue = "";

            ActionResult result = null;

            ExecuteNamedPipeAction(ep, (channel) =>
            {
                result = channel.GetServiceNames();
            },
            3,
            (msg) =>
            {
                result.Result = -1;
                result.Message = msg;
            });

            if (0 == result.Result)
            {
                retValue = (string)result.Data;
            }
            else
            {
                result.Message.Error();
            }

            return retValue;
        }

        public string GetServiceInfo(ServiceStarterSection srvConfig, string name)
        {
            EndpointAddress ep = new EndpointAddress(string.Format("net.pipe://localhost/{0}/{1}", srvConfig.ServiceInfo.Name,
                "control"));

            string retValue = "";
            ActionResult result = null;

            ExecuteNamedPipeAction(ep, (channel) =>
            {
                result = channel.GetServiceInfo(name);
            },
            3,
            (msg) =>
            {
                result.Result = -1;
                result.Message = msg;
            });

            if (0 == result.Result)
                retValue = result.Data as string;
            else
            {
                result.Message.Error();
            }

            return retValue;
        }

        public void StartService(ServiceStarterSection srvConfig, string name)
        {
            EndpointAddress ep = new EndpointAddress(string.Format("net.pipe://localhost/{0}/{1}", srvConfig.ServiceInfo.Name,
                "control"));

            ActionResult result = null;

            ExecuteNamedPipeAction(ep, (channel) =>
            {
                result = channel.StartService(name);
            },
            3,
            (msg) =>
            {
                result.Result = -1;
                result.Message = msg;
            });

            if(0 == result.Result)
            {
                "服务 {0} 成功启动".Formate(name).Info();
            }
            else
            {
                "服务 {0} 启动失败：{1}".Formate(name, result.Message).Error();
            }
        }

        public void StopService(ServiceStarterSection srvConfig, string name)
        {
            EndpointAddress ep = new EndpointAddress(string.Format("net.pipe://localhost/{0}/{1}", srvConfig.ServiceInfo.Name,
                "control"));

            ActionResult result = null;

            "等待服务 {0} 停止".Formate(name).Info();

            ExecuteNamedPipeAction(ep, (channel) =>
            {
                result = channel.StopService(name, 10);
            },
            3,
            (msg) =>
            {
                result.Result = -1;
                result.Message = msg;
            });

            if(0 == result.Result)
            {
                ExecuteNamedPipeAction(ep, (channel) =>
                {
                    ActionResult chkResult = channel.CheckService(name);

                    if (-1 == chkResult.Result)
                    {
                        chkResult.Message.Error();
                    }
                    else
                    {
                        if (0 == chkResult.Result)
                        {
                            "服务 {0} 成功停止".Formate(name).Info();
                        }
                        else
                        {
                            "服务 {0} 停止失败".Formate(name).Error();
                        }
                    }
                },
                3,
                (msg) =>
                {
                    msg.Error();
                });
            }
            else
            {
                result.Message.Error();
            }
        }
    }
}
