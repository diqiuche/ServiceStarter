using CStarter.OptionsSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CStarterD
{
    public class ServiceStarterParams
    {
        public bool IsShowVersion { get; private set; }

        public string ConfigurationFileName { get; private set; }

        public bool IsShowHelp { get; private set; }

        public OptionSet Options { get; private set; }

        public List<string> Parse(string[] args)
        {
            Options = new OptionSet(){
                { "v|version", "显示版本号", v => IsShowVersion = true},
                { "c|config=", "使用指定的配置文件", v => { ConfigurationFileName = v;} },
                { "h|help", "显示帮助", v=>IsShowHelp=true}
            };

            return Options.Parse(args);
        }
    }
}
