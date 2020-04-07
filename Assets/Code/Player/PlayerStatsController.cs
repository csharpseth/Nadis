using System;
using UnityEngine;

[Serializable]
public struct PlayerStats
{
    public int MaxHealth;
    public float Health;
    public int MaxPower;
    public float Power;

    public int ActualHealth { get { return (int)(MaxHealth * Health); } }
    public int ActualPower { get { return (int)(MaxPower / Power); } }

    public PlayerStats(int maxHealth, float health, int maxPower, float power)
    {
        MaxHealth = maxHealth;
        MaxPower = maxPower;

        Health = health;
        Power = power;
    }
}

public class PlayerStatsController : MonoBehaviour
{
    private PlayerStats stats;

    public PlayerStats Stats { get { return stats; } }
    public int NetID { get; private set; }


    //maxHealth & maxPower are the maximum integer value that either can be
    //The health & power are a float  from 0f-1f that are multiplied by the maximums to get the current value
    //This way you can directly use the health and power for UI elements
    public void InitFromServer(int playerID, int maxHealth, float startHealth, int maxPower, float startPower)
    {
        NetID = playerID;
        stats = new PlayerStats();
        stats.MaxHealth = maxHealth;
        stats.Health = startHealth;
        stats.MaxPower = maxPower;
        stats.Power = startPower;

        Subcribe();
    }
    private void Init(PlayerStats data)
    {
        stats.MaxHealth = data.MaxHealth;
        stats.Health = data.Health;
        stats.MaxPower = data.MaxPower;
        stats.Power = data.Power;
    }
    private void ResetStats()
    {
        Init(NetworkManager.DefaultStats);
    }


    private void Subcribe()
    {
        Events.PlayerStats.SetHealth += SetHealth;
        Events.PlayerStats.Heal += Heal;
        Events.PlayerStats.Damage += Damage;

        Events.PlayerStats.AlterPower += AlterPower;
        Events.PlayerStats.Die += Die;
        Events.Player.UnSubscribe += UnSubcribe;
    }
    private void UnSubcribe(int netID)
    {
        if (NetID != netID) return;

        Events.PlayerStats.SetHealth -= SetHealth;
        Events.PlayerStats.Heal -= Heal;
        Events.PlayerStats.Damage -= Damage;

        Events.PlayerStats.AlterPower -= AlterPower;
        Events.PlayerStats.Die -= Die;
        Events.Player.UnSubscribe -= UnSubcribe;
    }


    private float AddToHealth(int amount)
    {
        float perc = (amount / stats.MaxHealth);
        stats.Health += perc;
        if (stats.Health > 1f) stats.Health = 1f;
        return stats.Health;
    }

    private float RemoveFromHealth(int amount)
    {
        float perc = ((float)amount / (float)stats.MaxHealth);
        stats.Health -= perc;
        return stats.Health;
    }

    private float AddToPower(int amount)
    {
        float perc = (amount / stats.MaxPower);
        stats.Power += perc;
        if (stats.Power > 1f) stats.Power = 1f;
        return stats.Power;
    }

    public void SetHealth(int playerID, float perc, bool send)
    {
        if (playerID != NetID) return;

        stats.Health = perc;
        Events.PlayerStats.OnAlterHealth(playerID, perc, send);
    }

    public void SetPower(int playerID, float perc, bool send)
    {
        if (playerID != NetID) return;

        stats.Power = perc;
        Events.PlayerStats.OnAlterPower(playerID, perc, send);
    }

    public void Heal(int playerID, int amount, bool send)
    {
        if (playerID != NetID) return;
        float perc = AddToHealth(amount);
        Events.PlayerStats.OnAlterHealth?.Invoke(playerID, perc, send);
    }

    public void Damage(int playerID, int amount, bool send)
    {
        if (playerID != NetID) return;
        float perc = RemoveFromHealth(amount);
        Events.PlayerStats.OnAlterHealth?.Invoke(playerID, perc, send);
    }

    public void AlterPower(int playerID, int amount, bool send)
    {
        if (playerID != NetID) return;
        float perc = AddToPower(amount);
        Events.PlayerStats.OnAlterPower?.Invoke(playerID, perc, send);
    }

    private void Die(int playerID)
    {
        if (playerID != NetID) return;
        if(NetID == NetworkManager.LocalPlayer.ID)
        {
            Events.Inventory.DropAllItems(playerID, true);
            Events.Player.Respawn(playerID);
            Events.BipedAnimator.EndCurrentHandTarget(playerID, true);
        }

        NetworkManager.ins.CreatePlayerRagdoll(transform.position, transform.rotation);
        ResetStats();
        Debug.LogFormat("Player W/ ID:{0} Has Died R.I.P.", playerID);
    }
}
