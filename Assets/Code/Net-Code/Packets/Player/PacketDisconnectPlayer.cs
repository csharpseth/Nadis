namespace Nadis.Net
{
    public struct PacketDisconnectPlayer : IPacketData
    {
        public int PacketID => (int)SharedPacket.PlayerDisconnected;

        public int playerID;

        public void Deserialize(PacketBuffer buffer)
        {
            playerID = buffer.ReadInt();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);

            buffer.Write(playerID);
            buffer.WriteLength();

            return buffer;
        }
    }
}