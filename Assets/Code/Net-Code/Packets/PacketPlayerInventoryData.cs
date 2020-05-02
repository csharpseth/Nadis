namespace Nadis.Net
{
    public struct PacketPlayerInventoryData : IPacketData
    {
        public int PacketID => (int)ServerPacket.PlayerInventoryData;

        public int playerID;
        public int size;

        public void Deserialize(PacketBuffer buffer)
        {
            playerID = buffer.ReadInt();
            size = buffer.ReadInt();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);

            buffer.Write(playerID);
            buffer.Write(size);
            buffer.WriteLength();

            return buffer;
        }
    }
}