using UnityEngine;

namespace Nadis.Net
{
    public struct PacketUnitAnimationState : IPacketData
    {
        public int PacketID => (int)ServerPacket.UnitAnimationState;

        public int unitID;
        public Vector2Int moveDir;

        public void Deserialize(PacketBuffer buffer)
        {
            unitID = buffer.ReadInt();
            moveDir = buffer.ReadVector2Int();
        }

        public PacketBuffer Serialize()
        {
            PacketBuffer buffer = new PacketBuffer(PacketID);

            buffer.Write(unitID);
            buffer.Write(moveDir);
            buffer.WriteLength();

            return buffer;
        }
    }
}