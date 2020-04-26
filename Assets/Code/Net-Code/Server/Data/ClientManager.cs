using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace Nadis.Net.Server
{
    public static class ClientManager
    {
        public static List<ServerClientData> Clients { get { return _clients; } }

        private static Dictionary<int, ServerClientData> _clientDictionary;
        private static List<ServerClientData> _clients;
        private static int _maxClients;
        private static int _currentID = 0;
        private static bool initialized = false;

        public static void Init(int maxClients = 15)
        {
            if (initialized) return;

            _maxClients = maxClients;
            _clientDictionary = new Dictionary<int, ServerClientData>();
            _clients = new List<ServerClientData>();
            initialized = true;
        }

        public static bool ClientExists(int clientID)
        {
            return _clientDictionary.ContainsKey(clientID);
        }

        public static ServerClientData GetClient(int clientID)
        {
            if (ClientExists(clientID) == false) return default;
            return _clientDictionary[clientID];
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
                _clients.Add(cd);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return -1;
            }
            
            return clientID;
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
                    if (_clients[i].NetID != clientID) continue;
                    _clients.Remove(_clients[i]);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

    }
}
