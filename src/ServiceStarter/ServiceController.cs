using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace ServiceStarter
{
    public class ServiceControllerDaemon
    {
        private static ServiceControllerDaemon _Instance;

        private static object _Locker = new object();

        private ServiceController _Controller;

        private ServiceControllerDaemon()
        {
            _Controller = new ServiceController();
        }

        public static ServiceControllerDaemon Current
        {
            get
            {
                if(null == _Instance)
                {
                    lock(_Locker)
                    {
                        if (null == _Instance)
                        {
                            _Instance = new ServiceControllerDaemon();
                        }
                    }
                }

                return _Instance;
            }
        }

        public void Start()
        {
            _Controller.Start();
        }

        public void Stop()
        {
            _Controller.Stop();
        }
    }

    public class ServiceController : IDisposable
    {
        ServiceHost _Host;
        ServiceControllerService _Srv;

        bool _IsStarted = false;

        public ServiceController()
        {
            
        }

        public void Start()
        {
            try
            {
                "初始化监听服务对象".Info();

                _Srv = new ServiceControllerService();

                "初始化监听服务配置".Info();
                string.Format("监听服务监听地址：{0}", ServiceContext.Current.PipeUri).Info();

                _Host = new ServiceHost(_Srv, new Uri(ServiceContext.Current.PipeUri));
                _Host.AddServiceEndpoint(typeof(IServiceController), new NetNamedPipeBinding(), ServiceContext.Current.PipeEndPointName);

                "开启监听服务".Info();

                _Host.Open();

                _IsStarted = true;
            }
            catch(Exception eX)
            {
                eX.Message.Error();
                eX.Exception();
            }
        }

        public void Stop()
        {
            if(_IsStarted)
            {
                if(CommunicationState.Closed != _Host.State)
                {
                    _Host.Close();

                    _IsStarted = false;
                }
            }
        }

        public void Dispose()
        {
            Stop();

            if (null != _Srv)
                _Srv = null;
        }
    }
}
