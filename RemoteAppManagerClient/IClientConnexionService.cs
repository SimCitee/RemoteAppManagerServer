using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace RemoteAppManagerClient
{
    interface IClientConnexionService
    {
        void Start(IPAddress ipAddress);
    }
}
