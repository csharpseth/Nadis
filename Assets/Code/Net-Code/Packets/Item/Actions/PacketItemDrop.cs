namespace Nadis.Net
{
    public struct PacketItemDrop : IPacketData
    {
        public int PacketID => (int)SharedPacket.ItemDrop;

        public int NetworkID;
        public int PlayerID;

        public void Deserialize(PacketBuffer buffer)
        {
            NetworkID = buffer.ReadInt();
            PlayerID = buffer.ReadInt();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);
            //use buffer.Write(variable); to append data to the packet buffer
            buffer.Write(NetworkID);
            buffer.Write(PlayerID);

            buffer.WriteLength();
            return buffer;
        }
    }
}