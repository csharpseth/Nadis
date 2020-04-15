using System;

namespace Nadis.Net.Foundation
{
    [Serializable]
    public class ServerData
    {
        public string displayName;
        public string remoteIP;
        public int port;

        public ServerData(string ip, int port)
        {
            remoteIP = ip;
            this.port = port;
        }
    }
}
