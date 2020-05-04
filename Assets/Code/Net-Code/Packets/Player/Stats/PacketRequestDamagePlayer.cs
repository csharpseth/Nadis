namespace Nadis.Net
{
    public struct PacketRequestDamagePlayer : IPacketData
    {
        public int PacketID => (int)ClientPacket.DamagePlayerRequest;

        public int playerID;
        public int alterAmount;


        public void Deserialize(PacketBuffer buffer)
        {
            playerID = buffer.ReadInt();
            alterAmount = buffer.ReadInt();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);
            //use buffer.Write(variable); to append data to the packet buffer

            buffer.Write(playerID);
            buffer.Write(Util.FastAbs(alterAmount));

            buffer.WriteLength();
            return buffer;
        }
    }
}