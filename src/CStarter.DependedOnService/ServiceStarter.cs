using CStarter.Samples.Common;
using CStarter.SDK;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CStarter.DependedOnService
{
    public class ServiceStarter : BaseStarter
    {
        private ILog _Logger;

        public override void StartService()
        {
            log4net.Config.XmlConfigurator.Configure();

            LogUtils.PrepareLog(LogRoot, "DependedOnService");

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
