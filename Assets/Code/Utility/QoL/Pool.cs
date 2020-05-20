using System.Collections.Generic;
using UnityEngine;

public struct Pool<T>
{
    private Queue<T> _content;
    private bool createNewOnOverflow;
    private int inUse;

    public bool IsNull { get { return _content == null; } }

    public Pool(int size, bool overflow)
    {
        _content = new Queue<T>();
        for (int i = 0; i < size; i++)
        {
            T t = System.Activator.CreateInstance<T>();
            _content.Enqueue(t);
        }

        createNewOnOverflow = overflow;
        inUse = 0;
    }

    public T GetInstance()
    {
        if (inUse >= _content.Count && createNewOnOverflow)
        {
            T t = System.Activator.CreateInstance<T>();
            _content.Enqueue(t);
        }
        else if (createNewOnOverflow && inUse >= _content.Count)
            return default;
        inUse++;
        return _content.Dequeue();
    }
    public void Release(T obj)
    {
        inUse--;
        _content.Enqueue(obj);
    }

}

public class GameObjectPool
{
    private Queue<GameObject> _content;
    private int size;
    private bool autoEnque;

    public GameObjectPool(GameObject prefab, int size = 300, bool autoEnque = true)
    {
        this.size = size;
        this.autoEnque = autoEnque;
        _content = new Queue<GameObject>();
        for (int i = 0; i < size; i++)
        {
            Spawner.Spawn(prefab, (GameObject go) =>
            {
                go.SetActive(false);
                _content.Enqueue(go);
            });
        }
    }
    public GameObjectPool(GameObject[] prefabs, int size = 300, bool autoEnque = true)
    {
        this.size = size;
        this.autoEnque = autoEnque;
        _content = new Queue<GameObject>();
        for (int i = 0; i < size; i++)
        {
            int index = Random.Range(0, prefabs.Length);
            if (index >= prefabs.Length) index = prefabs.Length - 1;

            Spawner.Spawn(prefabs[index], (GameObject temp) =>
            {
                temp.SetActive(false);
                _content.Enqueue(temp);
                Debug.Log("Enqueued");
            });
        }
    }

    public GameObject SpawnAt(Vector3 pos = default, Vector3 rot = default)
    {
        GameObject temp = _content.Dequeue();
        temp.SetActive(false);
        temp.transform.position = pos;
        temp.transform.eulerAngles = rot;
        temp.SetActive(true);
        if (autoEnque)
            _content.Enqueue(temp);
        return temp;
    }
    public void Enque(GameObject go)
    {
        go.SetActive(false);
        _content.Enqueue(go);
    }

}