using System;
using System.Net;
using System.Net.Sockets;
using Nadis.Net;
using UnityEngine;

namespace Nadis.Net.Server
{
    public class ServerTCP
    {
        public TcpListener socket;

        public ServerTCP(int port)
        {
            socket = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            socket.Start();
            BeginListenForClient();
            Log.Not("Server TCP Has Started.");
        }

        public void Stop()
        {
            socket.Stop();
        }

        private void BeginListenForClient()
        {
            socket.BeginAcceptTcpClient(new AsyncCallback(ClientConnectCallback), null);
        }

        private void ClientConnectCallback(IAsyncResult ar)
        {
            TcpClient client = socket.EndAcceptTcpClient(ar);
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

                    Vector3 position = ClientManager.GetClient(id).position;
                    float rotation = ClientManager.GetClient(clients[i]).rotation;

                    //Se
                    PacketPlayerConnection playerData = new PacketPlayerConnection
                    {
                        playerID = id,
                        playerIsLocal = false,
                        playerPosition = position,
                        playerRotation = rotation
                    };
                    ServerSend.ReliableToOne(playerData, clientID);

                    //Send Connecting Client other players Inventory
                    PacketPlayerInventoryData remInventoryData = new PacketPlayerInventoryData
                    {
                        playerID = id,
                        size = 7
                    };
                    ServerSend.ReliableToOne(remInventoryData, clientID);

                }

                //Send THIS clients data to this client so they are sync'd with the server
                PacketPlayerConnection localClientData = new PacketPlayerConnection
                {
                    playerID = clientID,
                    playerIsLocal = true,
                    playerPosition = Vector3.zero,
                    playerRotation = 0f
                };
                ServerSend.ReliableToOne(localClientData, clientID);
                localClientData.playerIsLocal = false;
                ServerSend.ReliableToAll(localClientData, clientID);

                PacketPlayerInventoryData inventoryData = new PacketPlayerInventoryData
                {
                    playerID = clientID,
                    size = 7
                };
                ServerSend.ReliableToOne(inventoryData, clientID);
                ServerSend.ReliableToAll(inventoryData, clientID);

                ItemManager.SendSceneItemsToPlayer(clientID);
                ItemManager.SendInventoryItemsToPlayer(clientID);

                return;
            }

            Log.Txt("A Player Failed To Connect To The Server.");
        }
    }
}