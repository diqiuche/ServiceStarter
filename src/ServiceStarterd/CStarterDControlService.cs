using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using CStarter.Utils;
using System.Configuration;
using CStarter.Configuration;
using CStarterD.Common;

namespace CStarterD
{
    public class CStarterDControlServiceDaemon
    {
        private static CStarterDControlServiceDaemon _Instance;

        private static object _Locker = new object();

        private CStarterDControlService _Controller;

        private CStarterDControlServiceDaemon()
        {
            _Controller = new CStarterDControlService();
        }

        public static CStarterDControlServiceDaemon Current
        {
            get
            {
                if(null == _Instance)
                {
                    lock(_Locker)
                    {
                        if (null == _Instance)
                        {
                            _Instance = new CStarterDControlServiceDaemon();
                        }
                    }
                }

                return _Instance;
            }
        }

        public void Start(ServiceStarterSection config)
        {
            _Controller.Start(config);
        }

        public void Stop()
        {
            _Controller.Stop();
        }
    }

    internal class CStarterDControlService : IDisposable
    {
        ServiceHost _Host;
        CStarterDController _Srv;
        ServiceStarterSection _SrvConfig;

        bool _IsStarted = false;

        public CStarterDControlService()
        {
            
        }

        public void Start(ServiceStarterSection config)
        {
            _SrvConfig = config;

            try
            {
                "初始化监听服务对象".Info();

                _Srv = new CStarterDController();

                "初始化监听服务配置".Info();
                string.Format("监听服务监听地址：{0}", "net.pipe://localhost/" + _SrvConfig.ServiceInfo.Name).Info();

                _Host = new ServiceHost(_Srv, new Uri("net.pipe://localhost/" + _SrvConfig.ServiceInfo.Name));
                _Host.AddServiceEndpoint(typeof(ICStarterDControl), new NetNamedPipeBinding(), "control");

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
