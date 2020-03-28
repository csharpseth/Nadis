﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KaymakNetwork;

enum ClientPackets
{
    CPing = 1,
    CPlayerPosition = 2,
    CPlayerRotation = 3,
    CSpawnNetObject = 4,
    CDestroyNetObject = 5,
    CMoveNetObject = 6,
    CRotatateNetObject = 7,
    CPlayerMoveData = 8,
}

internal static class NetworkSend
{
    public static void SendPing()
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ClientPackets.CPing);
        buffer.WriteString("Hello, am client :D");
        NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

        buffer.Dispose();
    }

    public static void SendPlayerPosition(int id, Vector3 pos)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ClientPackets.CPlayerPosition);
        buffer.WriteInt32(id);

        buffer.WriteDouble(pos.x);
        buffer.WriteDouble(pos.y);
        buffer.WriteDouble(pos.z);

        NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

        buffer.Dispose();

    }

    public static void SendPlayerRotation(int id, Vector3 rot)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ClientPackets.CPlayerRotation);
        buffer.WriteInt32(id);
        
        buffer.WriteDouble(rot.x);
        buffer.WriteDouble(rot.y);
        buffer.WriteDouble(rot.z);

        NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

        buffer.Dispose();

    }

    public static void SendSpawnNetObjectRequest(int registryID)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ClientPackets.CSpawnNetObject);
        buffer.WriteInt32(registryID);

        NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

        buffer.Dispose();
    }

    public static void SendMoveNetObjectRequest(int spawnID, Vector3 newPos)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ClientPackets.CMoveNetObject);
        buffer.WriteInt32(spawnID);
        buffer.WriteDouble(newPos.x);
        buffer.WriteDouble(newPos.y);
        buffer.WriteDouble(newPos.z);

        NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

        buffer.Dispose();
    }

    public static void SendRotateNetObjectRequest(int spawnID, Vector3 newRot)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ClientPackets.CRotatateNetObject);
        buffer.WriteInt32(spawnID);
        buffer.WriteDouble(newRot.x);
        buffer.WriteDouble(newRot.y);
        buffer.WriteDouble(newRot.z);

        NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

        buffer.Dispose();
    }

    public static void SendPlayerMoveData(int id, bool grounded, Vector2 inputDir, PlayerMoveState moveState, float moveSpeed)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ClientPackets.CPlayerMoveData);
        buffer.WriteInt32(id);
        buffer.WriteBoolean(grounded);
        buffer.WriteDouble(inputDir.x);
        buffer.WriteDouble(inputDir.y);
        buffer.WriteInt32((int)moveState);
        buffer.WriteDouble(moveSpeed);

        NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

        buffer.Dispose();
    }

}
