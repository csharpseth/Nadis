using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class TesterMenu : MonoBehaviour
{
    private static TesterMenu ins;

    NetworkingManager manager;
    public static ulong LocalPlayerID;
    private Dictionary<ulong, NetworkedPlayer> players = new Dictionary<ulong, NetworkedPlayer>();

    private void Awake()
    {
        ins = this;
        manager = FindObjectOfType<NetworkingManager>();
    }

    public void Host()
    {
        if (manager.IsClient) return;
        
        manager.StartHost();
        Hide();
    }

    public void Client()
    {
        if (manager.IsClient) return;
        
        manager.StartClient();
        Hide();
    }
    
    public static void RegisterClient(NetworkedPlayer ply)
    {
        if(ins.players.ContainsKey(ply.ID))
        {
            Debug.LogError("Failed To Register Client: ID already exists");
            return;
        }

        ins.players.Add(ply.ID, ply);
        Debug.Log("Successfully Register Client With ID: " + ply.ID);
    }

    public static GameObject SpawnObject(GameObject go, Vector3 pos = default, Quaternion rot = default)
    {
        GameObject temp = null;

        temp = Instantiate(go, pos, rot);
        NetworkedObject no = temp.GetComponent<NetworkedObject>();
        no.Spawn();

        return temp;
    }

    public static BipedProceduralAnimator GetAnimator(ulong id)
    {
        if (ins.players.ContainsKey(id) == false) return null;
        return ins.players[id].GetComponent<BipedProceduralAnimator>();
    }
    public static NetworkedPlayer GetPlayer(ulong id)
    {
        if (ins.players.ContainsKey(id) == false) return null;
        return ins.players[id];
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
    

}
