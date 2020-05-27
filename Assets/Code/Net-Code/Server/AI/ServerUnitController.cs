using Nadis.Net;
using Nadis.Net.Client;
using Nadis.Net.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerUnitController : MonoBehaviour
{
    private static ServerUnitController instance;
    public GameObject unitPrefab;
    public LayerMask spawnableRegionLayer;
    public float distanceToFindNextDestination = 3f;
    public bool drawGizmos = true;

    private static List<ServerUnitData> units;
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

        float distToFindNext = (distanceToFindNextDestination * distanceToFindNextDestination);

        for (int i = 0; i < units.Count; i++)
        {
            ServerUnitData data = units[i];
            Vector3 pos = data.Location;
            float dist = (data.Destination - pos).sqrMagnitude;
            if(dist <= distanceToFindNextDestination)
            {
                //Find next destination
                data.needsDestination = true;
            }

            units[i] = data;
        }

        for (int i = 0; i < units.Count; i++)
        {
            if(units[i].needsDestination)
            {
                units[i].SetDestination(FindDestination(units[i].Location, units[i].wanderRadius, instance.spawnableRegionLayer));
                units[i].needsDestination = false;
            }
        }

    }

    private static Vector3 FindDestination(Vector3 origin, float range, LayerMask groundMask)
    {
        Vector3 randPoint = (Random.insideUnitSphere * range) + origin + Vector3.up * 100f;
        Vector3 destination = origin;
        RaycastHit hit;
        if(Physics.Raycast(randPoint, Vector3.down, out hit, groundMask))
            destination = hit.point;

        return destination;
    }

    public static void Initialize(int numUnits)
    {
        units = new List<ServerUnitData>();

        numberOfUnits = numUnits;
        for (int i = 0; i < numberOfUnits; i++)
        {
            Vector3 spawnPoint = ServerData.playerSpawnLocations[Random.Range(0, ServerData.playerSpawnLocations.Length)];
            Vector3 origin = (spawnPoint + (Vector3.up * ServerData.spawnRadius)) + Random.insideUnitSphere * ServerData.spawnRadius;

            RaycastHit[] hits = null;
            Physics.RaycastNonAlloc(origin, Vector3.down, hits, 10f, instance.spawnableRegionLayer);

            if(hits != null)
                spawnPoint = hits[0].point;
            ServerUnitData data = null;
            GameObject go = Instantiate(instance.unitPrefab, spawnPoint, Quaternion.identity, instance.transform);
            go.TryGetComponent(out data);
            if (data == null) { Destroy(go); continue; }

            data.unitID = i;

            units.Add(data);

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

}
