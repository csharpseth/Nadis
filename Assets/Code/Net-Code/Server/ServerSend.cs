using System;
using System.Collections.Generic;
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

            PacketBuffer buffer = data.Serialize();
            client.SendData(buffer, true);
        }

        //Hopefully improve with C# Jobs :D
        /// <summary>
        /// Sends an IPacketData to all connected, and valid, Clients.
        /// </summary>
        /// <param name="data"></param>
        public static void ReliableToAll(IPacketData data, int exceptionID = -1)
        {
            List<ServerClientData> clients = ClientManager.Clients;
            if (clients == null || clients.Count == 0) return;

            bool except = (exceptionID != -1);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Invalid) continue;
                if(except == true)
                {
                    if (clients[i].NetID != exceptionID)
                        ReliableToOne(data, clients[i].NetID);
                }
                else
                {
                    ReliableToOne(data, clients[i].NetID);
                }
            }
        }
    }
}
