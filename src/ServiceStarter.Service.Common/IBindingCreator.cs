using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStarter.Service.Common
{
    public interface IBindingCreator
    {
        Binding CreateBinding();
    }
}
