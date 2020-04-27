using Nadis.Net;

public struct PacketWelcomeMessage : IPacketData
{
    public int PacketID { get { return (int)ServerPacket.WelcomeMessage; } }
    public string message;
    public int clientID;
    
    public void Deserialize(PacketBuffer buffer)
    {
        message = buffer.ReadString();
        clientID = buffer.ReadInt();
    }

    public PacketBuffer Serialize()
    {
        PacketBuffer buffer = new PacketBuffer(PacketID);
        buffer.Write(message);
        buffer.Write(clientID);
        return buffer;
    }
    
}