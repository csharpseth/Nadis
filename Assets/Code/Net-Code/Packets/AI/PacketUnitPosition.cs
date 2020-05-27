using UnityEngine;

namespace Nadis.Net
{
    public struct PacketUnitPosition : IPacketData
    {
        public int PacketID => (int)ServerPacket.UnitPosition;

        public int unitID;
        public Vector3 location;
        public float speed;

        public void Deserialize(PacketBuffer buffer)
        {
            unitID = buffer.ReadInt();
            location = buffer.ReadVector3();
            speed = buffer.ReadFloat();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);

            buffer.Write(unitID);
            buffer.Write(location);
            buffer.Write(speed);
            buffer.WriteLength();

            return buffer;
        }
    }
}