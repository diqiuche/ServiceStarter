using CStarter.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using CStarter.Utils;
using CStarterD.Contracts;

namespace CStarterD
{
    public class CStarterDNotifierServiceDaemon
    {
        private static CStarterDNotifierServiceDaemon _Instance;

        private static object _Locker = new object();

        private CStarterDNotifierService _Controller;

        private CStarterDNotifierServiceDaemon()
        {
            _Controller = new CStarterDNotifierService();
        }

        public static CStarterDNotifierServiceDaemon Current
        {
            get
            {
                if(null == _Instance)
                {
                    lock(_Locker)
                    {
                        if (null == _Instance)
                        {
                            _Instance = new CStarterDNotifierServiceDaemon();
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

    internal class CStarterDNotifierService
    {
        ServiceHost _Host;
        CStarterDNotifier _Srv;
        ServiceStarterSection _SrvConfig;

        bool _IsStarted = false;

        public CStarterDNotifierService()
        {
            
        }

        public void Start(ServiceStarterSection config)
        {
            _SrvConfig = config;

            try
            {
                "初始化通讯服务对象".Debug();

                _Srv = new CStarterDNotifier();

                "初始化通讯服务配置".Debug();
                string.Format("通讯服务监听地址：{0}/comm", "net.pipe://localhost/" + _SrvConfig.ServiceInfo.Name).Debug();

                _Host = new ServiceHost(_Srv, new Uri("net.pipe://localhost/" + _SrvConfig.ServiceInfo.Name));
                _Host.AddServiceEndpoint(typeof(ICStarterDNotifier), new NetNamedPipeBinding(), "comm");

                "开启监听服务".Debug();

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
