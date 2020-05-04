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

        targetsNewPosition = buffer.ReadVector3();

        targetsNewRotation = buffer.ReadVector3();

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

        buffer.Write(targetsNewPosition);

        buffer.Write(targetsNewRotation);

        buffer.Write((int)target);
        buffer.Write(speed);
        buffer.Write((int)space);
        buffer.Write(persistent);
        buffer.Write((int)targetParent);
        buffer.Write((int)side);

        buffer.WriteLength();

        return buffer;
    }
}