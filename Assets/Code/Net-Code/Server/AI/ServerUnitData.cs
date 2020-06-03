using UnityEngine;
using UnityEngine.AI;

public struct ServerUnitData
{
    public int unitID;
    public Transform transform;
    public NavMeshAgent agent;
    public float updateTimer;
    public bool stopped;
    public float fireTimer;
    public bool agro;
    public bool attack;
    public bool disabled;
    public int health;
    public int startHealth;

    public ServerUnitData(GameObject go, int unitID, int startHealth)
    {
        transform = go.transform;
        agent = transform.GetComponent<NavMeshAgent>();
        this.unitID = unitID;
        updateTimer = 0f;
        stopped = false;
        fireTimer = Random.Range(-5f, 5f);
        agro = false;
        attack = false;
        disabled = false;
        health = startHealth;
        this.startHealth = startHealth;
    }


    public void SetDestination(Vector3 destination)
    {
        agent.isStopped = false;
        stopped = false;
        agent.SetDestination(destination);
    }

    public void Stop()
    {
        agent.isStopped = true;
        stopped = true;
    }

    public void Reset()
    {
        updateTimer = 0f;
        stopped = false;
        fireTimer = Random.Range(-5f, 5f);
        agro = false;
        attack = false;
        disabled = false;
        health = startHealth;
    }

}
