namespace Nadis.Net
{
    public struct PacketItemPickup : IPacketData
    {
        public int PacketID => (int)SharedPacket.ItemPickup;

        //ID of the player who interacted /w or picked up the item
        public int PlayerID;
        //ID of the item they picked up
        public int NetworkID;


        public void Deserialize(PacketBuffer buffer)
        {
            PlayerID = buffer.ReadInt();
            NetworkID = buffer.ReadInt();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);
            //use buffer.Write(variable); to append data to the packet buffer

            buffer.Write(PlayerID);
            buffer.Write(NetworkID);

            buffer.WriteLength();
            return buffer;
        }
    }
}