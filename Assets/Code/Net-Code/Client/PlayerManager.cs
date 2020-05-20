using UnityEngine;
using System.Collections;
using Nadis.Net.Client;
using Nadis.Net;
using System.Collections.Generic;

public static class PlayerManager
{
    private static Dictionary<int, PlayerStatsData> playerStats = new Dictionary<int, PlayerStatsData>();

    public static void CreatePlayerStatData(NetworkedPlayer player, int initialHealth, int maxHealth, int initialPower, int maxPower)
    {
        if (playerStats.ContainsKey(player.NetID)) return;

        StatData health = new StatData(player.NetID, initialHealth, maxHealth);
        StatData power = new StatData(player.NetID, initialPower, maxPower);
        PlayerStatsData stats = new PlayerStatsData(player, health, power);
        playerStats.Add(player.NetID, stats);
        Log.Event(" Created Stats :: Health:{0}, Power:{1}", health.Value, power.Value);
            
        /*Events.PlayerStats.OnAlterHealth = (int playerID, float percent, bool send) =>
        {
            Log.Not("Player({0})'s Health was Altered. Current Health: %{1}", playerID, (100f * percent));
        };*/
    }
    public static void DestroyPlayerStatData(int playerID)
    {
        if (playerStats.ContainsKey(playerID) == false) return;
        //Could potentially return PlayerStatsData to a pool if needed
        playerStats.Remove(playerID);
    }
    public static void DamagePlayer(PacketDamagePlayer packet)
    {
        if (playerStats.ContainsKey(packet.playerID) == false) return;

        PlayerStatsData temp = playerStats[packet.playerID];
        temp.health.AlterValue(-packet.alterAmount);
        playerStats[packet.playerID] = temp;
        //Events.PlayerStats.OnAlterHealth(packet.playerID, temp.health.Percent, false);
        Debug.Log("Player Damaged, New Health: " + temp.health.Value);
    }

    public static void KillPlayer(PacketKillPlayer packet)
    {
        if(playerStats.ContainsKey(packet.playerID) == false) return;

        PlayerStatsData temp = playerStats[packet.playerID];
        temp.Reset();
        playerStats[packet.playerID] = temp;

        Events.Player.Respawn(packet.playerID);
        Log.Not("Player:{0} Has Been Killed.", packet.playerID);
    }

    public static void AlterPlayerPowerLevel(PacketAlterPlayerPower packet)
    {
        if(playerStats.ContainsKey(packet.playerID) == false) return;

        PlayerStatsData temp = playerStats[packet.playerID];
        StatData stat = temp.power;
        stat.SetValue(packet.powerLevel);
        Debug.Log(packet.powerLevel);
    }

    public static int GetPlayerPowerLevel(int playerID)
    {
        //LOL
        if(playerStats.ContainsKey(playerID) == false) return -1;
        return playerStats[playerID].power.Value;
    }

    public static bool RequestUsePower(int playerID, int amount)
    {
        if(playerStats.ContainsKey(playerID) == false) return false;
        if(GetPlayerPowerLevel(playerID) < amount) return false;

        PacketRequestUsePower packet = new PacketRequestUsePower
        {
            playerID = playerID,
            useAmount = amount
        };
        Events.Net.SendAsClientUnreliable(playerID, packet);
        return true;
    }

}
