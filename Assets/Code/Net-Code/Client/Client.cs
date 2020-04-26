using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

namespace Nadis.Net.Client
{
    public class Client : INetworkID
    {
        public int NetID { get; private set; }
        public ClientTCP TCP { get; private set; }

        private int _bufferSize;
        private string _ip = NetData.Default.IP;
        private int _port = NetData.Default.Port;

        public Client()
        {
            _bufferSize = NetData.Default.BufferSize;
            TCP = new ClientTCP();
        }

        public void ConnectToServer()
        {
            ClientPacketHandler.Initialize();
            ClientPacketHandler.SubscribeTo((int)ServerPacketID.WelcomeMessage, (IPacketData data) =>
            {
                PacketWelcomeMessage msg = (PacketWelcomeMessage)data;
                Debug.Log(msg.message);
            });
            TCP.Connect(_ip, _port);
        }
    }
}
