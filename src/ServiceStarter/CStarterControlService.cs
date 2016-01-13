using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using CStarter.Utils;
using CStarter.Contracts;

namespace CStarter
{
    public class CStarterControlServiceDaemon
    {
        private static CStarterControlServiceDaemon _Instance;

        private static object _Locker = new object();

        private CStarterControlService _Controller;

        private CStarterControlServiceDaemon()
        {
            _Controller = new CStarterControlService();
        }

        public static CStarterControlServiceDaemon Current
        {
            get
            {
                if (null == _Instance)
                {
                    lock (_Locker)
                    {
                        if (null == _Instance)
                        {
                            _Instance = new CStarterControlServiceDaemon();
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

    public class CStarterControlService
    {
        ServiceHost _Host;
        CStarterController _Srv;

        bool _IsStarted = false;

        public CStarterControlService()
        {

        }

        public void Start()
        {
            try
            {
                "初始化通讯服务对象".Debug();

                _Srv = new CStarterController();

                "初始化通讯服务配置".Debug();
                string.Format("通讯服务监听地址：net.pipe://localhost/{0}/{1}/comm", ServiceContext.Current.Domain, ServiceContext.Current.Name).Debug();

                _Host = new ServiceHost(_Srv, new Uri(
                    string.Format("net.pipe://localhost/{0}/{1}", 
                    ServiceContext.Current.Domain, 
                    ServiceContext.Current.Name)));
                _Host.AddServiceEndpoint(typeof(ICStarterControl), new NetNamedPipeBinding(), "comm");

                "开启监听服务".Debug();

                _Host.Open();

                _IsStarted = true;
            }
            catch (Exception eX)
            {
                eX.Message.Error();
                eX.Exception();
            }
        }

        public void Stop()
        {
            if (_IsStarted)
            {
                if (CommunicationState.Closed != _Host.State)
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
