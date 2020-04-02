using System;
using KaymakNetwork;

namespace Darwin_Server
{
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
        CItemMove = 12,
        CItemRotate = 13,
    }

    internal static class NetworkReceive
    {
        internal static void PacketRouter()
        {
            NetworkConfig.Socket.PacketId[(int)ClientPackets.CPing] = Packet_Ping;
            NetworkConfig.Socket.PacketId[(int)ClientPackets.CPlayerPosition] = Packet_PlayerPosition;
            NetworkConfig.Socket.PacketId[(int)ClientPackets.CPlayerRotation] = Packet_PlayerRotation;
            NetworkConfig.Socket.PacketId[(int)ClientPackets.CSpawnNetObject] = Packet_NetObjectSpawn;
            NetworkConfig.Socket.PacketId[(int)ClientPackets.CMoveNetObject] = Packet_NetObjectMove;
            NetworkConfig.Socket.PacketId[(int)ClientPackets.CRotatateNetObject] = Packet_NetObjectRotate;
            NetworkConfig.Socket.PacketId[(int)ClientPackets.CPlayerMoveData] = Packet_PlayerMoveData;
            NetworkConfig.Socket.PacketId[(int)ClientPackets.CPlayerInventoryUpdate] = Packet_PlayerInventoryUpdate;
            NetworkConfig.Socket.PacketId[(int)ClientPackets.CPlayerRequestSpawnItem] = Packet_PlayerRequestSpawnItem;
            NetworkConfig.Socket.PacketId[(int)ClientPackets.CPlayerRequestDestroyItem] = Packet_PlayerRequestDestroyItem;
            NetworkConfig.Socket.PacketId[(int)ClientPackets.CItemMove] = Packet_MoveItem;
            NetworkConfig.Socket.PacketId[(int)ClientPackets.CItemRotate] = Packet_RotateItem;
        }

        private static void Packet_Ping(int connID, ref byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer(data);
            string msg = buffer.ReadString();

            Console.WriteLine(msg);
            NetworkSend.PlayerConnected(connID, ClientManager.InventorySize);
            NetworkSend.ConnectionSuccessful(connID, MapData.seed);

            buffer.Dispose();
        }

        private static void Packet_PlayerPosition(int connID, ref byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer(data);
            int playerID = buffer.ReadInt32();

            float x = (float)buffer.ReadDouble();
            float y = (float)buffer.ReadDouble();
            float z = (float)buffer.ReadDouble();

            NetworkSend.PlayerPosition(playerID, x, y, z);
            ClientManager.UpdateClientPosition(playerID, x, y, z);

            buffer.Dispose();

        }

        private static void Packet_PlayerRotation(int connID, ref byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer(data);
            int playerID = buffer.ReadInt32();

            float x = (float)buffer.ReadDouble();
            float y = (float)buffer.ReadDouble();
            float z = (float)buffer.ReadDouble();

            NetworkSend.PlayerRotation(playerID, x, y, z);
            ClientManager.UpdateClientRotation(playerID, x, y, z);
            
            buffer.Dispose();

        }

        private static void Packet_NetObjectSpawn(int connID, ref byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer(data);
            int registryID = buffer.ReadInt32();
            
            NetworkSend.SpawnNetObject(registryID);

            buffer.Dispose();
        }

        private static void Packet_NetObjectMove(int connID, ref byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer(data);
            int spawnID = buffer.ReadInt32();

            float x = (float)buffer.ReadDouble();
            float y = (float)buffer.ReadDouble();
            float z = (float)buffer.ReadDouble();
            Vector3 newPos = new Vector3(x, y, z);

            //Send Data To Clients \/
            NetworkSend.MoveNetObject(connID, spawnID, newPos);
            NetObjectManager.SetNetObjectPosition(spawnID, newPos);

            buffer.Dispose();
        }

        private static void Packet_NetObjectRotate(int connID, ref byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer(data);
            int spawnID = buffer.ReadInt32();

            float x = (float)buffer.ReadDouble();
            float y = (float)buffer.ReadDouble();
            float z = (float)buffer.ReadDouble();
            Vector3 newRot = new Vector3(x, y, z);

            NetworkSend.RotateNetObject(connID, spawnID, newRot);
            NetObjectManager.SetNetObjectRotation(spawnID, newRot);

            buffer.Dispose();
        }

        private static void Packet_PlayerMoveData(int connID, ref byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer(data);
            int playerID = buffer.ReadInt32();
            bool grounded = buffer.ReadBoolean();
            float x = (float)buffer.ReadDouble();
            float y = (float)buffer.ReadDouble();
            int moveState = buffer.ReadInt32();
            float moveSpeed = (float)buffer.ReadDouble();

            NetworkSend.PlayerMoveData(playerID, grounded, x, y, moveState, moveSpeed);
            ClientManager.UpdateClientMoveData(playerID, grounded, x, y, moveState);

            buffer.Dispose();
        }

        private static void Packet_PlayerInventoryUpdate(int connID, ref byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer(data);
            int playerID = buffer.ReadInt32();
            int inventorySize = buffer.ReadInt32();
            int[] inventory = new int[inventorySize];
            for (int i = 0; i < inventorySize; i++)
            {
                inventory[i] = buffer.ReadInt32();
            }

            NetworkSend.PlayerInventoryUpdate(playerID, inventory);
            ClientManager.UpdateClientInventory(playerID, inventory);

            buffer.Dispose();
        }

        private static void Packet_PlayerRequestSpawnItem(int connID, ref byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer(data);
            int itemID = buffer.ReadInt32();

            float x = (float)buffer.ReadDouble();
            float y = (float)buffer.ReadDouble();
            float z = (float)buffer.ReadDouble();

            float rotX = (float)buffer.ReadDouble();
            float rotY = (float)buffer.ReadDouble();
            float rotZ = (float)buffer.ReadDouble();

            Vector3 pos = new Vector3(x, y, z);
            Vector3 rot = new Vector3(rotX, rotY, rotZ);

            int instanceID = ItemManager.RegisterNewItem(itemID, pos, rot);
            NetworkSend.PlayerItemSpawn(itemID, instanceID, pos, rot);

            buffer.Dispose();
        }

        private static void Packet_PlayerRequestDestroyItem(int connID, ref byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer(data);
            int instanceID = buffer.ReadInt32();

            ItemManager.RemoveItem(instanceID);
            NetworkSend.PlayerItemDestroy(instanceID);

            buffer.Dispose();
        }

        private static void Packet_MoveItem(int connID, ref byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer(data);
            int instanceID = buffer.ReadInt32();

            float x = (float)buffer.ReadDouble();
            float y = (float)buffer.ReadDouble();
            float z = (float)buffer.ReadDouble();

            Vector3 pos = new Vector3(x, y, z);

            NetworkSend.SendItemMove(connID, instanceID, pos);
            ItemManager.UpdateItemPosition(instanceID, pos);

            buffer.Dispose();
        }

        private static void Packet_RotateItem(int connID, ref byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer(data);
            int instanceID = buffer.ReadInt32();

            float x = (float)buffer.ReadDouble();
            float y = (float)buffer.ReadDouble();
            float z = (float)buffer.ReadDouble();

            Vector3 rot = new Vector3(x, y, z);

            NetworkSend.SendItemRotate(connID, instanceID, rot);
            ItemManager.UpdateItemRotation(instanceID, rot);

            buffer.Dispose();
        }
        
    }
}
