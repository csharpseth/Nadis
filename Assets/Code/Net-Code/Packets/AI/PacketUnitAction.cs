namespace Nadis.Net
{
    public struct PacketUnitAction : IPacketData
    {
        public int PacketID => (int)ServerPacket.UnitAction;

        public int unitID;
        public UnitActionType action;



        public void Deserialize(PacketBuffer buffer)
        {
            unitID = buffer.ReadInt();
            action = (UnitActionType)buffer.ReadInt();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);

            buffer.Write(unitID);
            buffer.Write((int)action);
            buffer.WriteLength();

            return buffer;
        }
    }
}