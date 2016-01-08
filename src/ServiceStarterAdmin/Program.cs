using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CStarter.Utils;
using CStarter.OptionsSharp;
using CStarter.Configuration;
using System.IO;
using System.Configuration;

namespace CStarterAdmin
{
    class Program
    {
        static Configuration _Config;

        static ServiceStarterSection _SrcConfig;

        static string _ConfigFile;

        static void Version()
        {
            "服务启动通用程序，版本号：1.0".Info(false);
        }

        static void ShowHelp(OptionSet p)
        {
            "用法：CStarterAdmin [Options]+".Info(false);
            "对服务做出相应的操作指令".Info(false);

            p.WriteOptionDescriptions(Console.Out);
        }

        static void Install()
        {
            string.Format("安装服务：{0}:{1}", _SrcConfig.ServiceInfo.DisplayName,
                        _SrcConfig.ServiceInfo.ServiceName).Info();

            try
            {
                BasicServiceInstaller.Install(_SrcConfig.ServiceInfo.DisplayName,
                    _SrcConfig.ServiceInfo.ServiceName,
                    _SrcConfig.ServiceInfo.Description,
                    _SrcConfig.ServiceInfo.DependedOn,
                    _ConfigFile);
            }
            catch (Exception eX)
            {
                eX.Exception();
            }
        }

        static void UnInstall()
        {
            string.Format("卸载服务：{0}:{1}", _SrcConfig.ServiceInfo.DisplayName,
                        _SrcConfig.ServiceInfo.ServiceName).Info();
            try
            {
                BasicServiceInstaller.Uninstall(_SrcConfig.ServiceInfo.DisplayName,
                    _SrcConfig.ServiceInfo.ServiceName,
                    _SrcConfig.ServiceInfo.Description);
            }
            catch (Exception eX)
            {
                eX.Exception();
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            bool isShowHelp = false;
            bool isInstall = false;
            bool isUninstall = false;
            bool isStop = false;
            bool isRun = false;
            bool isShowVersion = false;
            bool isListServices = false;
            bool isShowServiceInfo = false;

            string restartName = string.Empty;
            string targetService = string.Empty;

            var p = new OptionSet(){
                { "i|install", "安装服务", v => isInstall = true},
                { "u|uninstall", "卸载服务", v => isUninstall = true},
                { "v|version", "显示版本号", v => isShowVersion = true},
                { "o|stop=", "停止指定的服务", v => { isStop = true; restartName = v; } },
                { "r|run=", "运行指定的服务", v => { isRun = true; restartName = v; } },
                { "l|list", "列出服务列表", v => isListServices = true },
                { "n|info=", "列出服务信息", v => { targetService = v; isShowServiceInfo = true; } },
                { "s|service=", "运行指定名称的服务（该服务必须已经配置在配置文件中）", v => targetService = v },
                { "c|config=", "要使用的配置文件，默认是CStarterD.exe.config，可以与i配合使用", v => _ConfigFile = v},
                { "h|help", "显示帮助", v=>isShowHelp=true}
            };

            List<string> extra;

            try
            {
                extra = p.Parse(args);

                if (string.IsNullOrEmpty(_ConfigFile))
                {
                    _ConfigFile = "CStarterD.exe.config";
                }

                string fileFullName = _ConfigFile;

                if (!Path.IsPathRooted(fileFullName))
                {
                    fileFullName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _ConfigFile);
                }

                ExeConfigurationFileMap map = new ExeConfigurationFileMap();
                map.ExeConfigFilename = fileFullName;
                _Config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

                _SrcConfig = (ServiceStarterSection)_Config.GetSection("serviceStarters");

                if (isShowHelp)
                {
                    ShowHelp(p);
                    (string.Join(",", extra.ToArray())).Info();
                    return;
                }
                else if (isShowVersion)
                {
                    Version();
                }
                else if (isInstall)
                {
                    Install();
                }
                else if (isUninstall)
                {
                    UnInstall();
                }
                else if (isListServices)
                {
                    string names = (new CStarterDControlClient()).GetServices(_SrcConfig);

                    names.Info(false);
                }
                else if (isShowServiceInfo)
                {
                    string info = (new CStarterDControlClient()).GetServiceInfo(_SrcConfig, targetService);

                    info.Info(false);
                }
                else if (isStop)
                {
                    if (!string.IsNullOrEmpty(restartName))
                    {
                        "确定要停止服务：{0}？(Y/N)".Formate(restartName).Info();
                        string c = Console.ReadLine();

                        if ("Y" == c.ToUpper())
                        {
                            (new CStarterDControlClient()).StopService(_SrcConfig, restartName);
                        }
                    }
                }
                else if (isRun)
                {
                    if (!string.IsNullOrEmpty(restartName))
                    {
                        (new CStarterDControlClient()).StartService(_SrcConfig, restartName);
                    }
                }
                else
                {
                    ShowHelp(p);
                }
            }
            catch (OptionException eX)
            {
                "CStarterAdmin:".Error();
                eX.Message.Error();
                eX.Exception();
                "使用命令CStarterAdmin --help获取更多命令帮助".Info();
            }
        }
    }
}
