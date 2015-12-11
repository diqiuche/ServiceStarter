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

                    EventWaitHandle waitExitSignal = new EventWaitHandle(false, EventResetMode.ManualReset, "exit_" + slot.Signal);
                    if (waitExitSignal.WaitOne(10 * 1000))
                    {
                        if (!slot.WorkProcess.WaitForExit(10 * 1000))
                        {
                            slot.WorkProcess.Kill();

                            ServiceSlot tSlot = ServiceContext.Current.ServiceSlots.FirstOrDefault(s => s.Name == slot.Name);

                            if (null != tSlot)
                            {
                                ServiceContext.Current.ServiceSlots.Remove(tSlot);
                            }
                        }
                    }
                }
            }
        }
    }
}
