using Nadis.Net;
using Nadis.Net.Server;
using System.Net;
using System.Net.Sockets;

public class ServerClientUDP
{
    public IPEndPoint endPoint;
    private int clientID;

    public ServerClientUDP(int clientID)
    {
        this.clientID = clientID;
    }

    public void Connect(IPEndPoint endPoint)
    {
        this.endPoint = endPoint;
    }
    
    public void Disconnect()
    {
        endPoint = null;
    }
}