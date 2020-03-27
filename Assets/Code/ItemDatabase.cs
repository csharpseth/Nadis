using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item DB", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public GameObject[] items;

    public PhysicalItem SpawnItem(int id, Vector3 pos = default(Vector3), Vector3 rot = default(Vector3))
    {
        if (id > (items.Length - 1) || id < 0)
            return null;

        PhysicalItem item = Instantiate(items[id], pos, Quaternion.Euler(rot)).GetComponent<PhysicalItem>();
        item.meta.id = id;
        return item;

    }

    public PhysicalItem SpawnRandomItem(Vector3 pos = default(Vector3), Vector3 rot = default(Vector3))
    {
        int index = Random.Range(0, (items.Length - 1));
        return SpawnItem(index, pos, rot);
    }

}
