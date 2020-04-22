using System.Collections.Generic;

public static class Inventory
{
    private static Dictionary<ulong, EntityContainer<Entity>> _inventories;
    private static Dictionary<ulong, Entity> _sceneEntities;

    //Entities
    public static void RegisterEntity(Entity ent)
    {
        if (_sceneEntities == null) _sceneEntities = new Dictionary<ulong, Entity>();
        if (EntityExists(ent.NetworkID)) return;

        _sceneEntities.Add(ent.NetworkID, ent);
    }
    public static void DeRegisterEntity(Entity ent)
    {
        if (EntityExists(ent.NetworkID) == false) return;
        _sceneEntities.Remove(ent.NetworkID);
    }

    public static Entity GetEntity(ulong networkID)
    {
        if (EntityExists(networkID) == false) return null;
        return _sceneEntities[networkID];
    }
    public static bool EntityExists(ulong networkID)
    {
        return _sceneEntities.ContainsKey(networkID);
    }


    //Inventory
    public static void Create(ulong playerID, int size)
    {
        DictCheck();
        if (Exists(playerID)) return;

        EntityContainer<Entity> playerInventory = new EntityContainer<Entity>(size);
        _inventories.Add(playerID, playerInventory);
        if(playerID == NetworkedPlayer.LocalID)
        {
            Events.Inventory.OnInventoryCreated?.Invoke(size);
        }
    }
    public static void Destroy(ulong playerID)
    {
        if (Exists(playerID) == false) return;
        _inventories.Remove(playerID);
    }

    public static Entity GetItem(ulong playerID, ulong netID)
    {
        if (Exists(playerID) == false) return null;
        return _inventories[playerID].GetItem(netID);
    }
    public static Entity GetItem(ulong playerID, int index)
    {
        if (Exists(playerID) == false) return null;
        return _inventories[playerID].GetItem(index);
    }

    public static bool AddItem(ulong playerID, Entity item)
    {
        if (Exists(playerID) == false) return false;
        int index = _inventories[playerID].AddItem(item);

        if(index != -1 && playerID == NetworkedPlayer.LocalID)
        {
            Events.Inventory.OnItemAddedToInventory?.Invoke(item, index);
        }

        return (index != -1);
    }
    public static bool RemoveItem(ulong playerID, int index)
    {
        if (Exists(playerID) == false) return false;
        bool val = _inventories[playerID].Remove(index);
        if(val && playerID == NetworkedPlayer.LocalID)
        {
            Events.Inventory.OnItemRemovedFromInventory?.Invoke(index);
        }

        return val;
    }
    public static bool RemoveItem(ulong playerID, Entity item)
    {
        if (Exists(playerID) == false) return false;
        int index = _inventories[playerID].Remove(item);

        if(index != -1 && playerID == NetworkedPlayer.LocalID)
        {
            Events.Inventory.OnItemRemovedFromInventory?.Invoke(index);
        }

        return (index != -1);
    }
    public static void DisableAllExcept(ulong playerID, int index)
    {
        if (Exists(playerID) == false) return;
        _inventories[playerID].DisableAllExcept(index);
    }

    private static bool Exists(ulong key)
    {
        return _inventories.ContainsKey(key);
    }
    private static void DictCheck()
    {
        if (_inventories == null) _inventories = new Dictionary<ulong, EntityContainer<Entity>>();
    }
    public static int GetSize(ulong playerID)
    {
        if (Exists(playerID) == false) return -1;

        return _inventories[playerID].Size;
    }
}
