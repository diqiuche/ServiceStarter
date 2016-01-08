using CStarter.Contracts;
using CStarter.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace CStarter
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CStarterController : ICStarterControl
    {
        public void Stop(string signal)
        {
            if (signal == ServiceContext.Current.Signal)
            {
                ServiceContext.Current.Stop();
            }
        }
    }
}
