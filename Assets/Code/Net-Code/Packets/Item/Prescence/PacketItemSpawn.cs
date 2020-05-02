namespace Nadis.Net
{
    public struct PacketItemSpawn : IItemPacket
    {
        public int PacketID => (int)ServerPacket.SpawnItem;
        public int NetworkID { get; set; }
        public int ItemID { get; set; }

        public UnityEngine.Vector3 ItemPosition;

        public void Deserialize(PacketBuffer buffer)
        {
            NetworkID = buffer.ReadInt();
            ItemID = buffer.ReadInt();
            ItemPosition = buffer.ReadVector3();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);

            buffer.Write(NetworkID);
            buffer.Write(ItemID);
            buffer.Write(ItemPosition);

            buffer.WriteLength();
            return buffer;
        }
    }
}