using Nadis.Net;
using Nadis.Net.Client;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    private static Dictionary<int, EntityContainer<Item>> _inventories;
    private static Dictionary<int, Item> _sceneEntities;
    [SerializeField]
    private ItemDatabase itemDatabase;

    private void Awake()
    {
        if (instance != null) Destroy(this);

        instance = this;
        _inventories = new Dictionary<int, EntityContainer<Item>>();
        _sceneEntities = new Dictionary<int, Item>();
    }

    //Items
    public static void RegisterItem(Item ent)
    {
        if (_sceneEntities == null) _sceneEntities = new Dictionary<int, Item>();
        if (ItemExists(ent.NetID)) return;

        _sceneEntities.Add(ent.NetID, ent);
    }
    public static void DeRegisterItem(Item ent)
    {
        if (ItemExists(ent.NetID) == false) return;
        _sceneEntities.Remove(ent.NetID);
    }

    public static Item GetItem(int networkID)
    {
        if (ItemExists(networkID) == false) return null;
        return _sceneEntities[networkID];
    }
    public static bool ItemExists(int networkID)
    {
        return _sceneEntities.ContainsKey(networkID);
    }

    public void ServerSpawnItem(IPacketData packet)
    {
        PacketItemSpawn data = (PacketItemSpawn)packet;
        if (_sceneEntities.ContainsKey(data.NetworkID)) return;

        ItemDatabaseFetch fetch = instance.itemDatabase.GetItem(data.ItemID);

        Item i = Instantiate(fetch.ItemObject, data.ItemPosition, Quaternion.identity).GetComponent<Item>();
        i.ID = data.ItemID;
        i.InitFromNetwork(data.NetworkID);
    }
    public void ServerDestroyItem(IPacketData packet)
    {
        PacketItemDestroy data = (PacketItemDestroy)packet;
        Item i = FindItem(data.NetworkID);
        if (i != null)
            i.Destroy();
    }
    public void ServerHideItem(IPacketData packet)
    {
        PacketItemVisibility data = (PacketItemVisibility)packet;
        Item i = FindItem(data.NetworkID);
        if (i != null)
            i.Hide(data.Hidden);
        else
            Log.Err("Hide Item :: Unable To Find Item({0})", data.NetworkID);
    }
    public void ServerDropItem(IPacketData packet)
    {
        PacketItemDrop data = (PacketItemDrop)packet;
        MoveItemToWorld(data.NetworkID, data.PlayerID);
    }
    
    public static void RequestSpawnItem(int itemID, Vector3 pos = default)
    {
        PacketItemSpawnRequest packet = new PacketItemSpawnRequest
        {
            ItemID = itemID,
            ItemPosition = pos
        };
        Events.Net.SendAsClient(NetData.LocalPlayerID, packet);
    }
    public static void RequestDestroyItem(int networkID)
    {
        PacketItemDestroyRequest req = new PacketItemDestroyRequest
        {
            NetworkID = networkID
        };
        Events.Net.SendAsClient(NetData.LocalPlayerID, req);
    }
    public static void RequestPickupItem(int networkID, int playerID)
    {
        PacketItemPickup packet = new PacketItemPickup
        {
            NetworkID = networkID,
            PlayerID = playerID
        };
        Events.Net.SendAsClientUnreliable(playerID, packet);
    }
    public static void RequestDropItem(int networkID, int playerID)
    {
        PacketItemDrop packet = new PacketItemDrop
        {
            NetworkID = networkID,
            PlayerID = playerID
        };
        Events.Net.SendAsClient(playerID, packet);
    }
    public static void HideItem(int networkID, bool hidden)
    {
        Item item = FindItem(networkID);
        if (item == null) return;

        PacketItemVisibility packet = new PacketItemVisibility
        {
            NetworkID = item.NetID,
            Hidden = hidden
        };
        Events.Net.SendAsClientUnreliable(NetData.LocalPlayerID, packet);
    }
    public static void HideItem(Item item, bool hidden)
    {
        if (item == null) return;

        PacketItemVisibility packet = new PacketItemVisibility
        {
            NetworkID = item.NetID,
            Hidden = hidden
        };
        Events.Net.SendAsClient(NetData.LocalPlayerID, packet);
    }

    //Inventory
    public static void CreateInventory(int playerID, int size)
    {
        DictCheck();
        if (Exists(playerID)) return;

        EntityContainer<Item> playerInventory = new EntityContainer<Item>(size);
        _inventories.Add(playerID, playerInventory);
        if(playerID == Nadis.Net.Client.Client.Local.NetID)
        {
            Events.Inventory.OnInventoryCreated?.Invoke(size);
        }
    }
    public static void DestroyInventory(int playerID)
    {
        if (Exists(playerID) == false) return;
        _inventories.Remove(playerID);
    }

    public static Item GetItem(int playerID, int netID)
    {
        if (Exists(playerID) == false) return null;
        return _inventories[playerID].GetItem(netID);
    }
    public static Item GetItemAt(int playerID, int index)
    {
        if (Exists(playerID) == false) return null;
        return _inventories[playerID].GetItemAt(index);
    }
    public static Item FindItem(int netID)
    {
        if(_sceneEntities.ContainsKey(netID))
        {
            return _sceneEntities[netID];
        }

        foreach (var id in _inventories.Keys)
        {
            Item i = _inventories[id].GetItem(netID);
            if (i != null)
            {
                return i;
            }
        }

        return null;
    }

    public static bool AddItem(int playerID, Item item)
    {
        if (Exists(playerID) == false) return false;
        int index = _inventories[playerID].AddItem(item);

        if(index != -1 )//&& playerID == NetworkedPlayer.LocalID)
        {
            Events.Inventory.OnItemAddedToInventory?.Invoke(item, index);
        }

        return (index != -1);
    }
    public static bool RemoveItem(int playerID, int index)
    {
        if (Exists(playerID) == false) return false;
        bool val = _inventories[playerID].Remove(index);
        if(val) //&& playerID == NetworkedPlayer.LocalID)
        {
            Events.Inventory.OnItemRemovedFromInventory?.Invoke(index);
        }

        return val;
    }
    public static bool RemoveItem(int playerID, Item item)
    {
        if (Exists(playerID) == false) return false;
        int index = _inventories[playerID].Remove(item);

        if(index != -1)// && playerID == NetworkedPlayer.LocalID)
        {
            Events.Inventory.OnItemRemovedFromInventory?.Invoke(index);
        }

        return (index != -1);
    }
    public static void DisableAllExcept(int playerID, int index)
    {
        if (Exists(playerID) == false) return;
        _inventories[playerID].DisableAllExcept(index);
    }

    public static bool Exists(int key)
    {
        if (_inventories == null) return false;

        return _inventories.ContainsKey(key);
    }
    private static void DictCheck()
    {
        if (_inventories == null) _inventories = new Dictionary<int, EntityContainer<Item>>();
    }
    public static int GetSize(int playerID)
    {
        if (Exists(playerID) == false) return -1;

        return _inventories[playerID].Size;
    }

    public static void MoveItemToInventory(int networkID, int playerID)
    {
        if (_inventories.ContainsKey(playerID) == false) { Log.Err("Failed To Find Inventory W/ Specified PlayerID"); return; }
        Item item = null;
        _sceneEntities.TryGetValue(networkID, out item);
        if (item == null) item = _inventories[playerID].GetItem(networkID);
        if (item == null) { Log.Err("Failed To Find ITem W/ Specificed NetID"); return; }

        item.Interact(playerID);
    }
    public static void MoveItemToWorld(int networkID, int playerID)
    {
        if (_inventories.ContainsKey(playerID) == false) { Log.Err("Failed To Find Inventory W/ Specified PlayerID"); }
        Item item = GetItem(playerID, networkID);
        if (item == null) { Log.Err("CLIENT :: Failed To Find Item({0}) in Inventory({1})!", networkID, playerID); return; }
        Debug.Log("Dropping Item");
        _inventories[playerID].Remove(item);
        item.Drop();
    }

}
