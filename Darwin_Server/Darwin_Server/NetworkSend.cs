using System;
using KaymakNetwork;

namespace Darwin_Server
{
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

    internal static class NetworkSend
    {
        public static void WelcomeMsg(int connID)
        {
            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ServerPackets.SWelcomeMSG);
            buffer.WriteInt32(connID);
            NetworkConfig.Socket.SendDataTo(connID, buffer.Data, buffer.Head);

            buffer.Dispose();
        }

        public static void ConnectionSuccessful(int connID, int mapSeed)
        {
            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ServerPackets.SConnectionSuccessful);

            buffer.WriteInt32(connID);
            buffer.WriteInt32(mapSeed);
            buffer.WriteInt32(ClientManager.InventorySize);

            NetworkConfig.Socket.SendDataTo(connID, buffer.Data, buffer.Head);

            buffer.Dispose();
        }

        public static void PlayerPosition(int connID, float x, float y, float z)
        {
            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ServerPackets.SPlayerPosition);
            buffer.WriteInt32(connID);

            buffer.WriteDouble(x);
            buffer.WriteDouble(y);
            buffer.WriteDouble(z);

            NetworkConfig.Socket.SendDataToAllBut(connID, buffer.Data, buffer.Head);
            buffer.Dispose();

        }

        public static void PlayerRotation(int connID, float x, float y, float z)
        {
            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ServerPackets.SPlayerRotation);
            buffer.WriteInt32(connID);

            buffer.WriteDouble(x);
            buffer.WriteDouble(y);
            buffer.WriteDouble(z);

            NetworkConfig.Socket.SendDataToAllBut(connID, buffer.Data, buffer.Head);
            buffer.Dispose();

        }

        public static void PlayerConnected(int connID, int inventorySize, int sendTo = -1)
        {
            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ServerPackets.SPlayerConnected);
            buffer.WriteInt32(connID);
            buffer.WriteInt32(inventorySize);

            if(sendTo == -1)
            {
                NetworkConfig.Socket.SendDataToAllBut(connID, buffer.Data, buffer.Head);
            }
            else
                NetworkConfig.Socket.SendDataTo(sendTo, buffer.Data, buffer.Head);

            buffer.Dispose();
        }

        public static void PlayerDisconnected(int connID)
        {
            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ServerPackets.SPlayerDisconnected);
            buffer.WriteInt32(connID);

            NetworkConfig.Socket.SendDataToAll(buffer.Data, buffer.Head);

            buffer.Dispose();

        }

        public static void SpawnNetObject(int registryID)
        {
            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ServerPackets.SSpawnNetObject);
            buffer.WriteInt32(registryID);
            buffer.WriteInt32(NetObjectManager.RegisterObject(registryID));

            NetworkConfig.Socket.SendDataToAll(buffer.Data, buffer.Head);

            buffer.Dispose();
        }

        public static void MoveNetObject(int connID, int spawnID, Vector3 newPos)
        {
            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ServerPackets.SMoveNetObject);
            buffer.WriteInt32(spawnID);

            buffer.WriteDouble(newPos.x);
            buffer.WriteDouble(newPos.y);
            buffer.WriteDouble(newPos.z);

            NetworkConfig.Socket.SendDataToAllBut(connID, buffer.Data, buffer.Head);

            buffer.Dispose();
        }

        public static void RotateNetObject(int connID, int spawnID, Vector3 newRot)
        {
            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ServerPackets.SRotateNetObject);
            buffer.WriteInt32(spawnID);

            buffer.WriteDouble(newRot.x);
            buffer.WriteDouble(newRot.y);
            buffer.WriteDouble(newRot.z);

            NetworkConfig.Socket.SendDataToAllBut(connID, buffer.Data, buffer.Head);

            buffer.Dispose();
        }

        public static void PlayerMoveData(int connID, bool grounded, float x, float y, int moveState, float moveSpeed)
        {
            ByteBuffer buffer = new ByteBuffer(4);

            buffer.WriteInt32((int)ServerPackets.SPlayerMoveData);
            buffer.WriteInt32(connID);
            buffer.WriteBoolean(grounded);
            buffer.WriteDouble(x);
            buffer.WriteDouble(y);
            buffer.WriteInt32(moveState);
            buffer.WriteDouble(moveSpeed);

            NetworkConfig.Socket.SendDataToAllBut(connID, buffer.Data, buffer.Head);

            buffer.Dispose();
        }

        public static void PlayerInventoryUpdate(int connID, int[] inventory)
        {
            ByteBuffer buffer = new ByteBuffer(4);

            buffer.WriteInt32((int)ServerPackets.SPlayerInventoryUpdate);
            buffer.WriteInt32(connID);
            buffer.WriteInt32(inventory.Length);
            for (int i = 0; i < inventory.Length; i++)
            {
                buffer.WriteInt32(inventory[i]);
            }

            NetworkConfig.Socket.SendDataToAllBut(connID, buffer.Data, buffer.Head);

            buffer.Dispose();
        }

        public static void PlayerItemSpawn(int itemID, int instanceID, Vector3 pos, Vector3 rot)
        {
            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ServerPackets.SSpawnItem);
            buffer.WriteInt32(itemID);
            buffer.WriteInt32(instanceID);

            buffer.WriteDouble(pos.x);
            buffer.WriteDouble(pos.y);
            buffer.WriteDouble(pos.z);

            buffer.WriteDouble(rot.x);
            buffer.WriteDouble(rot.y);
            buffer.WriteDouble(rot.z);

            NetworkConfig.Socket.SendDataToAll(buffer.Data, buffer.Head);

            buffer.Dispose();
        }

        public static void PlayerItemDestroy(int instanceID)
        {
            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ServerPackets.SDestroyItem);
            buffer.WriteInt32(instanceID);

            NetworkConfig.Socket.SendDataToAll(buffer.Data, buffer.Head);

            buffer.Dispose();
        }

        public static void SendItemMove(int connID, int instanceID, Vector3 pos)
        {
            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ServerPackets.SMoveItem);
            buffer.WriteInt32(instanceID);

            buffer.WriteDouble(pos.x);
            buffer.WriteDouble(pos.y);
            buffer.WriteDouble(pos.z);

            NetworkConfig.Socket.SendDataToAllBut(connID, buffer.Data, buffer.Head);

            buffer.Dispose();
        }

        public static void SendItemRotate(int connID, int instanceID, Vector3 rot)
        {
            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ServerPackets.SRotateItem);
            buffer.WriteInt32(instanceID);

            buffer.WriteDouble(rot.x);
            buffer.WriteDouble(rot.y);
            buffer.WriteDouble(rot.z);

            NetworkConfig.Socket.SendDataToAllBut(connID, buffer.Data, buffer.Head);

            buffer.Dispose();
        }

    }
}
