using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class ServerUnitReferenceData
{
    public NavMeshAgent agent;
    public Transform transform;

    public ServerUnitReferenceData(Transform unitTransform)
    {
        agent = unitTransform.GetComponent<NavMeshAgent>();
        transform = unitTransform;
    }

    public bool SetDestination(Vector3 location) => agent.SetDestination(location);
    public float3 Position => transform.position;
    public float3 Rotation => transform.eulerAngles;
}
