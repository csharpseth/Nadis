using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KaymakNetwork;
using System.Threading;

enum ServerPackets
{
    SWelcomeMSG = 1,
    SPlayerPosition = 2,
    SPlayerRotation = 3,
    SPlayerConnected = 4,
    SPlayerDisconnected = 5,
    SConnectionSuccessful = 6,
    SSpawnNetObject = 7,
    SDestroyNetObject = 8,
    SMoveNetObject = 9,
    SRotateNetObject = 10,
    SPlayerMoveData = 11,
    SPlayerInventoryUpdate = 12,
    SSpawnItem = 13,
    SDestroyItem = 14,
    SMoveItem = 15,
    SRotateItem = 16,
}

internal static class NetworkReceive
{
    internal static void PacketRouter()
    {
        NetworkConfig.socket.PacketId[(int)ServerPackets.SWelcomeMSG] = new KaymakNetwork.Network.Client.DataArgs(Packet_WelcomeMSG);
        NetworkConfig.socket.PacketId[(int)ServerPackets.SPlayerConnected] = new KaymakNetwork.Network.Client.DataArgs(Packet_PlayerConnected);
        NetworkConfig.socket.PacketId[(int)ServerPackets.SPlayerPosition] = new KaymakNetwork.Network.Client.DataArgs(Packet_PlayerPosition);
        NetworkConfig.socket.PacketId[(int)ServerPackets.SPlayerRotation] = new KaymakNetwork.Network.Client.DataArgs(Packet_PlayerRotation);
        NetworkConfig.socket.PacketId[(int)ServerPackets.SConnectionSuccessful] = new KaymakNetwork.Network.Client.DataArgs(Packet_ConnectionSuccessful);
        NetworkConfig.socket.PacketId[(int)ServerPackets.SSpawnNetObject] = new KaymakNetwork.Network.Client.DataArgs(Packet_NetObjectSpawn);
        NetworkConfig.socket.PacketId[(int)ServerPackets.SMoveNetObject] = new KaymakNetwork.Network.Client.DataArgs(Packet_NetObjectMove);
        NetworkConfig.socket.PacketId[(int)ServerPackets.SRotateNetObject] = new KaymakNetwork.Network.Client.DataArgs(Packet_NetObjectRotate);
        NetworkConfig.socket.PacketId[(int)ServerPackets.SPlayerMoveData] = new KaymakNetwork.Network.Client.DataArgs(Packet_PlayerMoveData);
        NetworkConfig.socket.PacketId[(int)ServerPackets.SPlayerInventoryUpdate] = new KaymakNetwork.Network.Client.DataArgs(Packet_PlayerInventoryUpdate);
        NetworkConfig.socket.PacketId[(int)ServerPackets.SSpawnItem] = new KaymakNetwork.Network.Client.DataArgs(Packet_SpawnItem);
        NetworkConfig.socket.PacketId[(int)ServerPackets.SDestroyItem] = new KaymakNetwork.Network.Client.DataArgs(Packet_DestroyItem);
        NetworkConfig.socket.PacketId[(int)ServerPackets.SMoveItem] = new KaymakNetwork.Network.Client.DataArgs(Packet_MoveItem);
        NetworkConfig.socket.PacketId[(int)ServerPackets.SRotateItem] = new KaymakNetwork.Network.Client.DataArgs(Packet_RotateItem);
    }

    private static void Packet_WelcomeMSG(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int playerID = buffer.ReadInt32();

        //NetworkManager.localPlayer.ID = playerID;

        buffer.Dispose();

        NetworkSend.SendPing();
    }

    private static void Packet_ConnectionSuccessful(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);

        //Map Data
        int connID = buffer.ReadInt32();
        int mapSeed = buffer.ReadInt32();
        int inventorySize = buffer.ReadInt32();

        //Apply/Generate From Data Given
        NetworkManager.ins.SetMapGeneratorData(mapSeed);

        NetworkManager.ins.CreateLocalPlayer(connID, inventorySize);

