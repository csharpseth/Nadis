using UnityEngine;
using System.Collections.Generic;

namespace Nadis.Net.Server
{
    public class ServerChargingController : MonoBehaviour
    {
        public static ServerChargingController instance;
        private static List<PlayerChargingItem> chargeIDs;

        private void Awake() 
        {
            if(instance != null) Destroy(this);

            instance = this;
        }

        private void Update()
        {
            if(chargeIDs == null || chargeIDs.Count == 0) return;

            for(int i = 0; i < chargeIDs.Count; i++)
            {

                PlayerChargingItem ply = chargeIDs[i];
                ply.time += Time.deltaTime;

                if(ply.time >= ServerData.PlayerChargeDelay)
                {
                    ply.time = 0f;
                    //Adjust Player Power Level
                    ClientManager.AlterPlayerPower(ply.playerID, ServerData.PlayerChargeAmountPerDelay);
                }
                chargeIDs[i] = ply;
            }
        }

        private static void EnsureListPrescence()
        {
            if(chargeIDs == null) chargeIDs = new List<PlayerChargingItem>();
        }

        public static void EvaluatePlayer(int playerID, Vector3 location)
        {
            ServerClientData client = ClientManager.GetClient(playerID);
            if(client == null) return;

            EnsureListPrescence();

            float dist = 10000f;
            Vector3 bufferPos;
            for(int i = 0; i < ServerData.chargingStationLocations.Length; i++)
            {
                bufferPos = ServerData.chargingStationLocations[i];
                float newDist = Util.FastAndRoughDistance(location, bufferPos);
                if(newDist < dist)
                {
                    dist = newDist;
                }
            }
            int playerIndex = IndexOfPlayer(playerID);

            if(playerIndex == -1)
            {
                if(dist <= ServerData.ChargeDistance)
                {
                    chargeIDs.Add(new PlayerChargingItem(playerID));
                }
            }else
            {
                if(dist > ServerData.ChargeDistance)
                {
                    chargeIDs.RemoveAt(playerIndex);
                }
            }

        }

        public static int IndexOfPlayer(int playerID)
        {
            for(int i = 0; i < chargeIDs.Count; i++)
            {
                if(chargeIDs[i].playerID == playerID)
                    return i;
            }

            return -1;
        }

        struct PlayerChargingItem
        {
            public int playerID;
            public float time;

            public PlayerChargingItem(int playerID)
            {
                this.playerID = playerID;
                time = 0f;
            }
        }
    }

    
}