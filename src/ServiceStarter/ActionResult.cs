using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ServiceStarter
{
    [DataContract]
    public class ActionResult
    {
        [DataMember]
        public int Result { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public object Data { get; set; }
    }
}