        buffer.Dispose();
    }
    
    private static void Packet_PlayerPosition(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int id = buffer.ReadInt32();

        float x = (float)buffer.ReadDouble();
        float y = (float)buffer.ReadDouble();
        float z = (float)buffer.ReadDouble();

        Vector3 newPos = new Vector3(x, y, z);

        NetworkManager.ins.SetPlayerPosition(id, newPos);

        buffer.Dispose();

    }
    
    private static void Packet_PlayerRotation(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int id = buffer.ReadInt32();

        float x = (float)buffer.ReadDouble();
        float y = (float)buffer.ReadDouble();
        float z = (float)buffer.ReadDouble();

        Vector3 newRot = new Vector3(x, y, z);

        NetworkManager.ins.SetPlayerRotation(id, newRot);

        buffer.Dispose();
    }
    
    private static void Packet_PlayerConnected(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int playerID = buffer.ReadInt32();
        int inventorySize = buffer.ReadInt32();

        //Handle Non-Local Player Instancing Here
        NetworkManager.ins.CreateNonLocalPlayer(playerID, inventorySize);

        buffer.Dispose();

        Debug.LogError("Player Connected With ID[" + playerID + "]");
    }

    private static void Packet_PlayerDisconnected(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int playerID = buffer.ReadInt32();

        //Handle Non-Local Player Instancing Here
        NetworkManager.ins.DestroyNonLocalPlayer(playerID);

        buffer.Dispose();

        Debug.LogError("Player Disconnected With ID[" + playerID + "]");
    }

    private static void Packet_NetObjectSpawn(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int registryID = buffer.ReadInt32();
        int spawnID = buffer.ReadInt32();

        NetworkManager.ins.netObjectsManager.Spawn(registryID, spawnID);

        buffer.Dispose();


    }

    private static void Packet_NetObjectMove(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int spawnID = buffer.ReadInt32();

        float x = (float)buffer.ReadDouble();
        float y = (float)buffer.ReadDouble();
        float z = (float)buffer.ReadDouble();
        Vector3 newPos = new Vector3(x, y, z);

        NetworkManager.ins.netObjectsManager.MoveNetObject(spawnID, newPos);

        buffer.Dispose();
    }

    private static void Packet_NetObjectRotate(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int spawnID = buffer.ReadInt32();

        float x = (float)buffer.ReadDouble();
        float y = (float)buffer.ReadDouble();
        float z = (float)buffer.ReadDouble();
        Vector3 newRot = new Vector3(x, y, z);

        NetworkManager.ins.netObjectsManager.RotateNetObject(spawnID, newRot);

        buffer.Dispose();
    }

    private static void Packet_PlayerMoveData(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int playerID = buffer.ReadInt32();
        bool grounded = buffer.ReadBoolean();
        float x = (float)buffer.ReadDouble();
        float y = (float)buffer.ReadDouble();
        int moveState = buffer.ReadInt32();
        float moveSpeed = (float)buffer.ReadDouble();

        NetworkManager.ins.SetPlayerMoveData(playerID, grounded, new Vector2(x, y), (PlayerMoveState)moveState, moveSpeed);

        buffer.Dispose();
    }

    private static void Packet_PlayerInventoryUpdate(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int playerID = buffer.ReadInt32();
        int inventorySize = buffer.ReadInt32();
        int[] ids = new int[inventorySize];
        for (int i = 0; i < inventorySize; i++)
        {
            ids[i] = buffer.ReadInt32();
        }

        NetworkManager.ins.UpdateInventory(playerID, ids);

        buffer.Dispose();
    }

    private static void Packet_SpawnItem(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int itemID = buffer.ReadInt32();
        int instanceID = buffer.ReadInt32();

        float x = (float)buffer.ReadDouble();
        float y = (float)buffer.ReadDouble();
        float z = (float)buffer.ReadDouble();

        float rotX = (float)buffer.ReadDouble();
        float rotY = (float)buffer.ReadDouble();
        float rotZ = (float)buffer.ReadDouble();

        Vector3 pos = new Vector3(x, y, z);
        Vector3 rot = new Vector3(rotX, rotY, rotZ);

        ItemManager.ins.SpawnItem(itemID, instanceID, pos, rot);

        buffer.Dispose();
    }

    private static void Packet_DestroyItem(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int instanceID = buffer.ReadInt32();

        ItemManager.ins.DestroyItem(instanceID);

        buffer.Dispose();
    }

    private static void Packet_MoveItem(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int instanceID = buffer.ReadInt32();

        float x = (float)buffer.ReadDouble();
        float y = (float)buffer.ReadDouble();
        float z = (float)buffer.ReadDouble();

        Vector3 pos = new Vector3(x, y, z);

        ItemManager.ins.OnItemMove(instanceID, pos);

        buffer.Dispose();
    }

    private static void Packet_RotateItem(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int instanceID = buffer.ReadInt32();

        float x = (float)buffer.ReadDouble();
        float y = (float)buffer.ReadDouble();
        float z = (float)buffer.ReadDouble();

        Vector3 rot = new Vector3(x, y, z);

        ItemManager.ins.OnItemRotate(instanceID, rot);

        buffer.Dispose();
    }

}
