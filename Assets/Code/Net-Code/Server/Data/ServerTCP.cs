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
                SendClientExistingPlayersPrescence(clientID);
                SendClientExistingPlayersAdditionalData(clientID);
                SendClientConnectionDataToClient(clientID);
                InitialSyncClientInventory(clientID);
                ServerUnitController.SendPlayerUnits(clientID);
            }
        }

        private void SendClientExistingPlayersPrescence(int clientID)
        {
            int[] clients = ClientManager.Clients.ToArray();
            for (int i = 0; i < clients.Length; i++)
            {
                int id = clients[i];
                if (id == clientID) continue;

                Vector3 position = ClientManager.GetClient(id).position;
                float rotation = ClientManager.GetClient(clients[i]).rotation;
                ClientStatData playerStats = ClientManager.CreateOrGetClientStatData(id, ServerData.PlayerStartHealth, ServerData.PlayerMaxHealth, ServerData.PlayerStartPower, ServerData.PlayerMaxPower);
                //Se
                PacketPlayerConnection playerData = new PacketPlayerConnection
                {
                    playerID = id,
                    playerIsLocal = false,
                    playerPosition = position,
                    playerRotation = rotation,
                    currentHealth = playerStats.health,
                    maxHealth = playerStats.maxHealth,
                    currentPower = playerStats.power,
                    maxPower = playerStats.maxPower
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
        }

        private void SendClientExistingPlayersAdditionalData(int clientID)
        {
            int[] clients = ClientManager.Clients.ToArray();
            for (int i = 0; i < clients.Length; i++)
            {
                int id = clients[i];
                if (id == clientID) continue;
                //Send Connecting Client other players Inventory
                PacketPlayerInventoryData remInventoryData = new PacketPlayerInventoryData
                {
                    playerID = id,
                    size = 7
                };
                ServerSend.ReliableToOne(remInventoryData, clientID);

            }
        }

        private void SendClientConnectionDataToClient(int clientID)
        {
            ClientStatData localPlayerStats = ClientManager.CreateOrGetClientStatData(clientID, ServerData.PlayerStartHealth, ServerData.PlayerMaxHealth, ServerData.PlayerStartPower, ServerData.PlayerMaxPower);
            //Send THIS clients data to this client so they are sync'd with the server
            PacketPlayerConnection localClientData = new PacketPlayerConnection
            {
                playerID = clientID,
                playerIsLocal = true,
                playerPosition = Vector3.zero,
                playerRotation = 0f,
                currentHealth = localPlayerStats.health,
                maxHealth = localPlayerStats.maxHealth,
                currentPower = localPlayerStats.power,
                maxPower = localPlayerStats.maxPower
            };
            ServerSend.ReliableToOne(localClientData, clientID);

            //Send THIS client to all other players
            localClientData.playerIsLocal = false;
            ServerSend.ReliableToAll(localClientData, clientID);
        }

        private void InitialSyncClientInventory(int clientID, int inventorySize = 7)
        {
            PacketPlayerInventoryData inventoryData = new PacketPlayerInventoryData
            {
                playerID = clientID,
                size = inventorySize
            };
            ServerSend.ReliableToAll(inventoryData);

            ItemManager.SendSceneItemsToPlayer(clientID);
            ItemManager.SendInventoryItemsToPlayer(clientID);
        }
    }
}