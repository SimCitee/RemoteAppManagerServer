using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteAppManagerClient.MVVM;

namespace RemoteAppManagerClient.Prototype
{
    class ProcessToStart
    {
        public int ID { get; set; }

        public string Name { get; set; }
        
        public ProcessToStart(int id, string name) {
            ID = id;
            Name = name;
        }
    }
}
