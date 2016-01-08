using CStarter.SDK;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CStarter.Utils;

namespace CStarter
{
    internal class ServiceContext
    {
        private static object _Locker = new object();

        private static ServiceContext _Instance = null;

        public static ServiceContext Current
        {
            get
            {
                if (null == _Instance)
                {
                    lock (_Locker)
                    {
                        if (null == _Instance)
                        {
                            _Instance = new ServiceContext();
                        }
                    }
                }

                return _Instance;
            }
        }

        public AppDomain ServiceDomain { get; set; }

        public Sponsor<IService> Service { get; set; }

        public string Name { get; set; }

        public string ContentPath { get; set; }

        public string AssemblyName { get; set; }

        public string TargetType { get; set; }

        public string Signal { get; set; }

        public string Domain { get; set; }

        private SemaphoreSlim _StopSemaphore;

        private ServiceContext()
        {
            _StopSemaphore = new SemaphoreSlim(0);
        }

        public void WaitForStopSemaphore()
        {
            _StopSemaphore.Wait();
        }

        public void Stop()
        {
            _StopSemaphore.Release();
        }

        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            e.Data.Info();
        }

        void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            e.Data.Error();
        }

        void p_Exited(object sender, EventArgs e)
        {
            
        }
    }
}
