using System.Collections.Generic;

namespace Nadis.Net.Server
{
    public static class ItemManager
    {
        //                       NetID - Item
        private static Dictionary<int, ItemData> _sceneItems;
        //                       PlayerID - List of Items
        private static Dictionary<int, Dictionary<int, ItemData>> _playerInventories;
        private static int _itemCount;

        public static void Init()
        {
            _sceneItems = new Dictionary<int, ItemData>();
            _playerInventories = new Dictionary<int, Dictionary<int, ItemData>>();
            _itemCount = 0;
        }

        public static int AddItem(int itemID, UnityEngine.Vector3 pos)
        {
            _itemCount++;
            int netID = _itemCount;
            if (_sceneItems.ContainsKey(netID)) return -1;

            ItemData data = new ItemData
            {
                ID = itemID,
                NetworkID = netID,
                Position = pos
            };

            _sceneItems.Add(netID, data);

            return netID;
        }
        public static bool RemoveItem(int networkID)
        {
            if (_sceneItems.ContainsKey(networkID))
            {
                _sceneItems.Remove(networkID);
                return true;
            }

            foreach (var id in _playerInventories.Keys)
            {
                if(_playerInventories[id].ContainsKey(networkID))
                {
                    _playerInventories[id].Remove(networkID);
                    return true;
                }
            }
            
            return false;
        }
        public static bool UpdateItemPosition(int networkID, UnityEngine.Vector3 pos)
        {
            if (_sceneItems.ContainsKey(networkID) == false) return false;

            ItemData temp = _sceneItems[networkID];
            temp.Position = pos;
            _sceneItems[networkID] = temp;
            return true;
        }

        public static void CreateInventory(int playerID)
        {
            if (_playerInventories.ContainsKey(playerID) || ClientManager.GetClient(playerID) == null) return;
            
            Dictionary<int, ItemData> newInventory = new Dictionary<int, ItemData>();
            _playerInventories.Add(playerID, newInventory);
        }
        public static void FindInInventory(int itemNetID, out int playerID)
        {
            foreach (var plyID in _playerInventories.Keys)
            {
                if(_playerInventories[plyID].ContainsKey(itemNetID))
                {
                    playerID = plyID;
                    return;
                }
            }

            playerID = -1;
        }

        public static bool MoveItemToInventory(int networkID, int playerID)
        {
            int otherPlayerID = 0;
            bool inWorld = false;

            FindInInventory(networkID, out otherPlayerID);
            if (_sceneItems.ContainsKey(networkID) == false)
            {
                if (otherPlayerID == -1)
                    return false;
                else
                    inWorld = false;
            }else
            {
                inWorld = true;
            }
            
            if(inWorld)
            {
                ItemData item = _sceneItems[networkID];
                _playerInventories[playerID].Add(networkID, item);
                _sceneItems.Remove(networkID);
                return true;
            }else
            {
                ItemData item = _playerInventories[otherPlayerID][networkID];
                _playerInventories[otherPlayerID].Remove(networkID);
                _playerInventories[playerID].Add(networkID, item);
                return true;
            }

        }
        public static bool MoveItemToWorld(int networkID, int playerID)
        {
            if (_playerInventories.ContainsKey(playerID) == false || _playerInventories[playerID].ContainsKey(networkID) == false)
                return false;

            ItemData item = _playerInventories[playerID][networkID];
            _playerInventories[playerID].Remove(networkID);
            _sceneItems.Add(networkID, item);
            return true;
        }

        public static void SendSceneItemsToPlayer(int playerID)
        {
            foreach (var id in _sceneItems.Keys)
            {
                ItemData item = _sceneItems[id];
                PacketItemSpawn spawnPacket = new PacketItemSpawn
                {
                    ItemID = item.ID,
                    NetworkID = item.NetworkID,
                    ItemPosition = item.Position
                };
                ServerSend.ReliableToOne(spawnPacket, playerID);
            }
        }
        public static void SendInventoryItemsToPlayer(int playerID)
        {
            foreach (var plyID in _playerInventories.Keys)
            {
                if (plyID == playerID) continue;

                foreach (var netID in _playerInventories[plyID].Keys)
                {
                    ItemData item = _playerInventories[plyID][netID];
                    PacketItemSpawn spawnPacket = new PacketItemSpawn
                    {
                        ItemID = item.ID,
                        NetworkID = item.NetworkID,
                        ItemPosition = item.Position
                    };
                    ServerSend.ReliableToOne(spawnPacket, playerID);
                }

                foreach (var netID in _playerInventories[plyID].Keys)
                {
                    ItemData item = _playerInventories[plyID][netID];
                    PacketItemPickup pickupPacket = new PacketItemPickup
                    {
                        NetworkID = item.NetworkID,
                        PlayerID = plyID
                    };
                    ServerSend.ReliableToOne(pickupPacket, playerID);
                }
            }
        }

        public static void Clear()
        {
            _sceneItems.Clear();
            _itemCount = 0;
        }
    }
}