using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CStarter.SDK
{
    public abstract class BaseStarter : MarshalByRefObject, IService, IDisposable
    {
        private static object _Locker = new object();

        private bool _IsRunning = false;

        protected string LogRoot
        {
            get
            {
                return AppDomain.CurrentDomain.GetData("logRoot").ToString();
            }
        }

        //public override object InitializeLifetimeService()
        //{
        //    return null;
        //}

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

        public abstract void DisposeService();

        public void Dispose()
        {
            if(_IsRunning)
            {
                Stop();
            }

            DisposeService();
        }
    }
}
