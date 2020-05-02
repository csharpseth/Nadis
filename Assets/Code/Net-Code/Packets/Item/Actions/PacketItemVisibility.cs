namespace Nadis.Net
{
    public struct PacketItemVisibility : IPacketData
    {
        public int PacketID => (int)SharedPacket.ItemVisibility;
        public int NetworkID;
        public bool Hidden;

        public void Deserialize(PacketBuffer buffer)
        {
            NetworkID = buffer.ReadInt();
            Hidden = buffer.ReadBool();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);
            //use buffer.Write(variable); to append data to the packet buffer

            buffer.Write(NetworkID);
            buffer.Write(Hidden);

            buffer.WriteLength();
            return buffer;
        }
    }
}