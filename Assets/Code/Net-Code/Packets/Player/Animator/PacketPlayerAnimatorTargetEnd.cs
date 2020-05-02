using Nadis.Net;

public struct PacketPlayerAnimatorTargetEnd : IPacketData
{
    public int PacketID => (int)SharedPacket.PlayerAnimatorTargetEnd;

    public int playerID;
    public AnimatorTarget target;
    public Side side;

    public void Deserialize(PacketBuffer buffer)
    {
        playerID = buffer.ReadInt();
        target = (AnimatorTarget)buffer.ReadInt();
        side = (Side)buffer.ReadInt();
    }

    public PacketBuffer Serialize()
    {
        PacketBuffer buffer = new PacketBuffer(PacketID);

        buffer.Write(playerID);
        buffer.Write((int)target);
        buffer.Write((int)side);

        buffer.WriteLength();

        return buffer;
    }
}