using CStarter.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CStarter.RestartOnErrorService
{
    public class ServiceStarter : BaseStarter
    {
        private Timer _RunTimer;

        private void ThrowException(object state)
        {
            throw new Exception("故意抛出的错误，尝试服务重新启动");
        }

        public override void StartService()
        {
            _RunTimer = new Timer(new TimerCallback(ThrowException));

            _RunTimer.Change(30 * 1000, Timeout.Infinite);
        }

        public override void StopService()
        {
            
        }

        public override void DisposeService()
        {
            
        }
    }
}
