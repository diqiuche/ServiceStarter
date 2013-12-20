using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStarter.Common
{
    public interface IService : IDisposable
    {
        void Start();
    }
}
