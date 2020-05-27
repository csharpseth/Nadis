namespace Nadis.Net
{
    public struct PacketRequestRevivePlayer : IPacketData
    {
        public int PacketID => (int)ClientPacket.RequestRevivePlayer;

        public int playerID;
        public int playerToReviveID;


        public void Deserialize(PacketBuffer buffer)
        {
            playerID = buffer.ReadInt();
            playerToReviveID = buffer.ReadInt();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);
            //use buffer.Write(variable); to append data to the packet buffer

            buffer.Write(playerID);
            buffer.Write(playerToReviveID);

            buffer.WriteLength();
            return buffer;
        }
    }
}