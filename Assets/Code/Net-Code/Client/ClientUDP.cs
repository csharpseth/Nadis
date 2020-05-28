using Nadis.Net;
using Nadis.Net.Client;
using System;
using System.Net;
using System.Net.Sockets;

public class ClientUDP
{
    public UdpClient socket;
    public IPEndPoint endPoint;
    public bool connected;

    public ClientUDP(string ip, int port)
    {
        endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
    }

    public void Connect(int _localPort, int netID)
    {
        socket = new UdpClient(_localPort);

        socket.Connect(endPoint);
        BeginReceive();

        PacketUDPStart packet = new PacketUDPStart
        {
            playerID = netID
        };
        SendData(packet.Serialize().ToArray());
        Log.Event("Attempting To Connect UDP");
        Timeout.Create("clientudp", NetData.ClientTimeoutTimeInSeconds, () =>
        {
            if (connected == true) return;
            Log.Event("Failed To Establish UDP Connection, Reason: Timed Out");
            Events.Net.DisconnectClient();

        }, false);
    }
    public void Disconnect()
    {
        if(socket != null)
        {
            socket.Close();
            socket = null;
        }
        endPoint = null;
    }


    public void SendData(byte[] data)
    {
        try
        {
            if(socket != null)
            {
                socket.BeginSend(data, data.Length, null, null);
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e);
        }
    }

    private void BeginReceive()
    {
        socket.BeginReceive(ReceiveCallback, null);
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        if (socket == null || endPoint == null || ar == null) return;

        try
        {
            byte[] data = socket.EndReceive(ar, ref endPoint);
            BeginReceive();

            if(data.Length < 4)
            {
                //Disconnect
                Events.Net.DisconnectClient();
                return;
            }

            HandleData(data);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e);
        }
    }

    private void HandleData(byte[] data)
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
                ClientPacketHandler.Handle(packetID, buffer);
            }
        });
    }
}