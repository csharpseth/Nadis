using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace Nadis.Net.Server
{
    public static class ClientManager
    {
        public static List<int> Clients { get { return _clients; } }

        private static Dictionary<int, ServerClientData> _clientDictionary;
        private static Dictionary<int, HealthData> _clientHealthDictionary;
        private static List<int> _clients;
        private static int _maxClients;
        private static int _currentID = 0;
        private static bool initialized = false;

        public static void Init(int maxClients = 15)
        {
            if (initialized) return;

            _maxClients = maxClients;
            _clientDictionary = new Dictionary<int, ServerClientData>();
            _clientHealthDictionary = new Dictionary<int, HealthData>();
            _clients = new List<int>();
            initialized = true;
        }

        public static bool ClientExists(int clientID)
        {
            return _clientDictionary.ContainsKey(clientID);
        }

        public static ServerClientData GetClient(int clientID)
        {
            if (ClientExists(clientID) == false) return null;
            return _clientDictionary[clientID];
        }
        public static void DisconnectClient(int clientID, bool send = false)
        {
            if (ClientExists(clientID) == false) return;
            ServerClientData client = GetClient(clientID);
            client.Disconnect();
            int index = -1;
            for (int i = 0; i < _clients.Count; i++)
            {
                if (_clients[i] == clientID) index = i;
            }
            //Send a Disconnect Command to all Clients
            if(send)
            {
                PacketDisconnectPlayer packet = new PacketDisconnectPlayer
                {
                    playerID = clientID
                };
                ServerSend.ReliableToAll(packet, clientID);
            }

            _clients.RemoveAt(index);
            _clientDictionary.Remove(clientID);
            _clientHealthDictionary.Remove(clientID);
        }

        private static int NextID()
        {
            _currentID++;
            return _currentID;
        }

        public static int TryAddClient(TcpClient socket)
        {
            int clientID = NextID();

            if (ClientExists(clientID))
            {
                Debug.LogErrorFormat("Failed To Add Client: ID:{0} is Already In Use", clientID);
                return -1;
            }

            try
            {
                ServerClientData cd = new ServerClientData(socket, clientID);
                _clientDictionary.Add(clientID, cd);
                _clients.Add(clientID);
                ItemManager.CreateInventory(clientID);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return -1;
            }
            
            return clientID;
        }
        public static HealthData CreateOrGetClientHealthData(int clientID, int startHealth, int maxHealth)
        {
            if (_clientHealthDictionary.ContainsKey(clientID) == true) return _clientHealthDictionary[clientID];
            if (_clientDictionary.ContainsKey(clientID) == false) return default; 

            HealthData health = new HealthData(clientID, ServerData.PlayerStartHealth, ServerData.PlayerMaxHealth);
            _clientHealthDictionary.Add(clientID, health);

            return health;
        }
        public static bool TryDamagePlayer(int playerID, int amount)
        {
            if (_clientDictionary.ContainsKey(playerID) == false) return false;
            if (_clientHealthDictionary.ContainsKey(playerID) == false) return false;

            int damage = -amount;
            HealthData health = _clientHealthDictionary[playerID];
            health.AlterValue(damage);

            if (health.Dead)
            {
                ItemManager.PlayerDropAllItems(playerID);
                PacketKillPlayer packet = new PacketKillPlayer{
                    playerID = playerID,
                    respawn = true
                };
                ServerSend.ReliableToAll(packet);
                health.Reset();
                 _clientHealthDictionary[playerID] = health;
                return false;
            }

            _clientHealthDictionary[playerID] = health;
            return true;
        }

        public static void RemoveClient(int clientID)
        {
            if (ClientExists(clientID) == false)
            {
                Debug.LogErrorFormat("Failed To Remove Client ID:{0}, No Such Client Exists.");
                return;
            }

            try
            {
                _clientDictionary.Remove(clientID);
                for (int i = 0; i < _clients.Count; i++)
                {
                    if (_clients[i] != clientID) continue;

                    _clients.Remove(_clients[i]);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        
        public static void Clear(bool send)
        {
            for (int i = 0; i < _clients.Count; i++)
            {
                int id = _clients[i];
                DisconnectClient(id, send);
            }
        }
    }
}
