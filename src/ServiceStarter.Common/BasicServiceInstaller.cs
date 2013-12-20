using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStarter.Common
{
    public static class BasicServiceInstaller
    {
        public static Installer CreateInstaller(string displayName, string serviceName)
        {
            var installer = new TransactedInstaller();

            installer.Installers.Add(new ServiceInstaller
            {
                DisplayName = displayName,
                ServiceName = serviceName,
                StartType = ServiceStartMode.Automatic
            });

            installer.Installers.Add(new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            });

            var installContext = new InstallContext(serviceName + ".install.log", null);

            installContext.Parameters["assemblypath"] = Assembly.GetEntryAssembly().Location;

            installer.Context = installContext;

            return installer;
        }

        public static void Install(string displayName, string serviceName)
        {
            CreateInstaller(displayName, serviceName).Install(new Hashtable());
        }

        public static void Uninstall(string displayName, string serviceName)
        {
            CreateInstaller(displayName, serviceName).Uninstall(null);
        }
    }
}
