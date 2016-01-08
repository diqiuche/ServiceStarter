using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace CStarterD.Contracts
{
    [ServiceContract]
    public interface ICStarterDNotifier
    {
        [OperationContract]
        void StarterStartCompleted(string signal);

        [OperationContract]
        void StarterStopCompleted(string signal);
    }
}
