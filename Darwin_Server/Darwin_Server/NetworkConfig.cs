using System;
using KaymakNetwork.Network;

namespace Darwin_Server
{
    internal static class NetworkConfig
    {
        private static Server _socket;
        private static ServerConfig _data;

        internal static Server Socket {
            get { return _socket; }
            set
            {
                if(_socket != null)
                {
                    _socket.ConnectionReceived -= Socket_ConnectionReceived;
                    _socket.ConnectionLost -= Socket_ConnectionLost;
                }

                _socket = value;

                if(_socket != null)
                {
                    _socket.ConnectionReceived += Socket_ConnectionReceived;
                    _socket.ConnectionLost += Socket_ConnectionLost;
                }
            }
        }
        internal static ServerConfig Data
        {
            get
            {
                if (_data.DataSet == false)
                    _data = new ServerConfig("Default Server Name", 5555, 5, 1);

                return _data;
            }
        }

        internal static void InitNetwork()
        {
            if (!(Socket == null))
                return;

            Socket = new Server(100)
            {
                BufferLimit = 2048000,
                PacketAcceptLimit = 100,
                PacketDisconnectCount = 150
            };

            NetworkReceive.PacketRouter();
            Console.WriteLine("Initialized Packet Router Successfully");
            ClientManager.Init();
            Console.WriteLine("Initialized Client Manager Successfully");
            ItemManager.Initialize();
            Console.WriteLine("Initialized Item Manager Successfully");
            MapData.Generate();
            Console.WriteLine("Generated Map Data Successfully");

            Socket.StartListening(Data.Port, 5, 1);
        }

        internal static void Socket_ConnectionReceived(int connID)
        {
            Console.WriteLine("Player Has Connected & Given ID:" + connID);
            ClientManager.AddClient(connID, new ClientData(connID));
            NetworkSend.WelcomeMsg(connID);
        }

        internal static void Socket_ConnectionLost(int connID)
        {
            Console.WriteLine("Player With ID:" + connID + "Has Disconnected");
            ClientManager.RemoveClient(connID);
            NetworkSend.PlayerDisconnected(connID);
        }

    }
}
