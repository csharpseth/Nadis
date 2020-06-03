namespace Nadis.Net
{
    public struct PacketRequestDamageUnit : IPacketData
    {
        public int PacketID => (int)ClientPacket.RequestDamageUnit;

        public int unitID;
        public int damage;

        public void Deserialize(PacketBuffer buffer)
        {
            unitID = buffer.ReadInt();
            damage = buffer.ReadInt();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);

            buffer.Write(unitID);
            buffer.Write(damage);
            buffer.WriteLength();

            return buffer;
        }
    }
}