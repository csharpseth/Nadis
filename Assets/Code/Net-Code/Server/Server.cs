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
            ServerPacketHandler.Initialize();

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
                //Send all ALREADY connected players to the Client
                int[] clients = ClientManager.Clients.ToArray();
                for (int i = 0; i < clients.Length; i++)
                {
                    int id = clients[i];
                    if (id == clientID) continue;
                    PacketPlayerConnection playerData = new PacketPlayerConnection
                    {
                        playerID = id,
                        playerIsLocal = false
                    };
                    ServerSend.ReliableToOne(playerData, clientID);
                }

                //Send THIS clients data to this client so they are sync'd with the server
                PacketPlayerConnection localClientData = new PacketPlayerConnection
                {
                    playerID = clientID,
                    playerIsLocal = true
                };
                ServerSend.ReliableToOne(localClientData, clientID);

                //TODO Send this clients data to all already connected players
                localClientData.playerIsLocal = false;
                ServerSend.ReliableToAll(localClientData, clientID);

                return;
            }

            Debug.Log("A Player Failed To Connect To The Server.");
        }
    }
}
