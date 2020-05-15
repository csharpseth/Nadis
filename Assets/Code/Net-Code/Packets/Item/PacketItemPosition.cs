namespace Nadis.Net
{
    public struct PacketItemPosition : IItemPacket
    {
        public int PacketID => (int)SharedPacket.ItemPosition;
        public int NetworkID { get; set; }
        public int ItemID { get; set; }

        public UnityEngine.Vector3 ItemPosition;

        public void Deserialize(PacketBuffer buffer)
        {
            NetworkID = buffer.ReadInt();
            ItemPosition = buffer.ReadVector3();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);

            buffer.Write(NetworkID);
            buffer.Write(ItemPosition);

            buffer.WriteLength();
            return buffer;
        }
    }
}