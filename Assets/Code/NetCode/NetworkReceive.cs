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
    SRotateNetObject = 10

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
        float heightMultiplier = (float)buffer.ReadDouble();

        //Apply/Generate From Data Given
        NetworkManager.ins.SetMapGeneratorData(mapSeed);

        NetworkManager.ins.CreateLocalPlayer(connID);

        buffer.Dispose();
    }

    //Player Position
    private static void Packet_PlayerPosition(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int id = buffer.ReadInt32();

        float x = float.Parse(buffer.ReadString());
        float y = float.Parse(buffer.ReadString());
        float z = float.Parse(buffer.ReadString());

        Vector3 newPos = new Vector3(x, y, z);

        NetworkManager.ins.SetPlayerPosition(id, newPos);

        buffer.Dispose();

    }
    
    //Player Rotation
    private static void Packet_PlayerRotation(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int id = buffer.ReadInt32();

        float x = float.Parse(buffer.ReadString());
        float y = float.Parse(buffer.ReadString());
        float z = float.Parse(buffer.ReadString());

        Vector3 newRot = new Vector3(x, y, z);

        NetworkManager.ins.SetPlayerRotation(id, newRot);

        buffer.Dispose();
    }


    private static void Packet_PlayerConnected(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int playerID = buffer.ReadInt32();

        //Handle Non-Local Player Instancing Here
        NetworkManager.ins.CreateNonLocalPlayer(playerID);

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

}
