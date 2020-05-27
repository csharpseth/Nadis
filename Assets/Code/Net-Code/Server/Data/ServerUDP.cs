using System;
using System.Net;
using System.Net.Sockets;
using Nadis.Net;
using UnityEngine;

namespace Nadis.Net.Server
{
    public class ServerUDP
    {
        public UdpClient socket;
        public int Port { get; private set; }


        public ServerUDP(int port)
        {
            Port = port;
            socket = new UdpClient(Port);
        }
        
        public void Start()
        {
            BeginReceive();
            Log.Not("Server UDP Has Started.");
        }

        private void BeginReceive()
        {
            socket.BeginReceive(ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = socket.EndReceive(ar, ref endPoint);
                BeginReceive();

                if (data.Length < 4)
                {
                    //Disconnect
                    return;
                }

                HandleData(data, endPoint);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void HandleData(byte[] data, IPEndPoint endPoint)
        {
            try
            {
                using (PacketBuffer buffer = new PacketBuffer(data))
                {
                    int packetLength = buffer.ReadInt();
                    data = buffer.ReadBytes(packetLength);
                }

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (PacketBuffer buffer = new PacketBuffer(data))
                    {
                        int packetID = buffer.ReadInt();
                        int playerID = buffer.ReadInt(false);

                        ServerClientData client = ClientManager.GetClient(playerID);
                        if (client==null || client.UDP == null)
                        {
                            Log.Wrn("SERVER :: Client UDP is NULL");
                            return;
                        }

                        if (client.UDP.endPoint == null)
                        {
                            client.UDP.Connect(endPoint);
                            SendData(new PacketUDPConnected().Serialize().ToArray(), endPoint);
                            Log.Not("SERVER :: UDP Client Established.");
                            return;
                        }

                        if (client.UDP.endPoint.ToString() != endPoint.ToString())
                            return;

                        ServerPacketHandler.Handle(packetID, buffer);
                    }
                });
            }
            catch (Exception e)
            {
                Log.Err(e);
            }
        }

        public void SendData(byte[] data, IPEndPoint endPoint)
        {
            try
            {
                if (endPoint == null) return;

                socket.BeginSend(data, data.Length, endPoint, null, null);
            }
            catch (Exception e)
            {
                Log.Err(e);
            }
        }
    }
}