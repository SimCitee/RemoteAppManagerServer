using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAppManagerServer.Prototype
{
    class ProcessPrototype
    {
        public String FileName { get; set; }
        public string Name { get; set; }
        public int ProcessID { get; set; }

        public ProcessPrototype(int processID, String filename) {
            FileName = filename;
            ProcessID = processID;
        }

        public ProcessPrototype(int processID, string name, String filename)
        {
            FileName = filename;
            Name = name;
            ProcessID = processID;
        }
    }
}
