using Nadis.Net;

public struct PacketPlayerConnection : IPacketData
{
    public int PacketID => (int)ServerPacket.PlayerConnection;
    public int playerID;
    public bool playerIsLocal;

    public UnityEngine.Vector3 playerPosition;
    public float playerRotation;

    public int currentHealth;
    public int maxHealth;

    public void Deserialize(PacketBuffer buffer)
    {
        playerID = buffer.ReadInt();
        playerIsLocal = buffer.ReadBool();
        playerPosition = buffer.ReadVector3();
        playerRotation = buffer.ReadFloat();

        currentHealth = buffer.ReadInt();
        maxHealth = buffer.ReadInt();
    }

    public PacketBuffer Serialize()
    {
        PacketBuffer buffer = new PacketBuffer(PacketID);

        buffer.Write(playerID);
        buffer.Write(playerIsLocal);
        buffer.Write(playerPosition);
        buffer.Write(playerRotation);

        buffer.Write(currentHealth);
        buffer.Write(maxHealth);

        buffer.WriteLength();
        return buffer;
    }
    
}