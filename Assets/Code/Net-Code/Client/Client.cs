using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

namespace Nadis.Net.Client
{
    public class Client : INetworkID, IEventAccessor
    {
        public static Client Local { get; set; }
        public int NetID { get; private set; }
        public ClientTCP TCP { get; private set; }

        private int _bufferSize;
        private string _ip = NetData.Default.IP;
        private int _port = NetData.Default.Port;
        private bool _idSet = false;

        public Client()
        {
            _bufferSize = NetData.Default.BufferSize;
            TCP = new ClientTCP();
        }

        public void ConnectToServer()
        {
            ClientPacketHandler.Initialize();
            Local = this;
            Subscribe();

            TCP.Connect(_ip, _port);
        }

        public void SendData(int clientToSendOn, IPacketData data)
        {
            if (NetID != clientToSendOn) return;
            TCP.TrySendData(data.Serialize().ToArray());
        }

        public void SetID(int id)
        {
            if (_idSet) return;
            NetID = id;
            _idSet = true;
        }
        
        public void Subscribe()
        {
            Events.Net.SendAsClient += SendData;
        }

        public void UnSubscribe(int netID)
        {
            
        }
    }
}
