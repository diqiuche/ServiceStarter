using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using CStarter.Utils;
using CStarter.Contracts;

namespace CStarterD
{
    public class CStarterClient : NamedPipeClient<ICStarterControl>
    {
        public void Stop(string domain, string name, string signal)
        {
            EndpointAddress ep = new EndpointAddress(string.Format("net.pipe://localhost/{0}/{1}/comm", domain,
                name));

            ExecuteNamedPipeAction(ep, (channel) =>
            {
                channel.Stop(signal);
            },
            3,
            (msg) =>
            {
                msg.Error();
            });
        }
    }
}
