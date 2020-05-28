namespace Nadis.Net
{
    public struct PacketUnitRotation : IPacketData
    {
        public int PacketID => (int)ServerPacket.UnitRotation;

        public int unitID;
        public float rot;

        public void Deserialize(PacketBuffer buffer)
        {
            unitID = buffer.ReadInt();
            rot = buffer.ReadFloat();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);

            buffer.Write(unitID);
            buffer.Write(rot);
            buffer.WriteLength();

            return buffer;
        }
    }
}