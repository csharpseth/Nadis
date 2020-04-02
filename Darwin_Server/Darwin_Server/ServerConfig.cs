using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darwin_Server
{
    public struct ServerConfig
    {
        public string DisplayName { get; private set; }
        public int Port { get; private set; }
        public int PlayerQueueLimit { get; private set; }
        public int PlayerIDStartIndex { get; private set; }

        public bool DataSet { get; private set; }

        public ServerConfig(string name, int port, int queueLimit, int idStart)
        {
            DisplayName = name;
            Port = port;
            PlayerQueueLimit = queueLimit;
            PlayerIDStartIndex = idStart;

            DataSet = true;
        }

    }
}
