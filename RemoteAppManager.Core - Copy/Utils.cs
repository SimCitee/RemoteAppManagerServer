using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RemoteAppManager.Core
{
    public class Utils {
        public enum LogLevels { 
            INFO = 1,
            WARNING = 2,
            ERROR = 3
        }

        public static void Log(LogLevels logLevel, String message) {
            String log = String.Empty;

            switch (logLevel) {
                case LogLevels.INFO: log += "[Info]"; break;
                case LogLevels.WARNING: log += "[Warning]"; break;
                case LogLevels.ERROR: log += "[Error]"; break;
                default: log += "[Info]"; break;
            }

            log += " [" + DateTime.Now.ToString() + "] " + message;

            Debug.WriteLine(log);
        }
    }
}
