namespace Nadis.Net
{
    public struct PacketAlterPlayerPower : IPacketData
    {
        public int PacketID => (int)ServerPacket.AlterPowerLevel;

        public int playerID;
        public int powerLevel;


        public void Deserialize(PacketBuffer buffer)
        {
            playerID = buffer.ReadInt();
            powerLevel = buffer.ReadInt();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);
            //use buffer.Write(variable); to append data to the packet buffer

            buffer.Write(playerID);
            buffer.Write(powerLevel);

            buffer.WriteLength();
            return buffer;
        }
    }
}