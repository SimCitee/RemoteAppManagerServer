using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteAppManager.Core;

namespace RemoteAppManagerServer
{
    class ServerUtils
    {
        public static void DisplayMessage(String message) {
            String log = "[" + DateTime.Now.ToString() + "] " + message;
            Console.WriteLine(log);
        }
    }
}
