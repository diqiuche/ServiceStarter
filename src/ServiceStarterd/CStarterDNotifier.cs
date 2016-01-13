using CStarter.Utils;
using CStarterD.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace CStarterD
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CStarterDNotifier : ICStarterDNotifier
    {
        public void StarterStartCompleted(string signal)
        {
            "收到服务{0}启动完成指令".Formate(signal).Debug();
            ServiceContext.Current.ServiceStartedComplete();
        }

        public void StarterStopCompleted(string signal)
        {
            "收到服务{0}停止完成指令".Formate(signal).Debug();
            ServiceContext.Current.ServiceStoppedComplete();
        }
    }
}
