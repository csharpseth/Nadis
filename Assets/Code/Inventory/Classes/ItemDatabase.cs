using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Item Database", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField]
    private GameObject[] items;

    private int ClampInRange(int index)
    {
        return Mathf.Clamp(index, 0, items.Length - 1);
    }

    public ItemDatabaseFetch GetItem(int itemID)
    {
        int index = ClampInRange(itemID);
        ItemDatabaseFetch fetch = new ItemDatabaseFetch
        {
            ItemID = index,
            ItemObject = items[index]
        };
        return fetch;
    }
}

public struct ItemDatabaseFetch
{
    public int ItemID;
    public GameObject ItemObject;
}
