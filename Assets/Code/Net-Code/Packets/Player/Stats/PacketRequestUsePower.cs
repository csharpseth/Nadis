namespace Nadis.Net
{
    public struct PacketRequestUsePower : IPacketData
    {
        public int PacketID => (int)ClientPacket.RequestUsePower;

        public int playerID;
        public int useAmount;


        public void Deserialize(PacketBuffer buffer)
        {
            playerID = buffer.ReadInt();
            useAmount = buffer.ReadInt();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);
            //use buffer.Write(variable); to append data to the packet buffer

            buffer.Write(playerID);
            buffer.Write(useAmount);

            buffer.WriteLength();
            return buffer;
        }
    }
}