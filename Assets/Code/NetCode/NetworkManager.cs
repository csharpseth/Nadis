using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager ins;
    public static PlayerSync LocalPlayer;
    [HideInInspector]
    public NetworkObjectsManager netObjectsManager;

    [SerializeField]
    private List<Vector3> spawnPoints;
    private MapGenerator mapGenerator;

    private void Awake()
    {
        if (ins != null)
            Destroy(this);

        ins = this;

        mapGenerator = FindObjectOfType<MapGenerator>();

        Events.Player.OnGetPlayerSync += GetPlayer;
        Events.Player.OnGetPlayerAnimator += GetPlayerAnimator;
    }

    public GameObject nonLocalPlayerPrefab;
    public GameObject localPlayerPrefab;

    public Dictionary<int, PlayerSync> connectedPlayers = new Dictionary<int, PlayerSync>();

    private bool spawnPointsGenerated = false;
    private int localPlayerID = -1;

    
    
    void Start()
    {
        DontDestroyOnLoad(this);

        netObjectsManager = GetComponent<NetworkObjectsManager>();

        NetworkConfig.InitNetwork();
        //SessionFinder.ins.FindSession(NetworkConfig.ConnectToServer, 2f, Debug.LogError);
        NetworkConfig.ConnectToServer(new ServerData("45.79.9.19", 5555));
    }

    public void CreateLocalPlayer(int connID, int inventorySize)
    {
        if (LocalPlayer != null) return;

        Vector3 spawnPoint = Vector3.zero;
        if(spawnPoints.Count > 0)
        {
            spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
        }

        LocalPlayer = Instantiate(localPlayerPrefab).GetComponent<PlayerSync>();
        LocalPlayer.transform.position = spawnPoint;

        if (LocalPlayer.Local == false)
            Debug.LogError("Spawned Local Player is Not Set as 'Local'!");
        else
        {
            LocalPlayer.ID = connID;
            InteractionController.ins.InitInventory(inventorySize);
        }
    }
    
    public void RegisterSpawnPoint(Vector3 point)
    {
        if (spawnPoints == null)
            spawnPoints = new List<Vector3>();

        spawnPoints.Add(point);
    }

    public void CreateNonLocalPlayer(int connID, int inventorySize)
    {
        if (connectedPlayers.ContainsKey(connID))
            return;

        PlayerSync ps = Instantiate(nonLocalPlayerPrefab).GetComponent<PlayerSync>();
        ps.ID = connID;
        ps.SetInventorySize(inventorySize);

        connectedPlayers.Add(connID, ps);

    }

    public void DestroyNonLocalPlayer(int playerID)
    {
        if(connectedPlayers.ContainsKey(playerID) == false)
        {
            return;
        }

        Destroy(connectedPlayers[playerID].gameObject);
        connectedPlayers.Remove(playerID);

    }

    public void SetPlayerPosition(int playerID, Vector3 newPos)
    {
        if (connectedPlayers.ContainsKey(playerID) == false)
            return;

        connectedPlayers[playerID].SetPosition(newPos);
    }

    public void SetPlayerRotation(int playerID, Vector3 newRot)
    {
        if (connectedPlayers.ContainsKey(playerID) == false)
            return;

        connectedPlayers[playerID].SetRotation(newRot);
    }
    
    public void SetMapGeneratorData(int seed)
    {
        if (mapGenerator != null)
        {
            mapGenerator.Generate(seed);
        }
    }

    public void SetPlayerMoveData(int playerID, bool grounded, Vector2 inputDir, PlayerMoveState moveState, float moveSpeed)
    {
        if(connectedPlayers.ContainsKey(playerID) == false)
        {
            Debug.Log("Error No Player w/ ID:" + playerID + " was found to apply MoveData!");
            return;
        }

        connectedPlayers[playerID].SetProceduralMoveData(grounded, inputDir, moveState, moveSpeed);
    }

    public void UpdateInventory(int playerID, int[] ids)
    {
        if (connectedPlayers.ContainsKey(playerID) == false)
            return;

        PlayerSync ply = connectedPlayers[playerID];
        ply.UpdateInventory(ids);
    }

    public PlayerSync GetPlayer(int playerID)
    {
        if (connectedPlayers.ContainsKey(playerID) == false)
            return null;

        return connectedPlayers[playerID];
    }
    public BipedProceduralAnimator GetPlayerAnimator(int playerID)
    {
        PlayerSync ply = GetPlayer(playerID);
        if (ply == null && playerID == LocalPlayer.ID)
            ply = LocalPlayer;


        if (ply != null)
            return ply.GetComponent<BipedProceduralAnimator>();

        return null;
    }
    
    private void OnDrawGizmos()
    {
        if(spawnPoints != null && spawnPoints.Count > 0)
        {
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(spawnPoints[i], 1.5f);
            }
        }
    }

    private void OnApplicationQuit()
    {
        NetworkConfig.DisconnectFromServer();
    }
}


