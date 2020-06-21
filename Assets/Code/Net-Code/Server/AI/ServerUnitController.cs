using UnityEngine;
using Unity.Mathematics;
using Nadis.Net.Server;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

public class ServerUnitController : MonoBehaviour
{
    #region Singleton
    private static ServerUnitController instance;
    private void Awake()
    {
        if (instance != null) Destroy(this);
        instance = this;
    }
    #endregion

    private static ServerUnitReferenceData[] refUnitDatas;
    private static ServerUnitData[] unitDatas;

    public GameObject unitPrefab;
    public int numUnits = 10;
    public float updateDestinationDelay = 0.5f;

    public static void Init()
    {
        refUnitDatas = new ServerUnitReferenceData[instance.numUnits];
        unitDatas = new ServerUnitData[instance.numUnits];

        for (int i = 0; i < instance.numUnits; i++)
        {
            float3 spawnPoint = ServerData.GetRandomPlayerSpawnLocation();
            ServerUnitReferenceData refData = new ServerUnitReferenceData(Instantiate(instance.unitPrefab, spawnPoint, Quaternion.identity).transform);
            refUnitDatas[i] = refData;
            ServerUnitData data = new ServerUnitData
            {
                position = refData.Position,
                destination = float3.zero,
                agro = false,
                attack = false,
                closestPlayerID = -1,
                timeLastAttacked = 0f,
                timeLastUpdatedDestination = 0f
            };
            unitDatas[i] = data;
        }

    }

    private void Update()
    {
        UpdateUnitData();
        NativeArray<ServerUnitData> units = new NativeArray<ServerUnitData>(unitDatas, Allocator.TempJob);

        units.Dispose();
    }

    private void UpdateUnitData()
    {
        for (int i = 0; i < refUnitDatas.Length; i++)
        {
            ServerUnitData data = unitDatas[i];
            data.position = refUnitDatas[i].Position;
            unitDatas[i] = data;
        }
    }


    [BurstCompile]
    struct UnitDistanceToClosestPlayerJob : IJobParallelFor
    {
        public NativeArray<ServerUnitData> units;
        [ReadOnly]
        public NativeArray<float3> playerPositions;
        [ReadOnly]
        public NativeArray<int> playerIDs;

        public void Execute(int index)
        {
            ServerUnitData data = units[index];

            int closestPlayerIndex = -1;
            float distance = 10000f;
            for (int i = 0; i < playerPositions.Length; i++)
            {
                float dist = math.distance(playerPositions[i], data.position);
                if(dist < distance)
                {
                    distance = dist;
                    closestPlayerIndex = i;
                }
            }

            data.distanceToClosestPlayer = distance;
            if (closestPlayerIndex != -1)
                data.closestPlayerID = playerIDs[closestPlayerIndex];
            else
                data.closestPlayerID = -1;


            units[index] = data;
        }
    }

    

}
