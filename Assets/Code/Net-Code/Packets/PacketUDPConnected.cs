namespace Nadis.Net
{
    public struct PacketUDPConnected : IPacketData
    {
        public int PacketID => (int)ServerPacket.PlayerUDPConnected;

        public void Deserialize(PacketBuffer buffer) { }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);
            buffer.WriteLength();
            return buffer;
        }
    }
}

