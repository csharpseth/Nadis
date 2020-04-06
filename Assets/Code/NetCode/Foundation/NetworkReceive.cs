using UnityEngine;
using KaymakNetwork;

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
    SInventoryEvent = 12,
    SSpawnItem = 13,
    SDestroyItem = 14,
    SItemEvent = 15,
    SPlayerSetHandPosition = 16,
    SPlayerEndCurrentHandPosition = 17,
    SPlayerStatEvent = 18,
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
        NetworkConfig.socket.PacketId[(int)ServerPackets.SInventoryEvent] = new KaymakNetwork.Network.Client.DataArgs(Packet_InventoryEvent);
        NetworkConfig.socket.PacketId[(int)ServerPackets.SSpawnItem] = new KaymakNetwork.Network.Client.DataArgs(Packet_SpawnItem);
        NetworkConfig.socket.PacketId[(int)ServerPackets.SDestroyItem] = new KaymakNetwork.Network.Client.DataArgs(Packet_DestroyItem);
        NetworkConfig.socket.PacketId[(int)ServerPackets.SItemEvent] = new KaymakNetwork.Network.Client.DataArgs(Packet_ItemEvent);
        NetworkConfig.socket.PacketId[(int)ServerPackets.SPlayerSetHandPosition] = new KaymakNetwork.Network.Client.DataArgs(Packet_PlayerHandPosition);
        NetworkConfig.socket.PacketId[(int)ServerPackets.SPlayerEndCurrentHandPosition] = new KaymakNetwork.Network.Client.DataArgs(Packet_PlayerEndCurrentHandPosition);
        NetworkConfig.socket.PacketId[(int)ServerPackets.SPlayerStatEvent] = new KaymakNetwork.Network.Client.DataArgs(Packet_PlayerStatEvent);
    }

    private static void Packet_WelcomeMSG(ref byte[] data)
    {
        NetworkSend.SendPing();
    }

    private static void Packet_ConnectionSuccessful(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);

        //Map Data
        int connID = buffer.ReadInt32();
        int mapSeed = buffer.ReadInt32();
        int inventorySize = buffer.ReadInt32();

        int playerMaxHealth = buffer.ReadInt32();
        float playerStartHealth = (float)buffer.ReadDouble();
        int playerMaxPower = buffer.ReadInt32();
        float playerStartPower = (float)buffer.ReadDouble();

        //Apply/Generate From Data Given
        NetworkManager.ins.SetDefaultStats(new PlayerStats(playerMaxHealth, playerStartHealth, playerMaxPower, playerStartPower));
        NetworkManager.ins.SetMapGeneratorData(mapSeed);
        NetworkManager.ins.CreatePlayer(connID, inventorySize, true);

        Debug.Log("Connected To Server Successfully");

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

        int playerMaxHealth = buffer.ReadInt32();
        float playerStartHealth = (float)buffer.ReadDouble();
        int playerMaxPower = buffer.ReadInt32();
        float playerStartPower = (float)buffer.ReadDouble();

        NetworkManager.ins.SetDefaultStats(new PlayerStats(playerMaxHealth, playerStartHealth, playerMaxPower, playerStartPower));
        NetworkManager.ins.CreatePlayer(playerID, inventorySize, false);

        Debug.Log("Another Player Has Connected");

        buffer.Dispose();
    }

    private static void Packet_PlayerDisconnected(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int playerID = buffer.ReadInt32();

        //Handle Non-Local Player Instancing Here
        NetworkManager.ins.DestroyRemotePlayer(playerID);

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

    private static void Packet_InventoryEvent(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        InventoryEventType type = (InventoryEventType)buffer.ReadInt32();
        int instanceID = buffer.ReadInt32();
        int playerID = buffer.ReadInt32();

        if (type == InventoryEventType.AddItem)
        {
            Events.Inventory.AddItem(instanceID, playerID, false);
        }
        else if (type == InventoryEventType.RemoveItem)
        {
            Events.Inventory.RemoveItem(instanceID, playerID, false);
        }

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
    
    private static void Packet_ItemEvent(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int instanceID = buffer.ReadInt32();
        ItemEventType type = (ItemEventType)buffer.ReadInt32();
        
        if(type == ItemEventType.Interact)
        {
            int playerID = buffer.ReadInt32();
            Side side = (Side)buffer.ReadInt32();

            Events.Item.Interact(instanceID, playerID, side, false);
            //Call The ItemInteract Event

        }else if (type == ItemEventType.Hide)
        {
            bool hide = buffer.ReadBoolean();

            Events.Item.Hide(instanceID, hide, false);
            //Call the ItemReset Event

        }else if(type == ItemEventType.Reset)
        {
            //Call the ItemHide Event
            Events.Item.Reset(instanceID, false);
        }else if(type == ItemEventType.Transform)
        {
            Vector3 pos = new Vector3((float)buffer.ReadDouble(), (float)buffer.ReadDouble(), (float)buffer.ReadDouble());
            Vector3 rot = new Vector3((float)buffer.ReadDouble(), (float)buffer.ReadDouble(), (float)buffer.ReadDouble());

            Events.Item.OnSetItemTransform(instanceID, pos, rot);
        }else if(type == ItemEventType.Spawn)
        {
            int itemID = buffer.ReadInt32();
            Vector3 pos = new Vector3((float)buffer.ReadDouble(), (float)buffer.ReadDouble(), (float)buffer.ReadDouble());
            Vector3 rot = new Vector3((float)buffer.ReadDouble(), (float)buffer.ReadDouble(), (float)buffer.ReadDouble());

            ItemManager.ins.SpawnItem(itemID, instanceID, pos, rot);
        }
        else if (type == ItemEventType.Destroy)
        {
            ItemManager.ins.DestroyItem(instanceID);
        }

        buffer.Dispose();
    }

    private static void Packet_PlayerHandPosition(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);

        int playerID = buffer.ReadInt32();

        float x = (float)buffer.ReadDouble();
        float y = (float)buffer.ReadDouble();
        float z = (float)buffer.ReadDouble();
        Vector3 pos = new Vector3(x, y, z);

        Side side = (Side)buffer.ReadInt32();
        float speed = (float)buffer.ReadDouble();

        AnimatorTarget target = (AnimatorTarget)buffer.ReadInt32();

        bool persistent = buffer.ReadBoolean();

        Events.BipedAnimator.SetHandTargetPosition(playerID, pos, side, speed, target, persistent, false);

        buffer.Dispose();
    }

    private static void Packet_PlayerEndCurrentHandPosition(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);

        int playerID = buffer.ReadInt32();

        Events.BipedAnimator.EndCurrentHandTarget(playerID, false);

        buffer.Dispose();
    }

    private static void Packet_PlayerStatEvent(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int playerID = buffer.ReadInt32();
        PlayerStatsEventType type = (PlayerStatsEventType)buffer.ReadInt32();

        if(type != PlayerStatsEventType.Die)
        {
            float perc = (float)buffer.ReadDouble();
            if(type == PlayerStatsEventType.AlterHealth)
            {
                Events.PlayerStats.SetHealth?.Invoke(playerID, perc, false);
            }else if(type == PlayerStatsEventType.AlterPower)
            {
                Events.PlayerStats.SetPower?.Invoke(playerID, perc, false);
            }
        }else
        {
            Events.PlayerStats.Die?.Invoke(playerID);
        }
        
        buffer.Dispose();
    }

}
