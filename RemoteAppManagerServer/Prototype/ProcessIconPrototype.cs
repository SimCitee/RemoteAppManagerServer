using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAppManagerServer.Prototype
{
    class ProcessIconPrototype
    {
        public String FileName { get; set; }
        public int ProcessID { get; set; }

        public ProcessIconPrototype(int processID, String filename) {
            FileName = filename;
            ProcessID = processID;
        }
    }
}
