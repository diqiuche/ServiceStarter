using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStarter.Common
{
    public class BasicService<T> : ServiceBase where T : IService
    {
        private IService m_Service;

        protected override void OnStart(string[] args)
        {
            try
            {
                m_Service = Activator.CreateInstance(typeof(T)) as IService;
            }
            catch (Exception eX)
            {
                eX.WriteExceptionEvent(10001);
                throw eX;
            }
            if (null != m_Service)
            {
                m_Service.Start();
            }
            else
            {
                string.Format("要启动的服务：{0}，不是继承于IService的服务，无法启动", typeof(T).FullName).WriteErrorEvent(10002);
            }
        }

        protected override void OnStop()
        {
            if (null != m_Service)
                m_Service.Dispose();
        }
    }

    public class BasicService : ServiceBase
    {
        private IService m_Service;

        private string m_TypeName;

        private string m_TypeAssemblyName;

        private Assembly m_TypeAssembly;

        private Type m_ServiceType;

        public BasicService(string assemblyName, string typeName)
            : base()
        {
            m_TypeAssemblyName = assemblyName;
            m_TypeName = typeName;

            var location = System.Reflection.Assembly.GetEntryAssembly().Location;
            var directoryPath = Path.GetDirectoryName(location);

            var asmFile = Path.Combine(directoryPath, assemblyName + ".dll");

            try
            {
                m_TypeAssembly = Assembly.LoadFrom(asmFile);
            }
            catch (Exception eX)
            {
                eX.WriteExceptionEvent(10003);
                throw eX;
            }

            try
            {
                m_ServiceType = m_TypeAssembly.GetType(assemblyName + "." + m_TypeName, true, true);
            }
            catch (Exception eX)
            {
                eX.WriteExceptionEvent(10004);
                throw eX;
            }

            if (!typeof(IService).IsAssignableFrom(m_ServiceType))
            {
                string.Format("要启动的服务：{0}，不是继承于IService的服务，无法启动", assemblyName + "." + m_TypeName).WriteErrorEvent(10002);
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                m_Service = Activator.CreateInstance(m_ServiceType) as IService;
            }
            catch (Exception eX)
            {
                eX.WriteExceptionEvent(10001);
                throw eX;
            }
            if (null != m_Service)
            {
                m_Service.Start();
            }
            else
            {
                string.Format("要启动的服务：{0}，不是继承于IService的服务，无法启动", m_TypeName).WriteErrorEvent(10002);
            }
        }
    }
}
