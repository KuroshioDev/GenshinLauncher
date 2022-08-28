using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PU_Test.Model
{
    partial class LauncherConfig:ObservableObject
    {
        [ObservableProperty]
        private GameInfo gameInfo;
        [ObservableProperty]
        private ProxyConfig proxyConfig;
    }
}
