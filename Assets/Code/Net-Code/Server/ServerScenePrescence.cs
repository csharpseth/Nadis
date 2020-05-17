using UnityEngine;

public class ServerScenePrescence : MonoBehaviour 
{
    public static Vector3[] GetAllChargingStationLocations()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("ChargingStation");
        Vector3[] pos = new Vector3[gos.Length];
        for(int i = 0; i < gos.Length; i++)
        {
            pos[i] = gos[i].transform.position;
        }
        return pos;
    }
}