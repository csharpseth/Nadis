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
        private PacketBuffer _packetBuffer;
        public int LocalPort { get { return ((IPEndPoint)socket.Client.LocalEndPoint).Port; } }
        
        public void Connect(string ip, int port)
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = NetData.Default.BufferSize,
                SendBufferSize = NetData.Default.BufferSize
            };

            _receiveBuffer = new byte[NetData.Default.BufferSize];
            _packetBuffer = new PacketBuffer();
            socket.BeginConnect(ip, port, ConnectionCallback, null);
            Log.Event("Connecting To Server");
        }

        private void ConnectionCallback(IAsyncResult ar)
        {
            socket.EndConnect(ar);
            if(socket.Connected == false)
            {
                Log.Err("Failed To Connect To Server");
                return;
            }
            
            _stream = socket.GetStream();
            BeginRead();
            Log.Event("Connection To Server was Successfull");
        }

        public void Disconnect()
        {
            socket.Close();
            socket = null;
            _stream.Dispose();
            _stream = null;
            _receiveBuffer = null;
            _packetBuffer.Dispose();
        }

        private void BeginRead()
        {
            _stream.BeginRead(_receiveBuffer, 0, NetData.Default.BufferSize, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            if (_stream == null || socket == null)
            {
                if (_stream != null)
                    _stream.EndRead(ar);
                return;
            }

            if (_receiveBuffer == null) { Client.Local?.Disconnect(); return; }

            int size = _stream.EndRead(ar);
            if (size <= 0)
            {
                //Disconnect
                Client.Local?.Disconnect();
                return;
            }

            byte[] data = new byte[size];
            Array.Copy(_receiveBuffer, data, size);

            //Handle Data
            _packetBuffer.Reset(HandleData(data));

            BeginRead();
        }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;
            _packetBuffer.SetBytes(data);

            if(_packetBuffer.UnreadLength() >= 4)
            {
                packetLength = _packetBuffer.ReadInt();
                if (packetLength <= 0)
                    return true;
            }
            while(packetLength > 0 && packetLength <= _packetBuffer.UnreadLength())
            {
                byte[] packetBytes = _packetBuffer.ReadBytes(packetLength);
                //NativeArray<byte> packetBytes = new NativeArray<byte>(_receivedPacket.ReadBytes(packetLength), Allocator.TempJob);
                //Try to do this with the job system instead
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (PacketBuffer buffer = new PacketBuffer(packetBytes))
                    {
                        int packetID = buffer.ReadInt();
                        ClientPacketHandler.Handle(packetID, buffer);
                    }
                });

                packetLength = 0;
                if (_packetBuffer.UnreadLength() >= 4)
                {
                    packetLength = _packetBuffer.ReadInt();
                    if (packetLength <= 0)
                        return true;
                }
            }

            if (packetLength <= 1)
                return true;

            return false;
        }

        public void TrySendData(byte[] data)
        {
            try
            {
                if(socket != null)
                {
                    _stream.BeginWrite(data, 0, data.Length, null, null);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        
    }
}
