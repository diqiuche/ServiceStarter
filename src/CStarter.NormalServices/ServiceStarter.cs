using CStarter.Samples.Common;
using CStarter.SDK;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using syslog4net.Filter;
using syslog4net.Layout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CStarter.NormalServices
{
    public class ServiceStarter : BaseStarter
    {
        private ILog _Logger;

        public override void StartService()
        {
            log4net.Config.XmlConfigurator.Configure();

            LogUtils.PrepareLog(LogRoot, "NormalServices");

            _Logger = LogManager.GetLogger(this.GetType());

            _Logger.Info("服务已经启动");
        }

        public override void StopService()
        {
            _Logger.Info("服务已经停止");
        }

        public override void DisposeService()
        {
            
        }
    }
}
