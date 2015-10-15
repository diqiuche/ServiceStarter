using ServiceStarter.Common;
using ServiceStarter.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStarter
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ServiceControllerService : IServiceController
    {
        private int _LockCount = 0;
        private int _StartLockCount = 0;

        public ActionResult StartService(string name)
        {
            ActionResult retValue = new ActionResult(){
                Result = -1
            };

            if(1 == _StartLockCount)
            {
                retValue.Message = "正在启动一个服务，请稍后再试";
                return retValue;
            }

            Interlocked.Increment(ref _StartLockCount);

            "尝试启动服务：{0}".Formate(name).Info();

            ServiceSlot slot = ServiceContext.Current.ServiceSlots.FirstOrDefault(s => s.Name == name);

            if (null != slot)
            {
                Interlocked.Decrement(ref _StartLockCount);
                retValue.Message = "服务：{0} 已经启动".Formate(name);
                retValue.Message.Info();
            }
            else
            {
                string msg = "";
                if (!BasicServiceStarter.RunServiceProcess(name, out msg))
                {
                    retValue.Message = msg;
                    msg.Error();
                }
                else
                {
                    retValue.Result = 0;
                }

                Interlocked.Decrement(ref _StartLockCount);
            }

            return retValue;
        }

        public ActionResult StopService(string name)
        {
            ActionResult retValue = new ActionResult()
            {
                Result = -1
            };

            if (1 == _LockCount)
            {
                retValue.Message = "正在停止另一个服务，请稍后再试";
                return retValue;
            }

            Interlocked.Increment(ref _LockCount);

            "尝试停止服务：{0}".Formate(name).Info();

            ServiceSlot slot = ServiceContext.Current.ServiceSlots.FirstOrDefault(s => s.Name == name);

            if (null == slot)
            {
                Interlocked.Decrement(ref _LockCount);
                retValue.Message = "服务：{0}没有启动".Formate(name);
                retValue.Message.Info();
            }
            else
            {
                Task t = new Task(() =>
                {
                    EventWaitHandle doneSignal = new EventWaitHandle(false, EventResetMode.ManualReset, slot.Signal);
                    doneSignal.Set();

                    EventWaitHandle waitExitSignal = new EventWaitHandle(false, EventResetMode.ManualReset, "exit_" + slot.Signal);
                    bool result = waitExitSignal.WaitOne(10 * 1000);

                    if (result)
                    {
                        Interlocked.Decrement(ref _LockCount);

                        ServiceContext.Current.RemoveSlot(slot.WorkProcess.Id);

                        EventWaitHandle stopWaitSignal = new EventWaitHandle(false, EventResetMode.ManualReset, "stopservice_" + slot.Signal);
                        stopWaitSignal.Set();
                    }
                    else
                    {
                        Interlocked.Decrement(ref _LockCount);

                        if(slot.WorkProcess.HasExited)
                        {
                            ServiceContext.Current.RemoveSlot(slot.WorkProcess.Id);
                        }

                        EventWaitHandle stopWaitSignal = new EventWaitHandle(false, EventResetMode.ManualReset, "stopservice_" + slot.Signal);
                        stopWaitSignal.Set();
                    }
                });

                t.Start();

                retValue.Result = 0;
                retValue.Data = slot.Signal;
            }

            return retValue;
        }

        public ActionResult CheckService(string name)
        {
            ActionResult retValue = new ActionResult()
            {
                Result = -1
            };

            ServiceSlot slot = ServiceContext.Current.ServiceSlots.FirstOrDefault(s => s.Name == name);

            if (null != slot)
                retValue.Result = 1;
            else
                retValue.Result = 0;

            return retValue;
        }

        public ActionResult GetServiceNames()
        {
            ActionResult retValue = new ActionResult()
            {
                Result = -1
            };

            try
            {
                string retData = "";

                foreach (ServiceSlot slot in ServiceContext.Current.ServiceSlots)
                {
                    retData += string.Format("名称：{0}\n进程编号：{1}\n--------------------\n",
                        slot.Name,
                        slot.WorkProcess.Id);
                }

                retValue.Result = 0;
                retValue.Data = retData;
            }
            catch(Exception eX)
            {
                retValue.Message = eX.Message;
                eX.Exception();
            }

            return retValue;
        }

        public ActionResult GetServiceInfo(string name)
        {
            ActionResult retValue = new ActionResult()
            {
                Result = -1
            };

            try
            {
                string retData = "";

                ServiceSlot slot = ServiceContext.Current.ServiceSlots.FirstOrDefault(s => s.Name == name);

                double f = 1024.0;

                if (null != slot)
                {
                    retData = string.Format(string.Join("\n",
                        "未分页内存：{0:#,##0}",
                        "分页内存：{1:#,##0}",
                        "分页系统内存：{2:#,##0}",
                        "虚拟分页内存最大内存量：{3:#,##0}",
                        "最大虚拟内存：{4:#,##0}",
                        "最大物理内存：{5:#,##0}",
                        "专用内存：{6:#,##0}",
                        "总处理器时间：{7}",
                        "用户处理器时间：{8}",
                        "已分配虚拟内存：{9:#,##0}",
                        "已分配物理内存：{10:#,##0}"),
                        slot.WorkProcess.NonpagedSystemMemorySize64 / f,
                        slot.WorkProcess.PagedMemorySize64 / f,
                        slot.WorkProcess.PagedSystemMemorySize64 / f,
                        slot.WorkProcess.PeakPagedMemorySize64 / f,
                        slot.WorkProcess.PeakVirtualMemorySize64 / f,
                        slot.WorkProcess.PeakWorkingSet64 / f,
                        slot.WorkProcess.PrivateMemorySize64 / f,
                        slot.WorkProcess.TotalProcessorTime.TotalHours,
                        slot.WorkProcess.UserProcessorTime.TotalHours,
                        slot.WorkProcess.VirtualMemorySize64 / f,
                        slot.WorkProcess.WorkingSet64 / f);
                }

                retValue.Result = 0;
                retValue.Data = retData;
            }
            catch(Exception eX)
            {
                retValue.Message = eX.Message;
                eX.Exception();
            }

            return retValue;
        }
    }
}
