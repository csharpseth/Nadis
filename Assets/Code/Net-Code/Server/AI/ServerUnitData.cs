using Unity.Mathematics;

public struct ServerUnitData
{
    public float3 position;
    public float3 destination;
    public bool agro;
    public bool attack;
    public int closestPlayerID;

    public float distanceToClosestPlayer;
    public bool destinationVisibleToUnit;

    public float timeLastAttacked;
    public float timeLastUpdatedDestination;
}
