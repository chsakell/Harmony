using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Client.Infrastructure.Configuration
{
    public class ClientConfiguration
    {
        public ClientConfiguration(string signalrHostUrl, string backendUrl)
        {
            SignalrHostUrl = signalrHostUrl;
            BackendUrl = backendUrl;
        }

        public string SignalrHostUrl {  get; private set; }
        public string BackendUrl { get; private set;}
    }
}
