using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace Nadis.Net.Server
{
    public static class ClientManager
    {
        public static List<int> Clients { get; private set; }
        public static bool ClientSlotsOpen => (Clients.Count < _maxClients);

        private static Dictionary<int, ServerClientData> _clientDictionary;
        private static Dictionary<int, ClientStatData> _clientStatsDictionary;
        private static int _maxClients;
        private static int _currentID = 0;
        private static bool initialized = false;

        public static void Init(int maxClients = 15)
        {
            if (initialized) return;

            _maxClients = maxClients;
            _clientDictionary = new Dictionary<int, ServerClientData>();
            _clientStatsDictionary = new Dictionary<int, ClientStatData>();
            Clients = new List<int>();
            initialized = true;
        }

        public static bool ClientExists(int clientID)
        {
            return _clientDictionary.ContainsKey(clientID);
        }

        public static ServerClientData GetClient(int clientID)
        {
            if (ClientExists(clientID) == false) return null;
            return _clientDictionary[clientID];
        }
        public static void DisconnectClient(int clientID, bool send = false)
        {
            if (ClientExists(clientID) == false) return;
            ServerClientData client = GetClient(clientID);
            client.Disconnect();
            int index = -1;
            for (int i = 0; i < Clients.Count; i++)
            {
                if (Clients[i] == clientID) index = i;
            }
            //Send a Disconnect Command to all Clients
            if(send)
            {
                PacketDisconnectPlayer packet = new PacketDisconnectPlayer
                {
                    playerID = clientID
                };
                ServerSend.ReliableToAll(packet, clientID);
            }

            Clients.RemoveAt(index);
            _clientDictionary.Remove(clientID);
            _clientStatsDictionary.Remove(clientID);
        }

        private static int NextID()
        {
            _currentID++;
            return _currentID;
        }

        public static float SqrDistanceBetweenPlayers(int playerA, int playerB)
        {
            if (_clientDictionary.ContainsKey(playerA) == false || _clientDictionary.ContainsKey(playerB) == false) return 0f;

            Vector3 posA = _clientDictionary[playerA].position;
            Vector3 posB = _clientDictionary[playerB].position;
            return (posA - posB).sqrMagnitude;
        }

        public static int TryAddClient(TcpClient socket)
        {
            int clientID = NextID();

            if (ClientExists(clientID))
            {
                Log.Err("Failed To Add Client, ID:{0} is Already In Use", clientID);
                return -1;
            }

            try
            {
                ServerClientData cd = new ServerClientData(socket, clientID);
                _clientDictionary.Add(clientID, cd);
                Clients.Add(clientID);
                ItemManager.CreateInventory(clientID);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return -1;
            }
            
            return clientID;
        }
        public static ClientStatData CreateOrGetClientStatData(int clientID, int startHealth, int maxHealth, int startPower, int maxPower)
        {
            if (_clientStatsDictionary.ContainsKey(clientID) == true) return _clientStatsDictionary[clientID];
            if (_clientDictionary.ContainsKey(clientID) == false) return default;

            ClientStatData stats = new ClientStatData(startHealth, startPower, maxHealth, maxPower);
            _clientStatsDictionary.Add(clientID, stats);

            return stats;
        }
        public static int TryDamagePlayer(PacketRequestDamagePlayer data)
        {
            int playerID = data.playerID;
            int damagerPlayerID = data.damagerPlayerID;
            float playerDistance = SqrDistanceBetweenPlayers(playerID, damagerPlayerID);
            if (playerDistance == 0f || playerDistance > data.weaponRange) return -1;
            if (_clientStatsDictionary.ContainsKey(playerID) == false) return -1;

            int range = data.weaponRange;
            float dmgMultiplier = ServerData.DamageMultiplierFrom(data.limbHit);
            int damage = (int)(data.weaponDamage * dmgMultiplier);
            
            ClientStatData stat = _clientStatsDictionary[playerID];
            stat.health -= damage;

            if (stat.health <= 0)
            {
                ItemManager.PlayerDropAllItems(playerID);
                PacketKillPlayer packet = new PacketKillPlayer{
                    playerID = playerID,
                    respawn = true
                };
                ServerSend.ReliableToAll(packet);
                stat.Reset();
                 _clientStatsDictionary[playerID] = stat;
                return -1;
            }

            _clientStatsDictionary[playerID] = stat;
            return _clientStatsDictionary[playerID].health;
        }

        public static void DamagePlayer(int playerID, int damage)
        {
            ClientStatData stat = _clientStatsDictionary[playerID];
            Debug.Log(stat.health);
            stat.health = stat.health - Util.FastAbs(damage);

            //Debug.Log(stat.health);

            if (stat.health <= 0)
            {
                ItemManager.PlayerDropAllItems(playerID);
                PacketKillPlayer packet = new PacketKillPlayer
                {
                    playerID = playerID,
                    respawn = true
                };
                ServerSend.ReliableToAll(packet);
                stat.Reset();
                _clientStatsDictionary[playerID] = stat;
                Log.Not("SERVER::Killed Player: {0}", playerID);
                return;
            }

            _clientStatsDictionary[playerID] = stat;
            Debug.Log(_clientStatsDictionary[playerID].health);
            PacketAlterPlayerHealth cmd = new PacketAlterPlayerHealth
            {
                playerID = playerID,
                health = stat.health
            };
            //Debug.Log(_clientStatsDictionary[playerID].health);
            ServerSend.ReliableToAll(cmd);

        }

        public static void AlterPlayerPower(int playerID, int amount)
        {
            if(_clientStatsDictionary.ContainsKey(playerID) == false) return;

            //Apply the information to the server-side data
            ClientStatData stat = _clientStatsDictionary[playerID];
            stat.power += amount;

            //Over Charge Check
            if(stat.power > ServerData.PlayerMaxPower)
            {
                //Damage The Player
                stat.numTimesOverCharged += 1;
            }

            if(stat.numTimesOverCharged >= 2)
            {
                DamagePlayer(playerID, ServerData.PlayerOverChargeDamageAmount);
                stat = _clientStatsDictionary[playerID];
                stat.power = ServerData.PlayerMaxPower;
                _clientStatsDictionary[playerID] = stat;
                return;
            }

            if (stat.power / (float)stat.maxPower <= 0.99f)
                stat.numTimesOverCharged = 0;

            _clientStatsDictionary[playerID] = stat;

            //Send the updated level to clients
            PacketAlterPlayerPower packet = new PacketAlterPlayerPower
            {
                playerID = playerID,
                powerLevel = stat.power
            };
            ServerSend.ReliableToAll(packet);
        }

        public static void TransferPower(int playerID, int receiveID)
        {
            if (_clientStatsDictionary.ContainsKey(playerID) == false) return;
            if (_clientStatsDictionary.ContainsKey(receiveID) == false) return;

            ClientStatData ply = _clientStatsDictionary[playerID];

            if (ply.power < 15) return;

            AlterPlayerPower(playerID, -10);
            AlterPlayerPower(receiveID, 10);
        }

        /*
        public static bool CanChargePlayer(int playerID)
        {
            if(_clientStatsDictionary.ContainsKey(playerID) == false) return false;
            int powerLevel = _clientStatsDictionary[playerID].power.Value;
            return powerLevel < ServerData.PlayerMaxPower;
        }
        */

        public static void MoveLosePower(int playerID, Vector3 oldPos, Vector3 newPos)
        {
            //Debug.LogFormat("Player Old Position: {0}  New Position: {1}  Distance: {2}", oldPos, newPos, sqrDistance);
            AlterPlayerPower(playerID, -ServerData.PlayerPowerToLose);
        }
        public static void RemoveClient(int clientID)
        {
            if (ClientExists(clientID) == false)
            {
                Debug.LogErrorFormat("Failed To Remove Client ID:{0}, No Such Client Exists.");
                return;
            }

            try
            {
                _clientDictionary.Remove(clientID);
                for (int i = 0; i < Clients.Count; i++)
                {
                    if (Clients[i] != clientID) continue;

                    Clients.Remove(Clients[i]);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        
        public static void Clear(bool send)
        {
            for (int i = 0; i < Clients.Count; i++)
            {
                int id = Clients[i];
                DisconnectClient(id, send);
            }
        }
    }

    public struct ClientStatData
    {
        public int health;
        public int power;

        public int maxPower;
        public int maxHealth;

        public int startHealth;
        public int startPower;

        public int numTimesOverCharged;

        public ClientStatData(int startHealth, int startPower, int maxHealth, int maxPower)
        {
            this.startHealth = startHealth;
            this.startPower = startPower;
            health = startHealth;
            power = startPower;

            this.maxPower = maxPower;
            this.maxHealth = maxHealth;
            numTimesOverCharged = 0;
        }

        public void Reset()
        {
            power = startPower;
            health = startHealth;
            numTimesOverCharged = 0;
        }
    }
}
