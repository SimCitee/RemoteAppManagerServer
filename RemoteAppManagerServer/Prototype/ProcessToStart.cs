using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAppManagerServer.Prototype
{
    class ProcessToStart
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string FileName { get; set; }
        
        public ProcessToStart(int id, string name, string fileName) {
            ID = id;
            Name = name;
            FileName = fileName;
        }
    }
}
