using UnityEngine;

public class ServerScenePrescence : MonoBehaviour 
{
    public static Vector3[] GetAllPlayerSpawnPoints()
    {
        return GetAllPositionsFromTag("PlayerSpawnPoint");
    }

    public static Vector3[] GetAllChargingStationLocations()
    {
        return GetAllPositionsFromTag("ChargingStation");
    }

    public static Vector3[] GetAllPositionsFromTag(string tag)
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag(tag);
        Vector3[] pos = new Vector3[gos.Length];
        for (int i = 0; i < gos.Length; i++)
        {
            pos[i] = gos[i].transform.position;
        }
        return pos;
    }
}