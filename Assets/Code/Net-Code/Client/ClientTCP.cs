using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

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
                //NativeArray<byte> packetBytes = new NativeArray<byte>(_receivedPacket.ReadBytes(packetLength), Allocator.TempJob);
                //Try to do this with the job system instead
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

        public void TrySendData(PacketBuffer buffer)
        {
            try
            {
                if(socket != null)
                {
                    buffer.WriteLength();
                    _stream.BeginWrite(buffer.ToArray(), 0, buffer.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            buffer.Dispose();
        }
        /*
        struct HandlePacketBufferJob : IJob
        {
            [ReadOnly]
            public NativeArray<byte> bytes;

            public void Execute()
            {
                JobPacketBuffer buffer = new JobPacketBuffer();
                int packetID = buffer.ReadInt();
                ClientPacketHandler.Handle(packetID, buffer);
                buffer.Dispose();
                //PacketBuffer buffer = new PacketBuffer(bytes.ToArray());
                //int packetID = buffer.ReadInt();
                //ClientPacketHandler.Handle(packetID, buffer);
                //buffer.Dispose();
            }
        }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;
            _receivedPacket.SetBytes(data);

            if (_receivedPacket.UnreadLength() >= 4)
            {
                packetLength = _receivedPacket.ReadInt();
                if (packetLength <= 0)
                    return true;
            }

            while (packetLength > 0 && packetLength <= _receivedPacket.UnreadLength())
            {
                //byte[] packetBytes = _receivedPacket.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    NativeArray<byte> packetBytes = new NativeArray<byte>(_receivedPacket.ReadBytes(packetLength), Allocator.TempJob);
                    //Try to do this with the job system instead

                    HandlePacketBufferJob job = new HandlePacketBufferJob
                    {
                        bytes = packetBytes
                    };
                    JobHandle handle = job.Schedule(default);
                    handle.Complete();
                    packetBytes.Dispose();
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
        */

    }
}
