using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darwin_Server
{
    public static class ClientManager
    {
        private static Dictionary<int, ClientData> clients;
        public static int ClientCount { get { return clients.Count; } }
        public static int InventorySize = 7;

        public static Action OnClientCountChange;

        public static void Init(int inventorySize = 7)
        {
            clients = new Dictionary<int, ClientData>();
            InventorySize = inventorySize;
        }

        public static bool AddClient(int connID, ClientData clientData)
        {
            if(clients.ContainsKey(connID))
            {
                Console.WriteLine("Failed To Add New Client. Connection ID Already Exists.");
                return false;
            }

            clients.Add(connID, clientData);
            NetworkSend.PlayerConnected(connID, InventorySize);

            int currID = 0;
            int iterated = 0;
            while(iterated < clients.Count)
            {
                if(clients.ContainsKey(currID))
                {
                    iterated++;
                    if (currID != connID)
                    {
                        NetworkSend.PlayerConnected(currID, InventorySize, connID);
                    }
                }

                currID++;
            }

            OnClientCountChange?.Invoke();
            return true;
        }
        public static void RemoveClient(int connID)
        {
            if (clients.ContainsKey(connID) == false)
                return;

            clients.Remove(connID);

            NetworkSend.PlayerDisconnected(connID);
            OnClientCountChange?.Invoke();
        }
        public static ClientData GetClient(int connID)
        {
            if(clients.ContainsKey(connID) == false)
            {
                Console.WriteLine("Failed To Fetch Client With ID[ " + connID + " ]");
                return null;
            }

            return clients[connID];
        }
        public static void UpdateClientPosition(int connID, float x, float y, float z)
        {
            ClientData cd = GetClient(connID);
            if (cd == null)
                return;

            cd.SetPosition(x, y, z);

        }

        public static void UpdateClientRotation(int connID, float x, float y, float z)
        {
            ClientData cd = GetClient(connID);
            if (cd == null)
                return;

            cd.SetRotation(x, y, z);

        }

        public static void UpdateClientMoveData(int connID, bool grounded, float x, float y, int moveState)
        {
            ClientData cd = GetClient(connID);
            if (cd == null)
                return;

            cd.SetMoveData(grounded, x, y, moveState);
        }

        public static void UpdateClientInventory(int connID, int[] inventory)
        {
            ClientData cd = GetClient(connID);
            if (cd == null)
                return;

            clients[connID].SetInventory(inventory);
        }
    }
}
