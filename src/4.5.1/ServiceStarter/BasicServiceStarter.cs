using log4net;
using CStarter.SDK;
using CStarter.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CStarter
{
    public static class BasicServiceStarter
    {
        public static bool RunService(out string msg)
        {
            bool retValue = false;

            msg = "";

            try
            {
                string.Format("启动位于 {0} 位置的 {1} 服务，并启动实例： {2}",
                    ServiceContext.Current.ContentPath,
                    ServiceContext.Current.Name,
                    ServiceContext.Current.TargetType)
                    .Info();

                AppDomainSetup oSetup = new AppDomainSetup();
                oSetup.ApplicationName = ServiceContext.Current.Name;
                oSetup.ApplicationBase = ServiceContext.Current.ContentPath;

                //oSetup.PrivateBinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "private");
                oSetup.CachePath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(new char[] { '\\' })).FullName,
                    "Cache");
                oSetup.ShadowCopyFiles = "true"; //启用影像复制程序集
                oSetup.ShadowCopyDirectories = oSetup.ApplicationBase;
                oSetup.ConfigurationFile = Path.Combine(oSetup.ApplicationBase, ServiceContext.Current.AssemblyName + ".dll.config");
                oSetup.PrivateBinPath = oSetup.ApplicationBase;

                string.Format("基础路径：{0}", oSetup.ApplicationBase).Info();
                string.Format("BIN路径：{0}", oSetup.PrivateBinPath).Info();
                string.Format("镜像路径：{0}", oSetup.ShadowCopyDirectories).Info();
                string.Format("缓存路径：{0}", oSetup.CachePath).Info();
                string.Format("配置文件：{0}", oSetup.ConfigurationFile).Info();

                AppDomain newDomain = AppDomain.CreateDomain(ServiceContext.Current.Name,
                    AppDomain.CurrentDomain.Evidence,
                    oSetup);

                newDomain.SetData("logRoot", 
                    Path.Combine(
                    Directory.GetParent(
                    AppDomain.CurrentDomain.BaseDirectory.TrimEnd(new char[] { '\\' })).FullName,
                    "logs"));

                IService service = newDomain.CreateInstanceAndUnwrap(ServiceContext.Current.AssemblyName,
                    ServiceContext.Current.TargetType) as IService;

                Sponsor<IService> s = new Sponsor<IService>(service);

                service.Start();

                ServiceContext.Current.ServiceDomain = newDomain;
                ServiceContext.Current.Service = s;

                retValue = true;
            }
            catch (Exception eX)
            {
                msg = eX.Message;
                msg.Error();
                eX.Exception();
            }

            return retValue;
        }

        private static void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            e.Data.Info(false);
        }

        private static void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            e.Data.Error(false);
        }
    }
}
