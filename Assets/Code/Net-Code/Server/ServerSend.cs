using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Nadis.Net.Server
{
    public class ServerSend
    {
        /// <summary>
        /// Sends an IPacketData type to One Specific Client with "clientID".
        /// </summary>
        /// <param name="data"></param>
        /// <param name="clientID"></param>
        public static void ReliableToOne(IPacketData data, int clientID)
        {
            ServerClientData client = ClientManager.GetClient(clientID);
            if (client.Invalid) return;
            
            client.SendDataReliable(data.Serialize().ToArray());
        }

        //Hopefully improve with C# Jobs :D
        /// <summary>
        /// Sends an IPacketData to all connected, and valid, Clients.
        /// </summary>
        /// <param name="data"></param>
        public static void ReliableToAll(IPacketData data, int exceptionID = -1)
        {
            List<int> clients = ClientManager.Clients;
            if (clients == null || clients.Count == 0) return;

            bool except = (exceptionID != -1);
            for (int i = 0; i < clients.Count; i++)
            {
                if(except == true)
                {
                    if (clients[i] != exceptionID)
                        ReliableToOne(data, clients[i]);
                }
                else
                {
                    ReliableToOne(data, clients[i]);
                }
            }
        }

        public static void UnReliableToOne(IPacketData data, int clientID)
        {
            ServerClientData client = ClientManager.GetClient(clientID);
            if (client.Invalid) return;

            Server.instance.UDP.SendData(data.Serialize().ToArray(), client.UDP.endPoint);
        }

        public static void UnReliableToAll(IPacketData data, int exceptionID = -1)
        {
            List<int> clients = ClientManager.Clients;
            if (clients == null || clients.Count == 0) return;

            bool except = (exceptionID != -1);
            for (int i = 0; i < clients.Count; i++)
            {
                if (except == true)
                {
                    if (clients[i] != exceptionID)
                        UnReliableToOne(data, clients[i]);
                }
                else
                {
                    UnReliableToOne(data, clients[i]);
                }
            }
        }
    }
}
