namespace Nadis.Net
{
    public struct PacketPlayerAnimatorMoveData : IPacketData
    {
        public int PacketID => (int)SharedPacket.PlayerAnimatorMoveData;

        public int playerID;
        public float forwardBlend;
        public float sideBlend;

        public void Deserialize(PacketBuffer buffer)
        {
            playerID = buffer.ReadInt();
            forwardBlend = buffer.ReadFloat();
            sideBlend = buffer.ReadFloat();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);
            //use buffer.Write(variable); to append data to the packet buffer
            buffer.Write(playerID);
            buffer.Write(forwardBlend);
            buffer.Write(sideBlend);

            buffer.WriteLength();
            return buffer;
        }
    }
}