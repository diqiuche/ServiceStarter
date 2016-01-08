using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace CStarter.Utils
{
    public class EventWaitHandleHelper
    {
        public static bool Create(string name, EventResetMode mode, out EventWaitHandle signal)
        {
            var users = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
            var rule = new EventWaitHandleAccessRule(users, EventWaitHandleRights.Synchronize |
            EventWaitHandleRights.Modify, AccessControlType.Allow);
            var security = new EventWaitHandleSecurity();
            security.AddAccessRule(rule);

            bool created;
            signal = new EventWaitHandle(false, mode, @"Global\" + name, out created, security);

            return created;
        }

        public static bool OpenExisting(string name, out EventWaitHandle ewh)
        {
            bool retValue = false;

            ewh = null;

            try
            {
                ewh = EventWaitHandle.OpenExisting(@"Global\" + name, EventWaitHandleRights.Synchronize |
                    EventWaitHandleRights.Modify);

                retValue = true;
            }
            catch(Exception eX)
            {
                throw eX;
            }

            return retValue;
        }
    }
}
