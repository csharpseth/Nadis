using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    public static Spawner instance;
    private Queue<SpawnQueueItem> spawnQueue;

    private void Awake()
    {
        instance = this;
        spawnQueue = new Queue<SpawnQueueItem>();
    }

    private void Update()
    {
        if (spawnQueue == null || spawnQueue.Count == 0) return;

        while(spawnQueue.Count > 0)
        {
            SpawnQueueItem item = spawnQueue.Dequeue();
            GameObject spawnedObject = Instantiate(item.prefab);
            spawnedObject.transform.SetParent(transform);
            item.OnSpawn?.Invoke(spawnedObject);
        }
    }

    public static void Spawn(GameObject prefab, Action<GameObject> callback = null)
    {
        SpawnQueueItem item = new SpawnQueueItem
        {
            prefab = prefab,
            OnSpawn = callback
        };
        instance.spawnQueue.Enqueue(item);
    }
}

public struct SpawnQueueItem
{
    public GameObject prefab;
    public Action<GameObject> OnSpawn;
}
