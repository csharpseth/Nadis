using Nadis.Net;
using UnityEngine;

public class PlayerStatAdjuster : MonoBehaviour
{
    public StatTarget target;
    public float maxDistance = 10f;
    [Range(0.2f, 5f)]
    public float delay = 0.5f;
    [Range(-50, 50)]
    public int adjustAmt = 10;
    
    float timer = 0f;
    PlayerSync ply;

    private void Update()
    {
        if(ply == null)
        {
            ply = Events.Player.GetPlayerSync(Events.Player.GetLocalID());
            return;
        }

        float sqrDist = (ply.transform.position - transform.position).sqrMagnitude;
        if (sqrDist >= (maxDistance * maxDistance)) return;

        timer += Time.deltaTime;
        if(timer >= delay)
        {
            Debug.Log("Player Stats Being altered Station");

            if (target == StatTarget.Health)
            {
                Events.PlayerStats.Heal(ply.ID, adjustAmt, true);
            }
            else if (target == StatTarget.Power)
            {
                Events.PlayerStats.AlterPower(ply.ID, adjustAmt, true);
            }

            timer = 0f;
        }
    }
    
}

public enum StatTarget
{
    Health,
    Power
}
