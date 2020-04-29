using Nadis.Net;

public struct PacketPlayerAnimatorData : IPacketData
{
    public int PacketID => (int)SharedPacket.PlayerAnimatorData;

    public int playerID;
    public PlayerMoveState playerMoveState;
    public UnityEngine.Vector2 playerInputDir;
    public bool playerGrounded;

    public void Deserialize(PacketBuffer buffer)
    {
        playerID = buffer.ReadInt();
        playerMoveState = (PlayerMoveState)buffer.ReadInt();
        playerInputDir.x = buffer.ReadFloat();
        playerInputDir.y = buffer.ReadFloat();
        playerGrounded = buffer.ReadBool();
    }

    public PacketBuffer Serialize()
    {
        PacketBuffer buffer = new PacketBuffer(PacketID);

        buffer.Write(playerID);
        buffer.Write((int)playerMoveState);
        buffer.Write(playerInputDir.x);
        buffer.Write(playerInputDir.y);
        buffer.Write(playerGrounded);
        buffer.WriteLength();

        return buffer;
    }
}