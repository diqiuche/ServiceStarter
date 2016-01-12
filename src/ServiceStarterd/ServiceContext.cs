using CStarter.Configuration;
using CStarterD.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CStarter.Utils;
using System.Threading;

namespace CStarterD
{
    internal class ServiceContext
    {
        private static object _Locker = new object();

        private static ServiceContext _Instance = null;

        private List<ServiceSlot> _Slots;

        private SemaphoreSlim _StartSemaphore;

        private SemaphoreSlim _StopSemaphore;

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

        private ServiceContext()
        {
            _Slots = new List<ServiceSlot>();

            _StartSemaphore = new SemaphoreSlim(0);
            _StopSemaphore = new SemaphoreSlim(0);
        }

        public bool WaitServiceStarting(int secs)
        {
            return _StartSemaphore.Wait(secs * 1000);
        }

        public void ServiceStartedComplete()
        {
            _StartSemaphore.Release();
        }

        public void WaitServiceStopping()
        {
            _StopSemaphore.Wait();
        }

        public bool WaitServiceStopping(int waitSecs)
        {
            return _StopSemaphore.Wait(waitSecs * 1000);
        }

        public void ServiceStoppedComplete()
        {
            _StopSemaphore.Release();
        }

        public ServiceStarterSection Configuration
        {
            get;
            set;
        }

        public void AddSlot(ServiceStarterElement eleConfig, Process p, string signal)
        {
            ServiceSlot slot = new ServiceSlot()
            {
                Name = eleConfig.Name,
                Config = eleConfig,
                WorkProcess = p,
                Signal = signal
            };

            _Slots.Add(slot);
        }

        public List<ServiceSlot> ServiceSlots
        {
            get
            {
                return _Slots;
            }
        }

        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            e.Data.Info();
        }

        void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            e.Data.Error();
        }

        public void RemoveSlot(int pid)
        {
            if (0 != _Slots.Count)
            {
                lock (_Locker)
                {
                    if (0 != _Slots.Count)
                    {
                        ServiceSlot target = null;

                        foreach (ServiceSlot slot in _Slots)
                        {
                            if (slot.WorkProcess.Id == pid)
                            {
                                target = slot;
                                break;
                            }
                        }

                        if (null != target)
                        {
                            string.Format("{0} 已经退出", target.Name);
                            _Slots.Remove(target);
                        }
                    }
                }
            }
        }

        public ServiceSlot[] GetServiceSlots()
        {
            ServiceSlot[] retValue = new ServiceSlot[0];

            if(0 != _Slots.Count)
            {
                lock(_Locker)
                {
                    if (0 != _Slots.Count)
                    {
                        retValue = (from slot in _Slots
                                    select slot).ToArray();
                    }
                }
            }

            return retValue;
        }
    }
}
