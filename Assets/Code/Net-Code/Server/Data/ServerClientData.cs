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

        public UnityEngine.Vector3 position;
        public UnityEngine.Vector3 lastCheckedPosition;
        public float rotation;

        //Private
        private TcpClient _socket;
        public ServerClientUDP UDP;

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

            position = UnityEngine.Vector3.zero;
            rotation = 0f;

            UDP = new ServerClientUDP(id);

            SubscribeEvents();
            BeginReceive();
        }
        public void Disconnect()
        {
            _socket.Close();
            if(_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
            _receiveBuffer = null;
            if(_packetBuffer != null)
            {
                _packetBuffer.Dispose();
                _packetBuffer = null;
            }

            UDP.Disconnect();

            Log.Not("Player({0}) has Disconnected.", NetID);
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
                    ClientManager.DisconnectClient(NetID);
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
                Log.Err(e);
                ClientManager.DisconnectClient(NetID);
            }
        }

        public void SendDataReliable(byte[] data)
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
                Log.Err(e);
            }
        }

        private void SetPosition(IPacketData packet)
        {
            PacketPlayerPosition data = (PacketPlayerPosition)packet;
            if (data.playerID != NetID) return;
            if(Util.CustomDistanceScore(lastCheckedPosition, data.playerPosition) >= ServerData.PlayerUnitsToMoveBeforeChargingPower)
            {
                ClientManager.MoveLosePower(NetID, lastCheckedPosition, data.playerPosition);
                lastCheckedPosition = data.playerPosition;
            }
            ServerChargingController.EvaluatePlayer(data.playerID, data.playerPosition);

            position = data.playerPosition;
        }
        private void SetRotation(IPacketData packet)
        {
            PacketPlayerRotation data = (PacketPlayerRotation)packet;
            if (data.playerID != NetID) return;
            rotation = data.playerRotation;

        }

        private void SubscribeEvents()
        {
            ServerPacketHandler.SubscribeTo((int)SharedPacket.PlayerPosition, SetPosition);
            ServerPacketHandler.SubscribeTo((int)SharedPacket.PlayerRotation, SetRotation);
        }
        private void UnSubscribeEvents()
        {
            ServerPacketHandler.UnSubscribeFrom((int)SharedPacket.PlayerPosition, SetPosition);
            ServerPacketHandler.UnSubscribeFrom((int)SharedPacket.PlayerRotation, SetRotation);
        }
        
    }
}
 