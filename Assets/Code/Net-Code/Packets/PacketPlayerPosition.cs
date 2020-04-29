using Nadis.Net;

public struct PacketPlayerPosition : IPacketData
{
    public int PacketID => (int)SharedPacket.PlayerPosition;

    public int playerID;
    public UnityEngine.Vector3 playerPosition;

    public void Deserialize(PacketBuffer buffer)
    {
        playerID = buffer.ReadInt();
        playerPosition.x = buffer.ReadFloat();
        playerPosition.y = buffer.ReadFloat();
        playerPosition.z = buffer.ReadFloat();
    }

    public PacketBuffer Serialize()
    {
        PacketBuffer buffer = new PacketBuffer(PacketID);

        buffer.Write(playerID);
        buffer.Write(playerPosition.x);
        buffer.Write(playerPosition.y);
        buffer.Write(playerPosition.z);
        buffer.WriteLength();

        return buffer;
    }
}