using System.Collections;
using System.Collections.Generic;
using Nadis.Net;
using UnityEngine;

public struct PacketPlayerTransform : IPacketData
{
    public int PacketID { get { return (int)SharedPacket.PlayerTransform; } }

    public int playerID;
    public Vector3 position;
    public float rotation;


    public void Deserialize(PacketBuffer buffer)
    {
        playerID = buffer.ReadInt();
        position.x = buffer.ReadFloat();
        position.y = buffer.ReadFloat();
        position.z = buffer.ReadFloat();

        rotation = buffer.ReadFloat();
    }

    public PacketBuffer Serialize()
    {
        PacketBuffer buffer = new PacketBuffer();
        buffer.Write(playerID);
        buffer.Write(position.x);
        buffer.Write(position.y);
        buffer.Write(position.z);
        buffer.Write(rotation);
        return buffer;
    }
}
