using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStarter
{
    public static class BasicServiceInstaller
    {
        public static Installer CreateInstaller(string displayName, string serviceName, string description)
        {
            var installer = new TransactedInstaller();

            var install = new ServiceInstaller(){
                DisplayName = displayName,
                ServiceName = serviceName,
                StartType = ServiceStartMode.Automatic,
                Description = description
            };

            installer.Installers.Add(install);

            installer.Installers.Add(new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            });

            var installContext = new InstallContext(serviceName + ".install.log", null);

            installContext.Parameters["assemblypath"] = "\"" + Assembly.GetEntryAssembly().Location + "\"" + " -m=Daemon";
            //installContext.Parameters["assemblypath"] = Assembly.GetEntryAssembly().Location ;
            installer.Context = installContext;

            return installer;
        }

        public static void Install(string displayName, string serviceName, string description)
        {
            CreateInstaller(displayName, serviceName, description).Install(new Hashtable());
        }

        public static void Uninstall(string displayName, string serviceName, string description)
        {
            CreateInstaller(displayName, serviceName, description).Uninstall(null);
        }
    }
}
