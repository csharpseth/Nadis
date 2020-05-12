using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nadis.Net
{
    public class PlayerPopulatorSystem : MonoBehaviour
    {
        #region Singleton
        public static PlayerPopulatorSystem instance;
        private void Awake()
        {
            if (instance == null) instance = this;
            else Destroy(this);
            Init();
        }
        #endregion


        public GameObject playerPrefab;
        private Queue<PacketPlayerConnection> playersToSpawn;

        private void Init()
        {
            playersToSpawn = new Queue<PacketPlayerConnection>();
        }

        private void Update()
        {
            if (playersToSpawn == null || playersToSpawn.Count == 0) return;

            int amount = playersToSpawn.Count;
            for (int i = 0; i < amount; i++)
            {
                PacketPlayerConnection temp = playersToSpawn.Dequeue();
                GameObject ply = Instantiate(playerPrefab, temp.playerPosition, Quaternion.identity);
                Vector3 tempRot = ply.transform.eulerAngles;
                tempRot.y = temp.playerRotation;
                ply.transform.eulerAngles = tempRot;
                ply.name = "Ply_" + temp.playerID;

                Log.Txt("Player({0}) isLocal:{1}", temp.playerID, temp.playerIsLocal);

                if (temp.playerIsLocal == false)
                {
                    IDisableIfRemotePlayer[] disable = ply.GetComponentsInChildren<IDisableIfRemotePlayer>();
                    for (int j = 0; j < disable.Length; j++)
                    {
                        disable[j].Disable(true);
                    }
                }else
                {
                    NetData.LocalPlayerID = temp.playerID;
                    Client.Client.Local.SetID(temp.playerID);
                    Debug.Log(NetData.LocalPlayerID);
                }
                INetworkInitialized[] netInit = ply.GetComponentsInChildren<INetworkInitialized>();
                NetworkedPlayer netPly = ply.GetComponent<NetworkedPlayer>();
                for (int y = 0; y < netInit.Length; y++)
                {
                    Debug.Log("Net Init");
                    netInit[y].InitFromNetwork(temp.playerID);
                }
                if(netPly != null)
                {
                    PlayerManager.CreatePlayersStatData(netPly, temp.currentHealth, temp.maxHealth);
                }


            }
        }

        public static void SpawnPlayer(PacketPlayerConnection playerToSpawn)
        {
            instance.playersToSpawn.Enqueue(playerToSpawn);
        }
    }
}
