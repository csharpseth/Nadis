namespace Nadis.Net
{
    public struct PacketPlayerAnimatorHeadData : IPacketData
    {
        public int PacketID => (int)SharedPacket.PlayerAnimatorHeadData;

        public int playerID;
        public float headAngle;

        public void Deserialize(PacketBuffer buffer)
        {
            playerID = buffer.ReadInt();
            headAngle = buffer.ReadFloat();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);

            buffer.Write(playerID);
            buffer.Write(headAngle);
            buffer.WriteLength();

            return buffer;
        }
    }
}