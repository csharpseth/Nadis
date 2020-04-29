using System;
using System.Net;
using System.Net.Sockets;

namespace Nadis.Net.Server
{
    public class ServerClientData : INetworkID
    {
        //Public
        public int NetID { get; private set; }
        public TcpClient Socket { get { return _socket; } }
        public bool Invalid { get { return _socket == null; } }

        //Private
        private TcpClient _socket;
        private NetworkStream _stream;
        private byte[] _receiveBuffer;
        private int _bufferSize;
        private PacketBuffer _packetBuffer;

        //Constructors
        public ServerClientData(TcpClient socket, int id)
        {
            NetID = id;
            _bufferSize = NetData.Default.BufferSize;

            _socket = socket;
            _socket.SendBufferSize = _bufferSize;
            _socket.ReceiveBufferSize = _bufferSize;

            _stream = _socket.GetStream();
            _receiveBuffer = new byte[_bufferSize];
            _packetBuffer = new PacketBuffer();

            BeginReceive();
        }

        //Functions
        private void BeginReceive()
        {
            _stream.BeginRead(_receiveBuffer, 0, _bufferSize, OnReceiveCallback, null);
        }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;
            _packetBuffer.SetBytes(data);

            if (_packetBuffer.UnreadLength() >= 4)
            {
                packetLength = _packetBuffer.ReadInt();
                if (packetLength <= 0)
                    return true;
            }

            while (packetLength > 0 && packetLength <= _packetBuffer.UnreadLength())
            {
                byte[] packetBytes = _packetBuffer.ReadBytes(packetLength);
                //NativeArray<byte> packetBytes = new NativeArray<byte>(_receivedPacket.ReadBytes(packetLength), Allocator.TempJob);
                //Try to do this with the job system instead
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (PacketBuffer buffer = new PacketBuffer(packetBytes))
                    {
                        int packetID = buffer.ReadInt();
                        ServerPacketHandler.Handle(packetID, buffer);
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

        private void OnReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int size = _stream.EndRead(ar);
                if (size <= 0)
                {
                    //Disconnect Client
                    return;
                }

                byte[] data = new byte[size];
                Array.Copy(_receiveBuffer, data, size);

                //Handle Data
                _packetBuffer.Reset(HandleData(data));

                BeginReceive();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
                //Disconnect Client
            }
        }

        public void SendData(byte[] data)
        {
            try
            {
                if(_socket != null)
                {
                    _stream.BeginWrite(data, 0, data.Length, null, null);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
        }
    }
}
 