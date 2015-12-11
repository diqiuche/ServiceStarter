using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStarter.Common
{
    [Serializable]
    [SecurityPermission(SecurityAction.Demand, Infrastructure = true)]
    public sealed class Sponsor<T> : ISponsor, IDisposable where T : class
    {
        private T _TInstance;

        public bool IsDisposed { get; private set; }

        public T Instance
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(string.Format("{0}实例已经被释放", typeof(T).FullName));
                else
                    return _TInstance;
            }
            private set
            {
                _TInstance = value;
            }
        }

        public Sponsor(T instance)
        {
            Instance = instance;

            if(Instance is MarshalByRefObject)
            {
                object lifetimeService = RemotingServices.GetLifetimeService((MarshalByRefObject)(object)Instance);

                if(lifetimeService is ILease)
                {
                    ILease lease = (ILease)lifetimeService;
                    lease.Register(this);
                }
            }
        }

        ~Sponsor()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if(!IsDisposed)
            {
                if(disposing)
                {
                    if (Instance is IDisposable)
                    {
                        ((IDisposable)Instance).Dispose();
                    }

                    if(Instance is MarshalByRefObject)
                    {
                        object lifetimeService = RemotingServices.GetLifetimeService((MarshalByRefObject)(object)Instance);
                        if(lifetimeService is ILease)
                        {
                            ILease lease = (ILease)lifetimeService;
                            lease.Unregister(this);
                        }
                    }
                }

                Instance = null;
                IsDisposed = true;
            }
        }

        TimeSpan ISponsor.Renewal(ILease lease)
        {
            if (IsDisposed)
                return TimeSpan.Zero;
            else
                return LifetimeServices.RenewOnCallTime;
        }
    }
}
