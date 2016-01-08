using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStarter.Common
{
    public static class EventLogExtesion
    {
        public static void WriteExceptionEvent(this Exception eX, int eventID)
        {
            if (!EventLog.SourceExists(EventLogConsts.Log_Source))
            {
                EventLog.CreateEventSource(EventLogConsts.Log_Source, EventLogConsts.Log_Name);
            }

            EventLog.WriteEntry(EventLogConsts.Log_Source,
                eX.Message,
                EventLogEntryType.Error,
                eventID,
                100,
                Encoding.UTF8.GetBytes(eX.StackTrace));
        }

        public static void WriteErrorEvent(this string info, int eventID)
        {
            if (!EventLog.SourceExists(EventLogConsts.Log_Source))
            {
                EventLog.CreateEventSource(EventLogConsts.Log_Source, EventLogConsts.Log_Name);
            }

            EventLog.WriteEntry(EventLogConsts.Log_Source,
                info, EventLogEntryType.Error, eventID);
        }
    }
}
