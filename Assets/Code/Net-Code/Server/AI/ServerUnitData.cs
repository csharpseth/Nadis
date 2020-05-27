using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ServerUnitData : MonoBehaviour
{
    public float wanderRadius = 5f;
    public bool needsDestination = true;
    public int unitID;

    private NavMeshAgent agent;

    private void Awake()
    {
        TryGetComponent(out agent);
    }

    private void Update()
    {
        Location = transform.position;
    }

    public void SetDestination(Vector3 destination) => agent.SetDestination(destination);
    public Vector3 Location { get; private set; }
    public Vector3 LookDir => transform.forward;
    public Vector3 Destination => agent.destination;

    public float Speed => agent.speed;

}
