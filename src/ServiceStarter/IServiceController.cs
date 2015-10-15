using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace ServiceStarter
{
    [ServiceContract]
    public interface IServiceController
    {
        [OperationContract]
        ActionResult StartService(string name);

        [OperationContract]
        ActionResult StopService(string name);

        [OperationContract]
        ActionResult CheckService(string name);

        [OperationContract]
        ActionResult GetServiceNames();

        [OperationContract]
        ActionResult GetServiceInfo(string name);
    }
}
