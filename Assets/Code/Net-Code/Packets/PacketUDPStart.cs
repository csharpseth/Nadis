namespace Nadis.Net
{
    public struct PacketUDPStart : IPacketData
    {
        public int PacketID => (int)ClientPacket.PlayerUDPStart;

        public int playerID;

        public void Deserialize(PacketBuffer buffer)
        {
            playerID = buffer.ReadInt();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);
            //use buffer.Write(variable); to append data to the packet buffer
            buffer.Write(playerID);
            buffer.WriteLength();
            return buffer;
        }
    }
}