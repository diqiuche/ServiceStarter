using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceStarter.Common
{
    public abstract class BaseStarter : MarshalByRefObject, IService
    {
        private static object _Locker = new object();

        private bool _IsRunning = false;

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void Start()
        {
            //if (!_IsRunning)
            //{
            //    lock(_Locker)
            //    {
            //        if (!_IsRunning)
            //        {
                        StartService();

                        _IsRunning = true;
            //        }
            //    }
            //}
        }

        public abstract void StartService();

        public void Stop()
        {
            //if (_IsRunning)
            //{
            //    lock(_Locker)
            //    {
                    if(_IsRunning)
                    {
                        StopService();
                        _IsRunning = false;
                    }
            //    }
            //}
        }

        public abstract void StopService();

        public void Dispose()
        {
            if(_IsRunning)
            {
                Stop();
            }
        }
    }
}
