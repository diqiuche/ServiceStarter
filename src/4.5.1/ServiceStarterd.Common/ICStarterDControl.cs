using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace CStarterD.Common
{
    [ServiceContract]
    public interface ICStarterDControl
    {
        [OperationContract]
        ActionResult StartService(string name);

        [OperationContract]
        ActionResult StopService(string name, int waitSecs);

        [OperationContract]
        ActionResult CheckService(string name);

        [OperationContract]
        ActionResult GetServiceNames();

        [OperationContract]
        ActionResult GetServiceInfo(string name);
    }
}
