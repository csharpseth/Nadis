﻿using System;
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
        public GameObject remotePlayerPrefab;
        public GameObject unitPrefab;

        private Queue<PacketPlayerConnection> playersToSpawn;
        private Queue<PacketUnitData> unitsToSpawn;

        public Vector3 spawnLocation;

        private void Init()
        {
            playersToSpawn = new Queue<PacketPlayerConnection>();
            unitsToSpawn = new Queue<PacketUnitData>();
        }

        private void Update()
        {
            if(unitsToSpawn != null && unitsToSpawn.Count > 0)
            {
                PacketUnitData data = unitsToSpawn.Dequeue();
                GameObject go = Instantiate(instance.unitPrefab, data.location, Quaternion.identity);
                INetworkInitialized[] init = go.GetComponentsInChildren<INetworkInitialized>();
                for (int i = 0; i < init.Length; i++)
                {
                    init[i].InitFromNetwork(data.unitID);
                }
            }


            if (playersToSpawn == null || playersToSpawn.Count == 0) return;

            int amount = playersToSpawn.Count;
            for (int i = 0; i < amount; i++)
            {
                PacketPlayerConnection temp = playersToSpawn.Dequeue();
                if (temp.playerIsLocal)
                    SpawnLocalPlayer(temp);
                else
                    SpawnRemotePlayer(temp);
            }
        }

        private void SpawnLocalPlayer(PacketPlayerConnection data)
        {
            GameObject ply = Instantiate(playerPrefab, spawnLocation, Quaternion.identity);
            Vector3 tempRot = ply.transform.eulerAngles;
            tempRot.y = data.playerRotation;
            ply.transform.eulerAngles = tempRot;
            ply.name = "Ply_" + data.playerID;
            
            NetData.LocalPlayerID = data.playerID;
            Client.Client.Local.SetID(data.playerID);

            INetworkInitialized[] netInit = ply.GetComponentsInChildren<INetworkInitialized>();
            NetworkedPlayer netPly = ply.GetComponent<NetworkedPlayer>();
            for (int y = 0; y < netInit.Length; y++)
            {
                netInit[y].InitFromNetwork(data.playerID);
            }
            PlayerManager.CreatePlayerStatData(netPly, data.currentHealth, data.maxHealth, data.currentPower, data.maxPower);
        }

        private void SpawnRemotePlayer(PacketPlayerConnection data)
        {
            GameObject ply = Instantiate(remotePlayerPrefab, data.playerPosition, Quaternion.identity);
            Vector3 tempRot = ply.transform.eulerAngles;
            tempRot.y = data.playerRotation;
            ply.transform.eulerAngles = tempRot;
            ply.name = "Remote_Ply_" + data.playerID;
            
            IDisableIfRemotePlayer[] disable = ply.GetComponentsInChildren<IDisableIfRemotePlayer>();
            for (int i = 0; i < disable.Length; i++)
            {
                disable[i].Disable(true);
            }

            INetworkInitialized[] netInit = ply.GetComponentsInChildren<INetworkInitialized>();
            NetworkedPlayer netPly = ply.GetComponent<NetworkedPlayer>();
            for (int y = 0; y < netInit.Length; y++)
            {
                netInit[y].InitFromNetwork(data.playerID);
            }
            if (netPly != null)
            {
                PlayerManager.CreatePlayerStatData(netPly, data.currentHealth, data.maxHealth, data.currentPower, data.maxPower);
            }
        }

        public static void SpawnPlayer(PacketPlayerConnection playerToSpawn)
        {
            instance.playersToSpawn.Enqueue(playerToSpawn);
        }

        public static void SpawnUnit(PacketUnitData unitToSpawn)
        {
            instance.unitsToSpawn.Enqueue(unitToSpawn);
        }
    }
}
