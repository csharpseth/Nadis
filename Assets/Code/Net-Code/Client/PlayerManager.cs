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

        PlayerStatsData stats = new PlayerStatsData(player, initialHealth, initialPower, maxHealth, maxPower);
        playerStats.Add(player.NetID, stats);
    }
    public static void DestroyPlayerStatData(int playerID)
    {
        if (playerStats.ContainsKey(playerID) == false) return;
        //Could potentially return PlayerStatsData to a pool if needed
        playerStats.Remove(playerID);
    }
    public static void DamagePlayer(PacketAlterPlayerHealth packet)
    {
        if (playerStats.ContainsKey(packet.playerID) == false) return;

        PlayerStatsData temp = playerStats[packet.playerID];
        temp.health = packet.health;
        playerStats[packet.playerID] = temp;

        //Debug.Log("Player Health: " + temp.health);
        Events.PlayerStats.OnAlterHealth?.Invoke(temp.HealthPercent);
    }

    public static void KillPlayer(PacketKillPlayer packet)
    {
        if(playerStats.ContainsKey(packet.playerID) == false) return;

        PlayerStatsData temp = playerStats[packet.playerID];
        temp.Reset();
        playerStats[packet.playerID] = temp;

        Events.PlayerStats.OnAlterHealth?.Invoke(temp.HealthPercent);
        Events.PlayerStats.OnAlterPower?.Invoke(temp.PowerPercent);

        Events.Player.Respawn(packet.playerID);
        Log.Not("Player:{0} Has Been Killed.", packet.playerID);
    }

    public static void AlterPlayerPowerLevel(PacketAlterPlayerPower packet)
    {
        if(playerStats.ContainsKey(packet.playerID) == false) return;

        PlayerStatsData temp = playerStats[packet.playerID];
        temp.power = packet.powerLevel;
        if(packet.playerID == NetData.LocalPlayerID)
        {
            Events.PlayerStats.OnAlterPower?.Invoke(temp.PowerPercent);
            if(temp.PowerPercent <= 0.005f)
            {
                Events.Player.SetAnimatorTrigger?.Invoke(packet.playerID, "power_down");
                temp.isShutdown = true;
            }

            if(temp.isShutdown && temp.PowerPercent >= 1f)
            {
                Events.Player.SetAnimatorTrigger?.Invoke(packet.playerID, "power_up");
                temp.isShutdown = false;
            }
        }
        playerStats[packet.playerID] = temp;
    }

    public static int GetPlayerPowerLevel(int playerID)
    {
        if(playerStats.ContainsKey(playerID) == false) return -1;
        return playerStats[playerID].power;
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
