using UnityEngine;
using KaymakNetwork;

namespace Nadis.Net.Foundation
{
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

        public static void SendPlayerPosition(int playerID, Vector3 pos)
        {
            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ClientPackets.CPlayerPos);
            buffer.WriteInt32(playerID);

            buffer.WriteDouble(pos.x);
            buffer.WriteDouble(pos.y);
            buffer.WriteDouble(pos.z);

            NetworkConfig.socket.SendData(buffer.Data, buffer.Head);
            buffer.Dispose();

        }

        public static void SendPlayerRotation(int playerID, float rot)
        {
            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ClientPackets.CPlayerRot);
            buffer.WriteInt32(playerID);

            buffer.WriteDouble(rot);

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

        public static void SendInventoryAdd(int index, PhysicalItem item, int playerID, bool send)
        {
            if (send == false) return;

            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ClientPackets.CInventoryEvent);
            buffer.WriteInt32((int)InventoryEventType.AddItem);
            buffer.WriteInt32(index);
            buffer.WriteInt32(playerID);
            buffer.WriteInt32(item.InstanceID);

            NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

            buffer.Dispose();
        }

        public static void SendInventoryRemove(int index, int playerID, bool send)
        {
            if (send == false) return;

            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ClientPackets.CInventoryEvent);
            buffer.WriteInt32((int)InventoryEventType.RemoveItem);
            buffer.WriteInt32(index);
            buffer.WriteInt32(playerID);

            NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

            buffer.Dispose();
        }

        public static void SendPlayerSetHandPosition(int playerID, Vector3 position, Side side, float speed, AnimatorTarget target, bool persistent, bool send)
        {
            if (send == false) return;

            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ClientPackets.CPlayerSetHandTarget);
            buffer.WriteInt32(playerID);

            //Target Position
            buffer.WriteDouble(position.x);
            buffer.WriteDouble(position.y);
            buffer.WriteDouble(position.z);

            buffer.WriteInt32((int)side);
            buffer.WriteDouble(speed);

            buffer.WriteInt32((int)target);

            buffer.WriteBoolean(persistent);

            NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

            buffer.Dispose();
        }

        public static void SendPlayerEndCurrentHandTarget(int playerID, bool send)
        {
            if (send == false) return;

            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ClientPackets.CPlayerSetHandTarget);
            buffer.WriteInt32(playerID);

            NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

            buffer.Dispose();
        }

        #region Item
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

        #endregion

        public static void SendPlayerAlterHealth(int playerID, float percent, bool send)
        {
            if (send == false) return;

            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ClientPackets.CPlayerStatsEvent);
            buffer.WriteInt32(playerID);
            buffer.WriteInt32((int)PlayerStatsEventType.AlterHealth);
            buffer.WriteDouble(percent);

            NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

            buffer.Dispose();
        }

        public static void SendPlayerAlterPower(int playerID, float percent, bool send)
        {
            if (send == false) return;

            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ClientPackets.CPlayerStatsEvent);
            buffer.WriteInt32(playerID);
            buffer.WriteInt32((int)PlayerStatsEventType.AlterPower);
            buffer.WriteDouble(percent);

            NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

            buffer.Dispose();
        }

        public static void SendLocalPlayerSpawned()
        {
            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ClientPackets.CPlayerSpawnedSuccessfully);

            NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

            buffer.Dispose();
        }

        public static void SendServerRequestedData_IslandPos(Vector3 position)
        {
            ByteBuffer buffer = new ByteBuffer(4);
            buffer.WriteInt32((int)ClientPackets.CSendServerRequestedData);
            buffer.WriteInt32((int)DataRequestType.IslandPosition);

            buffer.WriteDouble(position.x);
            buffer.WriteDouble(position.y);
            buffer.WriteDouble(position.z);

            NetworkConfig.socket.SendData(buffer.Data, buffer.Head);

            buffer.Dispose();
        }
    }
}
