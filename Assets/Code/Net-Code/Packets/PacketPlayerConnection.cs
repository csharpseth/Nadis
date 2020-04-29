using Nadis.Net;

public struct PacketPlayerConnection : IPacketData
{
    public int PacketID => (int)ServerPacket.PlayerConnection;
    public int playerID;
    public bool playerIsLocal;
    
    public void Deserialize(PacketBuffer buffer)
    {
        playerID = buffer.ReadInt();
        playerIsLocal = buffer.ReadBool();
    }

    public PacketBuffer Serialize()
    {
        PacketBuffer buffer = new PacketBuffer(PacketID);
        buffer.Write(playerID);
        buffer.Write(playerIsLocal);
        buffer.WriteLength();
        return buffer;
    }
    
}