using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace CStarter.Contracts
{
    [ServiceContract]
    public interface ICStarterControl
    {
        [OperationContract]
        void Stop(string signal);
    }
}
