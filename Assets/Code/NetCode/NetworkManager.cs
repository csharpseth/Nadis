using System.Collections.Generic;
using Nadis.Net.Foundation;
using UnityEngine;

namespace Nadis.Net
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager ins;
        public static PlayerSync LocalPlayer;
        public static PlayerStats DefaultStats;
        private static bool defaultPlayerStatSet = false;
        
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
            Events.Player.GetLocalID = GetLocalPlayerID;
            Events.Player.GetInventory = GetPlayerInventory;

            Events.PlayerStats.SetDefaults = SetDefaultStats;

            Events.Player.Respawn += RespawnPlayer;
            Events.Player.Create += CreatePlayer;
            Events.Player.Disconnect += PlayerDisconnect;
            Events.Player.CreateRagdoll += CreatePlayerRagdoll;

            Events.MapGenerator.RegisterSpawnPoint += RegisterSpawnPoint;
        }

        public GameObject localPlayerPrefab;
        public GameObject remotePlayerPrefab;
        public GameObject playerRagdoll;
        public float ragdollCleanupDelay = 30f;

        public Dictionary<int, PlayerSync> connectedPlayers = new Dictionary<int, PlayerSync>();
        private bool spawnPointsGenerated = false;


        void Start()
        {
            DontDestroyOnLoad(this);
            
            NetworkConfig.InitNetwork();
            //SessionFinder.ins.FindSession(NetworkConfig.ConnectToServer, 2f, Debug.LogError);
            NetworkConfig.ConnectToServer(new ServerData("127.0.0.1", 5555));
        }

        private void SetDefaultStats(PlayerStats stats)
        {
            if (defaultPlayerStatSet == true) return;

            DefaultStats = stats;
            defaultPlayerStatSet = true;
        }

        private int GetLocalPlayerID()
        {
            return LocalPlayer.ID;
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

        private void CreatePlayer(int connID, int inventorySize, bool local)
        {
            PlayerSync ps = null;

            if (GetPlayer(connID) != null) return;

            if (local)
            {
                ps = Instantiate(localPlayerPrefab).GetComponent<PlayerSync>();
                ps.transform.position = GetSpawnPoint();

                LocalPlayer = ps;
            }
            else
            {
                ps = Instantiate(remotePlayerPrefab).GetComponent<PlayerSync>();
            }

            ps.GetComponent<Inventory>().Init(connID, inventorySize);
            ps.GetComponent<PlayerStatsController>().InitFromServer(connID, DefaultStats.MaxHealth, DefaultStats.Health, DefaultStats.MaxPower, DefaultStats.Power);
            ps.GetComponent<PlayerSoundController>().InitFromServer(connID);
            ps.GetComponent<BipedProceduralAnimator>().InitialSetup(connID);
            ps.ID = connID;
            ps.name = "ply_" + connID;

            connectedPlayers.Add(connID, ps);

            if(ps == LocalPlayer)
            {
                NetworkSend.SendLocalPlayerSpawned();
            }
        }

        private void RegisterSpawnPoint(Vector3 point)
        {
            if (spawnPoints == null)
                spawnPoints = new List<Vector3>();

            spawnPoints.Add(point);
        }

        private void CreatePlayerRagdoll(Vector3 pos, Quaternion rot)
        {
            GameObject temp = Instantiate(playerRagdoll, pos, rot);
            Destroy(temp, ragdollCleanupDelay);
        }

        private void PlayerDisconnect(int playerID)
        {
            connectedPlayers.Remove(playerID);
            Debug.Log("Player Has Disconnected");
        }
        
        private PlayerSync GetPlayer(int playerID)
        {
            if (connectedPlayers.ContainsKey(playerID) == false)
                return null;

            return connectedPlayers[playerID];
        }
        private Inventory GetPlayerInventory(int playerID)
        {
            PlayerSync ply = GetPlayer(playerID);
            if (ply == null && playerID == LocalPlayer.ID)
                ply = LocalPlayer;
            if (ply != null)
                return ply.GetComponent<Inventory>();

            return null;
        }
        private BipedProceduralAnimator GetPlayerAnimator(int playerID)
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
}


