using Nadis.Net;

public struct PacketPlayerRotation : IPacketData
{
    public int PacketID => (int)SharedPacket.PlayerRotation;

    public int playerID;
    public float playerRotation;

    public void Deserialize(PacketBuffer buffer)
    {
        playerID = buffer.ReadInt();
        playerRotation = buffer.ReadFloat();
    }

    public PacketBuffer Serialize()
    {
        PacketBuffer buffer = new PacketBuffer(PacketID);

        buffer.Write(playerID);
        buffer.Write(playerRotation);
        buffer.WriteLength();

        return buffer;
    }
}