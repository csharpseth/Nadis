namespace Nadis.Net
{
    public struct PacketUnitAnimationState : IPacketData
    {
        public int PacketID => (int)ServerPacket.UnitAnimationState;

        public int unitID;
        public Unity.Mathematics.int2 moveDir;
        public bool isDead;

        public void Deserialize(PacketBuffer buffer)
        {
            unitID = buffer.ReadInt();
            moveDir.y = buffer.ReadInt();
            moveDir.x = buffer.ReadInt();
            isDead = buffer.ReadBool();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);

            buffer.Write(unitID);
            buffer.Write(moveDir.y);
            buffer.Write(moveDir.x);
            buffer.Write(isDead);
            buffer.WriteLength();

            return buffer;
        }
    }
}