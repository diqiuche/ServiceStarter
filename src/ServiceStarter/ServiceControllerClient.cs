using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace ServiceStarter
{
    public class ServiceControllerClient
    {
        public static string GetServices()
        {
            EndpointAddress ep = new EndpointAddress(string.Format("{0}/{1}", ServiceContext.Current.PipeUri,
                ServiceContext.Current.PipeEndPointName));

            string retValue = "";

            ActionResult result = null;

            ExecuteNamedPipeAction<IServiceController>(ep, (channel) =>
            {
                result = channel.GetServiceNames();
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

        public static string GetServiceInfo(string name)
        {
            EndpointAddress ep = new EndpointAddress(string.Format("{0}/{1}", ServiceContext.Current.PipeUri,
                ServiceContext.Current.PipeEndPointName));

            string retValue = "";
            ActionResult result = null;

            ExecuteNamedPipeAction<IServiceController>(ep, (channel) =>
            {
                result = channel.GetServiceInfo(name);
            });

            if (0 == result.Result)
                retValue = result.Data as string;
            else
            {
                result.Message.Error();
            }

            return retValue;
        }

        public static void StartService(string name)
        {
            EndpointAddress ep = new EndpointAddress(string.Format("{0}/{1}", ServiceContext.Current.PipeUri,
                ServiceContext.Current.PipeEndPointName));

            ActionResult result = null;

            ExecuteNamedPipeAction<IServiceController>(ep, (channel) =>
            {
                result = channel.StartService(name);
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

        public static void StopService(string name)
        {
            EndpointAddress ep = new EndpointAddress(string.Format("{0}/{1}", ServiceContext.Current.PipeUri,
                ServiceContext.Current.PipeEndPointName));

            ActionResult result = null;

            ExecuteNamedPipeAction<IServiceController>(ep, (channel) =>
            {
                result = channel.StopService(name);
            });

            if(0 == result.Result)
            {
                "等待服务 {0} 停止".Formate(name).Info();

                EventWaitHandle stopWaitSignal = new EventWaitHandle(false, EventResetMode.ManualReset, "stopservice_" + result.Data.ToString());
                stopWaitSignal.WaitOne(10 * 1000);

                ExecuteNamedPipeAction<IServiceController>(ep, (channel) =>
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
                });
            }
            else
            {
                result.Message.Error();
            }
        }

        private static void ExecuteNamedPipeAction<T>(EndpointAddress ep, Action<T> executeAction)
        {
            T proxy = default(T);

            bool isContinued = false;

            ChannelFactory<T> factory = null;

            try
            {
                factory = new ChannelFactory<T>(new NetNamedPipeBinding(), ep);

                proxy = factory.CreateChannel();

                isContinued = true;
            }
            catch(Exception eX)
            {
                eX.Exception();
            }

            if(isContinued)
            {
                try
                {
                    executeAction(proxy);

                    factory.Close();
                }
                catch (CommunicationException eX)
                {
                    eX.Exception();
                    factory.Abort();
                }
                catch (TimeoutException eX)
                {
                    eX.Exception();
                    factory.Abort();
                }
                catch (Exception eX)
                {
                    factory.Abort();
                    eX.Exception();
                }
            }
        }
    }
}
