using Nadis.Net;
using System;
using UnityEngine;

[Serializable]
public struct PlayerStats
{
    public const float TimeToCheckCharging = 2f;
    public int MaxHealth;
    public float Health;
    public int MaxPower;
    public float Power;

    public float powerLossRatePerMinute;

    public int ActualHealth { get { return (int)(MaxHealth * Health); } }
    public int ActualPower { get { return (int)(MaxPower / Power); } }

    public PlayerStats(int maxHealth, float health, int maxPower, float power, float powerLossRate)
    {
        MaxHealth = maxHealth;
        MaxPower = maxPower;

        Health = health;
        Power = power;

        powerLossRatePerMinute = powerLossRate;
    }
}

public class PlayerStatsController : MonoBehaviour
{
    private PlayerStats stats;

    public PlayerStats Stats { get { return stats; } }
    public int NetID { get; private set; }

    bool charging = false;
    int powerLoseAmount = 0;

    //maxHealth & maxPower are the maximum integer value that either can be
    //The health & power are a float  from 0f-1f that are multiplied by the maximums to get the current value
    //This way you can directly use the health and power for UI elements
    public void InitFromServer(int playerID, int maxHealth, float startHealth, int maxPower, float startPower, float powerLossRate)
    {
        NetID = playerID;
        stats = new PlayerStats();
        stats.MaxHealth = maxHealth;
        stats.Health = startHealth;
        stats.MaxPower = maxPower;
        stats.Power = startPower;
        stats.powerLossRatePerMinute = powerLossRate;
        powerLoseAmount = 1;

        Subcribe();
    }
    private void Init(PlayerStats data)
    {
        stats.MaxHealth = data.MaxHealth;
        stats.Health = data.Health;
        stats.MaxPower = data.MaxPower;
        stats.Power = data.Power;

        stats.powerLossRatePerMinute = data.powerLossRatePerMinute;
        powerLoseAmount = 1;
    }
    private void ResetStats()
    {
        Init(NetworkManager.DefaultStats);
    }



    float powerTimer = 0f;
    float chargeTimer = 0f;

    private void Update()
    {
        if (NetID != Events.Player.GetLocalID()) return;

        if(charging == true)
        {
            chargeTimer += Time.deltaTime;
            if(chargeTimer >= PlayerStats.TimeToCheckCharging)
            {
                charging = false;
                chargeTimer = 0f;
            }
            return;
        }
        chargeTimer = 0f;

        if(stats.ActualPower > 0)
        {
            powerTimer += Time.deltaTime;
            if (powerTimer >= 1f)
            {
                Events.PlayerStats.AlterPower(NetID, -powerLoseAmount, true);
                powerTimer = 0f;
            }
        }
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
        float perc = (amount / (float)stats.MaxHealth);
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
        float perc = (amount / (float)stats.MaxPower);
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
        if (playerID != NetID || stats.Health >= 1f) return;
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
        Events.Notification.Remove(NotificationType.Charging);
        Events.Notification.Remove(NotificationType.NoPower);
        Events.Notification.Remove(NotificationType.FullyCharged);

        if (stats.Power >= 1f)
        {
            Events.Notification.Remove(NotificationType.Charging);
            Events.Notification.New(NotificationType.FullyCharged, true);
        }else if (stats.Power <= 0.25f)
        {
            Events.Notification.New(NotificationType.NoPower);
        }

        if (stats.Power >= 1f && amount > 0) return;
        if (stats.Power <= 0 && amount < 0) return;

        if (amount > 0) {
            charging = true;
            Events.Notification.New(NotificationType.Charging, true);
        }

        float perc = AddToPower(amount);
        Events.PlayerStats.OnAlterPower?.Invoke(playerID, perc, send);
    }

    private void Die(int playerID)
    {
        if (playerID != NetID) return;
        if(NetID == NetworkManager.LocalPlayer.ID)
        {
            Events.Inventory.DropAllItems?.Invoke(playerID, true);
            Events.Player.Respawn?.Invoke(playerID);
            Events.BipedAnimator.EndCurrentHandTarget?.Invoke(playerID, true);
        }

        Events.Player.CreateRagdoll?.Invoke(transform.position, transform.rotation);
        ResetStats();
    }
}
