using log4net.Appender;
using log4net.Repository.Hierarchy;
using syslog4net.Filter;
using syslog4net.Layout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CStarter.Samples.Common
{
    public class LogUtils
    {
        public static void PrepareLog(string logRoot, string serviceName)
        {
            Hierarchy hier = log4net.LogManager.GetRepository() as Hierarchy;

            string logRootFullName = Path.Combine(logRoot,
                serviceName, "Starter");

            if (null != hier)
            {
                RollingFileAppender commonAppender = (RollingFileAppender)hier.GetAppenders().Where(a => a.Name.Equals("CommonAppender")).FirstOrDefault();

                if (null != commonAppender)
                {
                    commonAppender.File = Path.Combine(logRootFullName, "logs.txt");

                    var filter = new LogExceptionToFileFilter()
                    {
                        ExceptionLogFolder = Path.Combine(logRootFullName, "Exceptions")
                    };
                    commonAppender.AddFilter(filter);
                    filter.ActivateOptions();

                    var layout = new SyslogLayout()
                    {
                        StructuredDataPrefix = "CStarterD@" + serviceName
                    };
                    commonAppender.Layout = layout;
                    layout.ActivateOptions();

                    commonAppender.ActivateOptions();
                }
            }
        }
    }
}
