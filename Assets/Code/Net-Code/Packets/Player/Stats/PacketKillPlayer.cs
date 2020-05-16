namespace Nadis.Net
{
    public struct PacketKillPlayer : IPacketData
    {
        public int PacketID => (int)ServerPacket.KillPlayer;

        public int playerID;
        public bool respawn;

        public void Deserialize(PacketBuffer buffer)
        {
            playerID = buffer.ReadInt();
            respawn = buffer.ReadBool();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);

            buffer.Write(playerID);
            buffer.Write(respawn);

            buffer.WriteLength();
            return buffer;
        }
    }
}