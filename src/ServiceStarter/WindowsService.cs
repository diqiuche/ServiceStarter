using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

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
            if (0 != ServiceContext.Current.Domains.Count)
            {
                foreach (KeyValuePair<string, AppDomain> appPair in ServiceContext.Current.Domains)
                {
                    AppDomain.Unload(appPair.Value);
                }
            }

            base.OnStop();
        }
    }
}
