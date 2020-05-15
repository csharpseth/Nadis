using UnityEngine;
using System.Collections;
using Nadis.Net.Client;
using Nadis.Net;
using System.Collections.Generic;

public static class PlayerManager
{
    private static Dictionary<int, PlayerStatsData> playerStats = new Dictionary<int, PlayerStatsData>();

    public static void CreatePlayersStatData(NetworkedPlayer player, int initialHealth, int maxHealth)
    {
        if (playerStats.ContainsKey(player.NetID)) return;

        HealthData health = new HealthData(player.NetID, initialHealth, maxHealth);
        PlayerStatsData stats = new PlayerStatsData(player, health);
        playerStats.Add(player.NetID, stats);
        if (player.NetID == NetData.LocalPlayerID)
            Log.Not("Created Local Player's Stats.");

        Events.PlayerStats.OnAlterHealth = (int playerID, float percent, bool send) =>
        {
            Log.Not("Player({0})'s Health was Altered. Current Health: %{1}", playerID, (100f * percent));
        };
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
        Events.PlayerStats.OnAlterHealth(packet.playerID, temp.health.Percent, false);
    }

}
