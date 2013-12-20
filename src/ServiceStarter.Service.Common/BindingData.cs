using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStarter.Service.Common
{
    [DataContract]
    public class BindingData
    {
        int MaxConnections { get; set; }

        long MaxReceivedMessageSize { get; set; }

        long MaxBufferPoolSize { get; set; }

        int MaxArrayLength { get; set; }

        int MaxBytesPerRead { get; set; }

        int MaxDepth { get; set; }

        int MaxStringContentLength { get; set; }

        TimeSpan OpenTimeout { get; set; }

        TimeSpan CloseTimeout { get; set; }

        TimeSpan ReceiveTimeout { get; set; }

        TimeSpan SendTimeout { get; set; }
    }
}
