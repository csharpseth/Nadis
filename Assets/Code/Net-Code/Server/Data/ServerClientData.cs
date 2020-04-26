using System;
using System.Net;
using System.Net.Sockets;

namespace Nadis.Net.Server
{
    public struct ServerClientData : INetworkID
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

            BeginReceive();
        }

        //Functions
        private void BeginReceive()
        {
            _stream.BeginRead(_receiveBuffer, 0, _bufferSize, OnReceiveCallback, null);
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

                BeginReceive();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
                //Disconnect Client
            }
        }

        public void SendData(PacketBuffer buffer, bool disposeWhenFinished = true)
        {
            try
            {
                if(_socket != null)
                {
                    buffer.WriteLength();
                    _stream.BeginWrite(buffer.ToArray(), 0, buffer.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
            
            if (disposeWhenFinished)
                buffer.Dispose();
        }
    }
}