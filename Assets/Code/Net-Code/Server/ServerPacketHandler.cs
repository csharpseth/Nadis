using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadis.Net.Server
{
    public static class ServerPacketHandler
    {
        private static Dictionary<int, PacketHandlerData> handlers;

        public static void Initialize()
        {
            handlers = new Dictionary<int, PacketHandlerData>();
            PopulateHandlers();
        }
        public static void Handle(int packetID, PacketBuffer buffer)
        {
            if (handlers.ContainsKey(packetID) == false)
            {
                Log.Err("Failed to Handle Packet With ID of '{0}', No Handler Exists.", packetID);
                return;
            }
            
            handlers[packetID].Invoke(buffer);
        }
        /*
        public static void Handle(int packetID, JobPacketBuffer buffer)
        {
            if (handlers.ContainsKey(packetID) == false)
            {
                UnityEngine.Debug.LogErrorFormat("Failed to Handle Packet With ID of '{0}', No Handler Exists.", packetID);
                return;
            }

            handlers[packetID].Invoke(buffer);
        }
        */
        public static void SubscribeTo(int packetID, PacketHandlerData.ReceiveCallback callback)
        {
            if (handlers.ContainsKey(packetID) == false) return;

            handlers[packetID].Subscribe(callback);
        }
        public static void UnSubscribeFrom(int packetID, PacketHandlerData.ReceiveCallback callback)
        {
            if (handlers.ContainsKey(packetID) == false) return;

            handlers[packetID].UnSubscribe(callback);
        }

        //The Created Handlers should only utilize SharedPacketID & ServerPacketID
        //You should never need to use ClientPacketID here.
        private static void PopulateHandlers()
        {
            CreateHandler((int)SharedPacket.PlayerPosition, new PacketPlayerPosition(), (IPacketData data) =>
            {
                PacketPlayerPosition plyPos = (PacketPlayerPosition)data;
                ServerSend.UnReliableToAll(plyPos, plyPos.playerID);
            });

            CreateHandler((int)SharedPacket.PlayerRotation, new PacketPlayerRotation(), (IPacketData data) =>
            {
                PacketPlayerRotation plyRot = (PacketPlayerRotation)data;
                ServerSend.UnReliableToAll(plyRot, plyRot.playerID);
            });
            
            CreateHandler((int)SharedPacket.PlayerDisconnected, new PacketDisconnectPlayer(), (IPacketData data) => 
            {
                PacketDisconnectPlayer packet = (PacketDisconnectPlayer)data;
                ClientManager.DisconnectClient(packet.playerID);
                ServerSend.ReliableToAll(packet, packet.playerID);
            });

            CreateHandler((int)ClientPacket.SpawnItemRequest, new PacketItemSpawnRequest(), (IPacketData data) =>
            {
                PacketItemSpawnRequest req = (PacketItemSpawnRequest)data;
                int netID = ItemManager.AddItem(req.ItemID, req.ItemPosition);
                PacketItemSpawn cmd = new PacketItemSpawn
                {
                    ItemID = req.ItemID,
                    NetworkID = netID,
                    ItemPosition = req.ItemPosition
                };
                ServerSend.ReliableToAll(cmd);
            });

            CreateHandler((int)SharedPacket.ItemPosition, new PacketItemPosition(), (IPacketData data) =>
            {
                PacketItemPosition req = (PacketItemPosition)data;
                ItemManager.UpdateItemPosition(req.NetworkID, req.ItemPosition);
                ServerSend.UnReliableToAll(req);
            });

            CreateHandler((int)SharedPacket.ItemPickup, new PacketItemPickup(), (IPacketData data) =>
            {
                PacketItemPickup req = (PacketItemPickup)data;
                if(ItemManager.MoveItemToInventory(req.NetworkID, req.PlayerID))
                    ServerSend.ReliableToAll(req);
                else
                    Log.Err("SERVER :: Failed To Move Item({0}) To Player({1})'s Inventory!", req.NetworkID, req.PlayerID);
            });
            CreateHandler((int)ClientPacket.DestroyItemRequest, new PacketItemDestroyRequest(), (IPacketData data) =>
            {
                PacketItemDestroyRequest req = (PacketItemDestroyRequest)data;
                if(ItemManager.RemoveItem(req.NetworkID))
                {
                    PacketItemDestroy cmd = new PacketItemDestroy
                    {
                        NetworkID = req.NetworkID
                    };
                    ServerSend.ReliableToAll(cmd);
                }
            });
            CreateHandler((int)SharedPacket.ItemVisibility, new PacketItemVisibility(), (IPacketData data) =>
            {
                ServerSend.ReliableToAll(data);
            });
            CreateHandler((int)SharedPacket.ItemDrop, new PacketItemDrop(), (IPacketData data) =>
            {
                PacketItemDrop req = (PacketItemDrop)data;
                if (ItemManager.MoveItemToWorld(req.NetworkID, req.PlayerID))
                {
                    Log.Txt("SERVER :: Moved Item From Inventory to World!");
                    ServerSend.ReliableToAll(data);
                }
                else
                    Log.Err("SERVER :: Failed To Move Item({0}) From Inventory({1}) To World", req.NetworkID, req.PlayerID);
            });
            CreateHandler((int)ClientPacket.DamagePlayerRequest, new PacketRequestDamagePlayer(), (IPacketData data) =>
            {
                PacketRequestDamagePlayer packet = (PacketRequestDamagePlayer)data;
                if(ClientManager.TryDamagePlayer(packet.playerID, packet.alterAmount))
                {
                    PacketDamagePlayer cmd = new PacketDamagePlayer
                    {
                        playerID = packet.playerID,
                        alterAmount = packet.alterAmount
                    };

                    ServerSend.ReliableToAll(cmd);
                }
            });
            CreateHandler((int)SharedPacket.PlayerAnimatorMoveData, new PacketPlayerAnimatorMoveData(), (IPacketData data) =>
            {
                PacketPlayerAnimatorMoveData packet = (PacketPlayerAnimatorMoveData)data;
                ServerSend.UnReliableToAll(packet, packet.playerID);
            });
        }

        private static void CreateHandler(int packetID, IPacketData packetType,
            PacketHandlerData.ReceiveCallback callback = null)
        {
            PacketHandlerData handler = new PacketHandlerData(packetType, callback);
            handlers.Add(packetID, handler);
        }
    }
}
