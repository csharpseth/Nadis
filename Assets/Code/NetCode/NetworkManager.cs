using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager ins;
    public static PlayerSync LocalPlayer;
    public static PlayerStats DefaultStats;
    private static bool defaultPlayerStatSet = false;

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

        Events.Player.GetPlayerSync = GetPlayer;
        Events.Player.GetPlayerAnimator = GetPlayerAnimator;
        Events.Inventory.GetInventory = GetPlayerInventory;

        Events.Player.Respawn += RespawnPlayer;
    }

    public GameObject localPlayerPrefab;
    public GameObject remotePlayerPrefab;
    public GameObject playerRagdoll;
    public float ragdollCleanupDelay = 30f;

    public Dictionary<int, PlayerSync> connectedPlayers = new Dictionary<int, PlayerSync>();

    private bool spawnPointsGenerated = false;
    private int localPlayerID = -1;
    

    void Start()
    {
        DontDestroyOnLoad(this);

        netObjectsManager = GetComponent<NetworkObjectsManager>();

        NetworkConfig.InitNetwork();
        //SessionFinder.ins.FindSession(NetworkConfig.ConnectToServer, 2f, Debug.LogError);
        NetworkConfig.ConnectToServer(new ServerData("127.0.0.1", 5555));
    }

    public void SetDefaultStats(PlayerStats stats)
    {
        if (defaultPlayerStatSet == true) return;

        DefaultStats = stats;
        defaultPlayerStatSet = true;
    }

    private void RespawnPlayer(int playerID)
    {
        if (playerID != LocalPlayer.ID) return;

        LocalPlayer.transform.position = GetSpawnPoint();
    }

    private Vector3 GetSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return Vector3.zero;

        int index = Random.Range(0, spawnPoints.Count);
        return spawnPoints[index];
    }

    public void CreatePlayer(int connID, int inventorySize, bool local)
    {
        PlayerSync ps = null;

        if (GetPlayer(connID) != null) return;

        if(local)
        {
            ps = Instantiate(localPlayerPrefab).GetComponent<PlayerSync>();
            ps.transform.position = GetSpawnPoint();

            LocalPlayer = ps;
        }else
        {
            ps = Instantiate(remotePlayerPrefab).GetComponent<PlayerSync>();
        }

        ps.ID = connID;
        ps.GetComponent<Inventory>().Init(connID, inventorySize);
        ps.GetComponent<PlayerStatsController>().InitFromServer(connID, DefaultStats.MaxHealth, DefaultStats.Health, DefaultStats.MaxPower, DefaultStats.Power);

        connectedPlayers.Add(connID, ps);
    }
    
    public void RegisterSpawnPoint(Vector3 point)
    {
        if (spawnPoints == null)
            spawnPoints = new List<Vector3>();

        spawnPoints.Add(point);
    }
    
    public void CreatePlayerRagdoll(Vector3 pos, Quaternion rot)
    {
        GameObject temp = Instantiate(playerRagdoll, pos, rot);
        Destroy(temp, ragdollCleanupDelay);
    }

    public void DestroyRemotePlayer(int playerID)
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
    
    public PlayerSync GetPlayer(int playerID)
    {
        if (connectedPlayers.ContainsKey(playerID) == false)
            return null;

        return connectedPlayers[playerID];
    }
    public Inventory GetPlayerInventory(int playerID)
    {
        PlayerSync ply = GetPlayer(playerID);
        if (ply == null && playerID == LocalPlayer.ID)
            ply = LocalPlayer;
        if (ply != null)
            return ply.GetComponent<Inventory>();

        return null;
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

    private void OnApplicationQuit()
    {
        NetworkConfig.DisconnectFromServer();
    }
}


