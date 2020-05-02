using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using Nadis.Net;

namespace Nadis.Net.Client
{
    public class Client : INetworkID, IEventAccessor
    {
        public static Client Local { get; set; }
        public int NetID { get; private set; }
        public ClientTCP TCP { get; private set; }
        public ClientUDP UDP { get; private set; }
        public string IP { get { return _ip; } }

        private int _bufferSize;
        private string _ip = NetData.Default.IP;
        private int _port = NetData.Default.Port;
        private bool _idSet = false;
        
        public void ConnectToServer()
        {
            _bufferSize = NetData.Default.BufferSize;
            TCP = new ClientTCP();
            UDP = new ClientUDP(_ip, _port);

            ClientPacketHandler.Initialize();
            Local = this;
            Subscribe();

            TCP.Connect(_ip, _port);
        }

        public void Disconnect(IPacketData packet)
        {
            PacketDisconnectPlayer data = (PacketDisconnectPlayer)packet;
            if (data.playerID != NetID) return;
            TCP.Disconnect();
            UDP.Disconnect();
            Events.Player.UnSubscribe(NetID);
        }

        public void Disconnect()
        {
            PacketDisconnectPlayer packet = new PacketDisconnectPlayer
            {
                playerID = NetID
            };
            SendDataReliabe(NetID, packet);
            TCP.Disconnect();
            TCP = null;
            UDP.Disconnect();
            UDP = null;
            Events.Player.UnSubscribe(NetID);
        }

        public void SendDataReliabe(int clientToSendOn, IPacketData data)
        {
            if (NetID != clientToSendOn) return;
            TCP.TrySendData(data.Serialize().ToArray());
        }
        public void SendDataUnReliable(int clientToSendOn, IPacketData data)
        {
            if (NetID != clientToSendOn) return;
            UDP.SendData(data.Serialize().ToArray());
        }

        public void SetID(int id)
        {
            if (_idSet) return;
            NetID = id;
            _idSet = true;
        }
        
        public void Subscribe()
        {
            Events.Net.SendAsClient += SendDataReliabe;
            Events.Net.SendAsClientUnreliable += SendDataUnReliable;
            Events.Net.DisconnectClient += Disconnect;
            ClientPacketHandler.SubscribeTo((int)SharedPacket.PlayerDisconnected, Disconnect);

            Events.Player.UnSubscribe += UnSubscribe;
        }

        public void UnSubscribe(int netID)
        {
            Events.Net.SendAsClient -= SendDataReliabe;
            Events.Net.SendAsClientUnreliable -= SendDataUnReliable;

            Events.Net.DisconnectClient -= Disconnect;
            ClientPacketHandler.UnSubscribeFrom((int)SharedPacket.PlayerDisconnected, Disconnect);

            Events.Player.UnSubscribe -= UnSubscribe;
        }
    }
}
