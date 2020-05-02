using Nadis.Net;

public struct PacketPlayerConnection : IPacketData
{
    public int PacketID => (int)ServerPacket.PlayerConnection;
    public int playerID;
    public bool playerIsLocal;

    public UnityEngine.Vector3 playerPosition;
    public float playerRotation;

    public void Deserialize(PacketBuffer buffer)
    {
        playerID = buffer.ReadInt();
        playerIsLocal = buffer.ReadBool();
        playerPosition = buffer.ReadVector3();
        playerRotation = buffer.ReadFloat();
    }

    public PacketBuffer Serialize()
    {
        PacketBuffer buffer = new PacketBuffer(PacketID);

        buffer.Write(playerID);
        buffer.Write(playerIsLocal);
        buffer.Write(playerPosition);
        buffer.Write(playerRotation);

        buffer.WriteLength();
        return buffer;
    }
    
}