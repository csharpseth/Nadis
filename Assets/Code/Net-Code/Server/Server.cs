using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

namespace Nadis.Net.Server
{
    public class Server
    {
        public int MaxPlayers { get; private set; }
        public int Port { get; private set; }

        private TcpListener _socket;

        public void Start(int maxPlayers = NetData.Default.MaxPlayers, int port = NetData.Default.Port)
        {
            MaxPlayers = maxPlayers;
            Port = port;

            _socket = new TcpListener(IPAddress.Any, Port);
            ClientManager.Init(MaxPlayers);

            _socket.Start();
            BeginListenForClient();

            Debug.Log("Server Started Successfully");
        }

        private void BeginListenForClient()
        {
            _socket.BeginAcceptTcpClient(new AsyncCallback(ClientConnectCallback), null);
        }

        private void ClientConnectCallback(IAsyncResult ar)
        {
            TcpClient client = _socket.EndAcceptTcpClient(ar);
            BeginListenForClient();

            if (client == null)
                return;

            int clientID = ClientManager.TryAddClient(client);
            if (clientID != -1)
            {
                Debug.Log("A Player Has Successfully Connected To The Server.");
                PacketWelcomeMessage msg = new PacketWelcomeMessage
                {
                    message = "Hello from Seth H, Creator of Nadis",
                    clientID = clientID
                };

                ServerSend.ReliableToOne(msg, clientID);
                return;
            }

            Debug.Log("A Player Failed To Connect To The Server.");
        }
    }
}
