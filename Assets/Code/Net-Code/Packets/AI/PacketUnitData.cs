using UnityEngine;

namespace Nadis.Net
{
    public struct PacketUnitData : IPacketData
    {
        public int PacketID => (int)ServerPacket.UnitData;

        public int unitID;
        public Vector3 location;

        public void Deserialize(PacketBuffer buffer)
        {
            unitID = buffer.ReadInt();
            location = buffer.ReadVector3();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);

            buffer.Write(unitID);
            buffer.Write(location);
            buffer.WriteLength();

            return buffer;
        }
    }
}