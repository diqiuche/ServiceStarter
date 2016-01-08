using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using CStarter.Utils;
using CStarterD.Contracts;

namespace CStarter
{
    public class CStarterDNotifyClient : NamedPipeClient<ICStarterDNotifier>
    {
        public void StartComplete(string domain, string signal)
        {
            EndpointAddress ep = new EndpointAddress(string.Format("net.pipe://localhost/{0}/{1}", domain,
                "comm"));

            ExecuteNamedPipeAction(ep, (channel) =>
            {
                channel.StarterStartCompleted(signal);
            }, (msg) =>
            {
            });
        }

        public void StopComplete(string domain, string signal)
        {
            EndpointAddress ep = new EndpointAddress(string.Format("net.pipe://localhost/{0}/{1}", domain,
                "comm"));

            ExecuteNamedPipeAction(ep, (channel) =>
            {
                channel.StarterStopCompleted(signal);
            }, (msg) =>
            {
            });
        }
    }
}
