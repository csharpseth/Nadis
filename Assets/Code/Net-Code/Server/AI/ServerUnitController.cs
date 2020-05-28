using Nadis.Net;
using Nadis.Net.Client;
using Nadis.Net.Server;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class ServerUnitController : MonoBehaviour
{
    private static ServerUnitController instance;
    public GameObject unitPrefab;
    public LayerMask spawnableRegionLayer;
    public float distanceToFindNextDestination = 3f;
    public bool drawGizmos = true;
    public float attackDistance = 3f;
    public float attackDelay = 2f;
    public int damage = 10;

    private static List<ServerUnitData> units;
    private static NativeList<float3> unitPositions;
    private static float[] timesLastAttacked;

    private static int numberOfUnits;

    private void Awake()
    {
        if (instance != null)
            Destroy(this);

        instance = this;
    }

    private void Update()
    {
        if (units == null || units.Count == 0) return;

        #region Updating Unit Positions
        //Getting up to date unit positions;
        for (int i = 0; i < units.Count; i++)
        {
            unitPositions[i] = units[i].Location;
        }
        #endregion

        NativeList<float3> playerPositions = new NativeList<float3>(Allocator.TempJob);
        #region Player Position Fetching
        List<int> clientIDs = ClientManager.Clients;
        for (int i = 0; i < clientIDs.Count; i++)
        {
            ServerClientData client = ClientManager.GetClient(clientIDs[i]);
            if (client == null) continue;

            playerPositions.Add(client.position);
        }
        #endregion

        NativeArray<DistanceData> distances = new NativeArray<DistanceData>(unitPositions.Length, Allocator.TempJob);
        #region Distance Job
        LowestPlayerToUnitDistanceJob distJob = new LowestPlayerToUnitDistanceJob
        {
            unitPositions = unitPositions,
            playerPositions = playerPositions,
            sqrDistances = distances
        };
        JobHandle distHandle = distJob.Schedule(unitPositions.Length, 10);
        distHandle.Complete();
        #endregion


        distances = distJob.sqrDistances;
        for (int i = 0; i < distances.Length; i++)
        {
            float timeSinceAttack = Time.realtimeSinceStartup - timesLastAttacked[i];
            if (timeSinceAttack < attackDelay) continue;

            if (distances[i].sqrDistance <= 0f || distances[i].sqrDistance > (attackDistance * attackDistance)) continue;

            ClientManager.DamagePlayer(clientIDs[i], damage);
            timesLastAttacked[i] = Time.realtimeSinceStartup;
        }

        NativeArray<TargetData> unitTargets = new NativeArray<TargetData>(unitPositions.Length, Allocator.TempJob);
        #region Targeting Job
        FindTargetJob job = new FindTargetJob
        {
            unitPositions = unitPositions,
            playerTargetPositions = playerPositions,
            output = unitTargets
        };
        JobHandle handle = job.Schedule(unitPositions.Length, 10);
        handle.Complete();
        #endregion

        #region Applying Targeting Job To Units
        unitTargets = job.output;
        for (int i = 0; i < unitTargets.Length; i++)
        {
            if (unitTargets[i].targetFound == false) continue;

            units[i].SetDestination(unitTargets[i].targetLocation);
        }
        #endregion

        


        #region Disposal
        playerPositions.Dispose();
        unitTargets.Dispose();
        distances.Dispose();
        #endregion
    }

    private void Wander()
    {
        float distToFindNext = (distanceToFindNextDestination * distanceToFindNextDestination);

        for (int i = 0; i < units.Count; i++)
        {
            ServerUnitData data = units[i];
            Vector3 pos = data.Location;
            float dist = (data.Destination - pos).sqrMagnitude;
            if (dist <= distanceToFindNextDestination)
            {
                //Find next destination
                data.needsDestination = true;
            }

            units[i] = data;
        }

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i].needsDestination)
            {
                units[i].SetDestination(FindDestination(units[i].Location, units[i].wanderRadius, instance.spawnableRegionLayer));
                units[i].needsDestination = false;
            }
        }
    }

    [BurstCompile]
    struct FindTargetJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeList<float3> unitPositions;
        [ReadOnly]
        public NativeList<float3> playerTargetPositions;

        public NativeArray<TargetData> output;

        public void Execute(int index)
        {
            Vector3 pos = unitPositions[index];

            float lowestDist = 10000f;
            int closestPlayerIndex = -1;

            for (int i = 0; i < playerTargetPositions.Length; i++)
            {
                float dist = math.distancesq(playerTargetPositions[i], pos);
                if (dist >= lowestDist) continue;

                lowestDist = dist;
                closestPlayerIndex = i;
            }

            TargetData data = output[index];

            data.targetFound = (closestPlayerIndex != -1);
            if(data.targetFound)
                data.targetLocation = playerTargetPositions[closestPlayerIndex];

            output[index] = new TargetData();
            output[index] = data;
        }
    }

    [BurstCompile]
    struct TargetData
    {
        public bool targetFound;
        public float3 targetLocation;
    }

    [BurstCompile]
    struct LowestPlayerToUnitDistanceJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeList<float3> unitPositions;
        [ReadOnly]
        public NativeArray<float3> playerPositions;
        public NativeArray<DistanceData> sqrDistances;

        public void Execute(int index)
        {
            float3 unitPos = unitPositions[index];
            int lowestPlyIndex = -1;
            float lowestDistance = 100000f;
            for (int i = 0; i < playerPositions.Length; i++)
            {
                float3 plyPos = playerPositions[i];
                float dist = math.distancesq(plyPos, unitPos);
                if (dist < lowestDistance)
                {
                    lowestPlyIndex = i;
                    lowestDistance = dist;
                }
            }

            DistanceData data = new DistanceData
            {
                sqrDistance = lowestDistance,
                plyIndex = lowestPlyIndex
            };
            sqrDistances[index] = data;
        }
    }

    private static Vector3 FindDestination(Vector3 origin, float range, LayerMask groundMask)
    {
        Vector3 randPoint = (UnityEngine.Random.insideUnitSphere * range) + origin + Vector3.up * 100f;
        Vector3 destination = origin;
        RaycastHit hit;
        if(Physics.Raycast(randPoint, Vector3.down, out hit, groundMask))
            destination = hit.point;

        return destination;
    }

    public static void Initialize(int numUnits)
    {
        units = new List<ServerUnitData>();
        unitPositions = new NativeList<float3>(Allocator.Persistent);
        timesLastAttacked = new float[numUnits];

        numberOfUnits = numUnits;
        for (int i = 0; i < numberOfUnits; i++)
        {
            Vector3 spawnPoint = ServerData.playerSpawnLocations[UnityEngine.Random.Range(0, ServerData.playerSpawnLocations.Length)];
            Vector3 origin = spawnPoint + (Vector3.up * ServerData.spawnRadius) + UnityEngine.Random.insideUnitSphere * ServerData.spawnRadius;

            RaycastHit[] hits = null;
            Physics.RaycastNonAlloc(origin, Vector3.down, hits, 10f, instance.spawnableRegionLayer);

            if(hits != null)
                spawnPoint = hits[0].point;
            ServerUnitData data = null;
            GameObject go = Instantiate(instance.unitPrefab, spawnPoint, Quaternion.identity, instance.transform);
            go.TryGetComponent(out data);
            if (data == null) { Destroy(go); continue; }
            INetworkInitialized[] init = go.GetComponentsInChildren<INetworkInitialized>();
            foreach (INetworkInitialized item in init)
                item.InitFromNetwork(i);

            units.Add(data);
            unitPositions.Add(spawnPoint);
            timesLastAttacked[i] = 0f;
        }


    }

    public static void SendPlayerUnits(int playerID)
    {
        for (int i = 0; i < units.Count; i++)
        {
            Vector3 location = units[i].Location;

            PacketUnitData packet = new PacketUnitData
            {
                unitID = i,
                location = location
            };
            ServerSend.ReliableToOne(packet, playerID);
        }
    }

    Vector3 bounds = new Vector3(0.5f, 1.5f, 0.5f);
    private void OnDrawGizmos()
    {
        if (units == null || units.Count == 0 || drawGizmos == false) return;

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] == null) continue;

            Vector3 position = new Vector3(0f, 0.75f, 0f);
            position += units[i].Location;

            Gizmos.DrawWireCube(position, bounds);
        }
    }

    private void OnDestroy()
    {
        unitPositions.Dispose();
    }

}

public struct DistanceData
{
    public float sqrDistance;
    public int plyIndex;
}
