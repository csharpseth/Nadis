using Nadis.Net;
using UnityEngine;

public struct PacketPlayerAnimatorTargetSet : IPacketData
{
    public int PacketID => (int)SharedPacket.PlayerAnimatorTargetSet;

    //Data
    public int playerID;
    public Vector3 targetsNewPosition;
    public Vector3 targetsNewRotation;
    public AnimatorTarget target;
    public float speed;
    public Space space;
    public bool persistent;
    public AnimatorTarget targetParent;
    public Side side;

    public void Deserialize(PacketBuffer buffer)
    {
        playerID = buffer.ReadInt();

        targetsNewPosition.x = buffer.ReadFloat();
        targetsNewPosition.y = buffer.ReadFloat();
        targetsNewPosition.z = buffer.ReadFloat();

        targetsNewRotation.x = buffer.ReadFloat();
        targetsNewRotation.y = buffer.ReadFloat();
        targetsNewRotation.z = buffer.ReadFloat();

        target = (AnimatorTarget)buffer.ReadInt();
        speed = buffer.ReadFloat();
        space = (Space)buffer.ReadInt();
        persistent = buffer.ReadBool();
        targetParent = (AnimatorTarget)buffer.ReadInt();
        side = (Side)buffer.ReadInt();
    }

    public PacketBuffer Serialize()
    {
        PacketBuffer buffer = new PacketBuffer(PacketID);

        buffer.Write(playerID);

        buffer.Write(targetsNewPosition.x);
        buffer.Write(targetsNewPosition.y);
        buffer.Write(targetsNewPosition.z);

        buffer.Write(targetsNewRotation.x);
        buffer.Write(targetsNewRotation.y);
        buffer.Write(targetsNewRotation.z);

        buffer.Write((int)target);
        buffer.Write(speed);
        buffer.Write((int)side);
        buffer.Write(persistent);
        buffer.Write((int)targetParent);
        buffer.Write((int)side);

        buffer.WriteLength();

        return buffer;
    }
}