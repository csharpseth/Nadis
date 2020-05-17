namespace Nadis.Net
{
    public struct PacketRequestDamagePlayer : IPacketData
    {
        public int PacketID => (int)ClientPacket.DamagePlayerRequest;

        public int playerID;
        public int damagerPlayerID;
        public int weaponDamage;
        public PlayerAppendage limbHit;
        public int weaponRange;


        public void Deserialize(PacketBuffer buffer)
        {
            playerID = buffer.ReadInt();
            damagerPlayerID = buffer.ReadInt();
            weaponDamage = buffer.ReadInt();
            limbHit = (PlayerAppendage)buffer.ReadInt();
            weaponRange = buffer.ReadInt();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);
            //use buffer.Write(variable); to append data to the packet buffer

            buffer.Write(playerID);
            buffer.Write(damagerPlayerID);
            buffer.Write(Util.FastAbs(weaponDamage));
            buffer.Write((int)limbHit);
            buffer.Write(Util.FastAbs(weaponRange));

            buffer.WriteLength();
            return buffer;
        }
    }
}