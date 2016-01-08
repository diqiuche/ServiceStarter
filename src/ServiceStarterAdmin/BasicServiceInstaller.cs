using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CStarterAdmin
{
    public static class BasicServiceInstaller
    {
        public static Installer CreateInstaller(string displayName, string serviceName, string description, string dependedon, string configFile)
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

            string starterFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CStarterD.exe");

            if (!string.IsNullOrEmpty(configFile))
            {
                installContext.Parameters["assemblypath"] = "\"" + starterFullPath + "\"" + " -c=" + configFile;
            }
            else
            {
                installContext.Parameters["assemblypath"] = "\"" + starterFullPath + "\"";
            }

            if(!string.IsNullOrEmpty(dependedon))
            {
                install.ServicesDependedOn = dependedon.Split(new char[] { ',' });
            }

            installer.Context = installContext;

            return installer;
        }

        public static void Install(string displayName, string serviceName, string description, string dependedon, string configFile)
        {
            CreateInstaller(displayName, serviceName, description, dependedon, configFile).Install(new Hashtable());
        }

        public static void Uninstall(string displayName, string serviceName, string description)
        {
            CreateInstaller(displayName, serviceName, description, null, null).Uninstall(null);
        }
    }
}
