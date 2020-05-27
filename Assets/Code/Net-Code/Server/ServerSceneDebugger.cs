using Nadis.Net.Server;
using UnityEngine;

public class ServerSceneDebugger : MonoBehaviour
{
    public Color spawnLocationColor = Color.yellow;
    public Color chargingLocationColor = Color.blue;

    private void OnDrawGizmos()
    {
        if(ServerData.playerSpawnLocations != null)
            DrawSphereArray(ServerData.playerSpawnLocations, ServerData.spawnRadius, spawnLocationColor);

        if (ServerData.chargingStationLocations != null)
            DrawSphereArray(ServerData.chargingStationLocations, ServerData.ChargeDistance, chargingLocationColor);
    }

    private void DrawSphereArray(Vector3[] positions, float radius, Color gizmoColor, bool solid = false)
    {
        Gizmos.color = gizmoColor;
        foreach (Vector3 pos in positions)
        {
            if(solid)
                Gizmos.DrawSphere(pos, radius);
            else
                Gizmos.DrawWireSphere(pos, radius);
        }
    }
}
