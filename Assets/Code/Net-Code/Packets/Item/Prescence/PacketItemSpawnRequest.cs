namespace Nadis.Net
{
    public struct PacketItemSpawnRequest : IItemPacket
    {
        public int PacketID => (int)ClientPacket.SpawnItemRequest;
        public int NetworkID { get; private set; }
        public int ItemID { get; set; }
        public UnityEngine.Vector3 ItemPosition;

        public void Deserialize(PacketBuffer buffer)
        {
            ItemID = buffer.ReadInt();
            ItemPosition = buffer.ReadVector3();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);

            buffer.Write(ItemID);
            buffer.Write(ItemPosition);

            buffer.WriteLength();
            return buffer;
        }
    }
}