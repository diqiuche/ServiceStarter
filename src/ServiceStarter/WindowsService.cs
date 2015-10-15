using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace ServiceStarter
{
    public class WindowsService : ServiceBase
    {
        protected override void OnStart(string[] args)
        {
            BasicServiceStarter.Run(new string[] { "" });
        }

        protected override void OnStop()
        {
            ServiceControllerDaemon.Current.Stop();

            if (0 != ServiceContext.Current.ServiceSlots.Count)
            {
                ServiceSlot[] slots = new ServiceSlot[ServiceContext.Current.ServiceSlots.Count];

                ServiceContext.Current.ServiceSlots.CopyTo(slots);

                foreach (ServiceSlot slot in slots)
                {
                    EventWaitHandle doneSignal = new EventWaitHandle(false, EventResetMode.ManualReset, slot.Signal);
                    doneSignal.Set();
                }
            }
        }
    }
}
