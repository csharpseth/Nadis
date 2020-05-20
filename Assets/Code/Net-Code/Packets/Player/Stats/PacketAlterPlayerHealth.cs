namespace Nadis.Net
{
    public struct PacketAlterPlayerHealth : IPacketData
    {
        public int PacketID => (int)ServerPacket.DamagePlayer;

        public int playerID;
        public int health;


        public void Deserialize(PacketBuffer buffer)
        {
            playerID = buffer.ReadInt();
            health = buffer.ReadInt();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);
            //use buffer.Write(variable); to append data to the packet buffer

            buffer.Write(playerID);
            buffer.Write(health);

            buffer.WriteLength();
            return buffer;
        }
    }
}