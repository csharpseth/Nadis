using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager ins;
    public ItemDatabase itemDatabase;
    public Dictionary<int, PhysicalItem> instanceItems;

    public Action<int, Vector3> OnItemMove;
    public Action<int, Vector3> OnItemRotate;
    private Action<int> OnItemCollect;
    private Action<int, bool> OnItemHide;

    private void Awake()
    {
        if (ins == null)
        {
            ins = this;
            instanceItems = new Dictionary<int, PhysicalItem>();
        }
    }

    public void RequestSpawnItem(int itemID, Vector3 pos = default(Vector3), Vector3 rot = default(Vector3))
    {
        NetworkSend.SendPlayerRequestItemSpawn(itemID, pos, rot);
    }

    public void SpawnItem(int itemID, int instanceID, Vector3 pos, Vector3 rot)
    {
        PhysicalItem item = itemDatabase.SpawnItem(itemID, pos, rot);
        item.SetInstanceID(instanceID);

        OnItemMove += item.ItemMove;
        OnItemRotate += item.ItemRotate;

        instanceItems.Add(instanceID, item);

    }

    public void RequestDestroyItem(int instanceID)
    {
        if(NetworkManager.ins != null)
        {
            NetworkSend.SendPlayerRequestItemDestroy(instanceID);
        }
    }

    public void DestroyItem(int instanceID)
    {
        if (instanceItems.ContainsKey(instanceID) == false)
            return;

        PhysicalItem item = instanceItems[instanceID];
        OnItemMove -= item.ItemMove;
        OnItemRotate -= item.ItemRotate;
        instanceItems.Remove(instanceID);
        item.Destroy();
    }

}
