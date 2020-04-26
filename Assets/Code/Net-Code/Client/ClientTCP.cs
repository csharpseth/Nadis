using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

namespace Nadis.Net.Client
{
    public class ClientTCP
    {
        public TcpClient socket;
        private NetworkStream _stream;
        private byte[] _receiveBuffer;
        private PacketBuffer _receivedPacket;

        public void Connect(string ip, int port)
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = NetData.Default.BufferSize,
                SendBufferSize = NetData.Default.BufferSize
            };

            _receiveBuffer = new byte[NetData.Default.BufferSize];
            _receivedPacket = new PacketBuffer();
            socket.BeginConnect(ip, port, ConnectionCallback, null);
        }

        private void ConnectionCallback(IAsyncResult ar)
        {
            socket.EndConnect(ar);
            if(socket.Connected == false)
            {
                Debug.Log("Failed To Connect To Server!");
                return;
            }
            
            _stream = socket.GetStream();
            BeginRead();
            Debug.Log("Connection To Server was Successfull");
        }

        private void BeginRead()
        {
            _stream.BeginRead(_receiveBuffer, 0, NetData.Default.BufferSize, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int size = _stream.EndRead(ar);
                if (size <= 0)
                {
                    //Disconnect
                    return;
                }

                byte[] data = new byte[size];
                Array.Copy(_receiveBuffer, data, size);

                //Handle Data
                _receivedPacket.Reset(HandleData(data));

                BeginRead();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                //Disconnect
            }
        }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;
            _receivedPacket.SetBytes(data);

            if(_receivedPacket.UnreadLength() >= 4)
            {
                packetLength = _receivedPacket.ReadInt();
                if (packetLength <= 0)
                    return true;
            }

            while(packetLength > 0 && packetLength <= _receivedPacket.UnreadLength())
            {
                byte[] packetBytes = _receivedPacket.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    PacketBuffer buffer = new PacketBuffer(packetBytes);
                    int packetID = buffer.ReadInt();
                    ClientPacketHandler.Handle(packetID, buffer);
                    buffer.Dispose();
                });

                if (_receivedPacket.UnreadLength() >= 4)
                {
                    packetLength = _receivedPacket.ReadInt();
                    if (packetLength <= 0)
                        return true;
                }
            }

            if (packetLength <= 1)
                return true;

            return false;
        }



    }
}
