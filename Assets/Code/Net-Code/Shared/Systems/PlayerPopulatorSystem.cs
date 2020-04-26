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
        public static PlayerPopulatorSystem instance;

        public GameObject playerPrefab;
        private Queue<int> playersToSpawn;


        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                playersToSpawn = new Queue<int>();
            }
            else
                Destroy(this);
        }

        private void Update()
        {
            if (playersToSpawn == null || playersToSpawn.Count == 0) return;

            int id = playersToSpawn.Dequeue();
            if (NetData.LocalPlayerID == -1)
                NetData.LocalPlayerID = id;

            GameObject player = Instantiate(playerPrefab);

            //Check if the player you are spawning is local
            if(NetData.LocalPlayerID != id)
            {
                IDisableIfRemotePlayer[] disable = player.GetComponentsInChildren<IDisableIfRemotePlayer>();
                for (int i = 0; i < disable.Length; i++)
                {
                    disable[i].Disable();
                }
            }
            INetworkInitialized[] networkInitialized = player.GetComponentsInChildren<INetworkInitialized>();
            for (int i = 0; i < networkInitialized.Length; i++)
            {
                networkInitialized[i].InitFromNetwork(id);
            }

        }


        public static void CreatePlayer(int clientID)
        {
            instance.playersToSpawn.Enqueue(clientID);
        }
    }
}
