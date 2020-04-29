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
                GameObject ply = Instantiate(playerPrefab); //Add support for a specific position from the packet
                if(temp.playerIsLocal == false)
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
                }
                INetworkInitialized[] netInit = ply.GetComponentsInChildren<INetworkInitialized>();
                for (int y = 0; y < netInit.Length; y++)
                {
                    netInit[y].InitFromNetwork(temp.playerID);
                }
            }
        }

        public static void SpawnPlayer(PacketPlayerConnection playerToSpawn)
        {
            instance.playersToSpawn.Enqueue(playerToSpawn);
        }
    }
}
