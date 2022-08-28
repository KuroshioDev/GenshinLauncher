using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PU_Test.Model
{
    internal class ProxyConfig
    {
        public bool ProxyEnable = false;

        public ProxyConfig(bool proxyEnable, string proxyServer)
        {
            ProxyEnable = proxyEnable;
            ProxyServer = proxyServer;
        }

        public string ProxyServer { get; set; }

        public string ProxyPort { get; set; }

    }
}
