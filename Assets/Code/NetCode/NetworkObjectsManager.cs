using UnityEngine;
using System;
using System.Collections.Generic;

public class NetworkObjectsManager : MonoBehaviour
{
    [SerializeField]
    private List<NetworkObject> registry = new List<NetworkObject>();
    private Dictionary<int, NetworkObject> spawned = new Dictionary<int, NetworkObject>();

    private Action<int, NetworkObjectsManager> NetDestroy;
    private Action<int, Vector3> NetMove;
    private Action<int, Vector3> NetRotate;

    //This function allows you to spawn
    public NetworkObject Spawn(int registryID, int spawnID)
    {
        NetworkObject no = null;
        for (int i = 0; i < registry.Count; i++)
        {
            if (registry[i].registryID == registryID)
            {
                no = Instantiate(registry[i].prefab).GetComponent<NetworkObject>();
                no.SpawnID = spawnID;

                NetMove += no.OnNetMove;
                NetDestroy += no.OnNetDestroy;

                spawned.Add(spawnID, no);

                break;
            }
        }

        return no;
    }

    public void DestroyNetObject(int id)
    {
        if (spawned.ContainsKey(id) == false)
            return;

        NetDestroy(id, this);
    }

    public void MoveNetObject(int id, Vector3 newPos)
    {
        if (spawned.ContainsKey(id) == false)
            return;

        NetMove(id, newPos);

    }

    public void RotateNetObject(int id, Vector3 newRot)
    {
        if (spawned.ContainsKey(id) == false)
            return;

        NetRotate(id, newRot);
    }

    public void RemoveFromEvents(NetworkObject netObject, int id)
    {
        NetDestroy -= netObject.OnNetDestroy;
        NetMove -= netObject.OnNetMove;
        spawned.Remove(id);
    }

}
