using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager ins;
    public ItemDatabase itemDatabase;
    public Dictionary<int, PhysicalItem> instanceItems;
    
    private void Awake()
    {
        if (ins == null)
        {
            ins = this;
            instanceItems = new Dictionary<int, PhysicalItem>();
        }
    }
    
    public void SpawnItem(int itemID, int instanceID, Vector3 pos, Vector3 rot)
    {
        PhysicalItem item = itemDatabase.SpawnItem(itemID, pos, rot);
        item.SetInstanceID(instanceID);
        
        instanceItems.Add(instanceID, item);

    }
    
    public void DestroyItem(int instanceID)
    {
        if (instanceItems.ContainsKey(instanceID) == false)
            return;

        PhysicalItem item = instanceItems[instanceID];
        instanceItems.Remove(instanceID);
        item.Destroy();
    }

}
