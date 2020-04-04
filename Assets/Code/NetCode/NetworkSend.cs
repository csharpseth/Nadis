using System.Collections;
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
    CPlayerInventoryUpdate = 9,
    CPlayerRequestSpawnItem = 10,
    CPlayerRequestDestroyItem = 11,
    CItemEvent = 12,
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

    public static void SendPlayerInventoryUpdate(int id, int[] ids)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ClientPackets.CPlayerMoveData);
        buffer.WriteInt32(id);
        buffer.WriteInt32(ids.Length);
        for (int i = 0; i < ids.Length; i++)
        {
            buffer.WriteInt32(ids[i]);
        }

        NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

        buffer.Dispose();
    }

    public static void SendRequestItemSpawn(int itemID, Vector3 pos, Vector3 rot, bool send = true)
    {
        if (send == false)
            return;

        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ClientPackets.CItemEvent);
        buffer.WriteInt32(itemID);
        buffer.WriteInt32((int)ItemEventType.Spawn);

        buffer.WriteDouble(pos.x);
        buffer.WriteDouble(pos.y + 0.2f);
        buffer.WriteDouble(pos.z);

        buffer.WriteDouble(rot.x);
        buffer.WriteDouble(rot.y);
        buffer.WriteDouble(rot.z);

        NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

        buffer.Dispose();
    }

    public static void SendRequestItemDestroy(int instanceID, bool send = true)
    {
        if (send == false)
            return;

        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ClientPackets.CItemEvent);
        buffer.WriteInt32(instanceID);
        buffer.WriteInt32((int)ItemEventType.Destroy);

        NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

        buffer.Dispose();
    }

    public static void SendItemTransform(int instanceID, Vector3 pos, Vector3 rot, bool send = true)
    {
        if (send == false) return;

        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ClientPackets.CItemEvent);
        buffer.WriteInt32(instanceID);
        buffer.WriteInt32((int)ItemEventType.Transform);

        buffer.WriteDouble(pos.x);
        buffer.WriteDouble(pos.y);
        buffer.WriteDouble(pos.z);

        buffer.WriteDouble(rot.x);
        buffer.WriteDouble(rot.y);
        buffer.WriteDouble(rot.z);

        NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

        buffer.Dispose();
    }
    
    public static void SendItemInteract(int instanceID, int playerID, Side side, bool send = true)
    {
        if (send == false) return;

        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ClientPackets.CItemEvent);
        buffer.WriteInt32(instanceID);
        buffer.WriteInt32((int)ItemEventType.Interact);
        buffer.WriteInt32(playerID);
        buffer.WriteInt32((int)side);

        NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

        buffer.Dispose();
    }

    public static void SendItemHide(int instanceID, bool hide, bool send = true)
    {
        if (send == false) return;

        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ClientPackets.CItemEvent);
        buffer.WriteInt32(instanceID);
        buffer.WriteInt32((int)ItemEventType.Hide);
        buffer.WriteBoolean(hide);

        NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

        buffer.Dispose();
    }

    public static void SendItemReset(int instanceID, bool send = true)
    {
        if (send == false) return;

        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ClientPackets.CItemEvent);
        buffer.WriteInt32(instanceID);
        buffer.WriteInt32((int)ItemEventType.Reset);

        NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

        buffer.Dispose();
    }

}
