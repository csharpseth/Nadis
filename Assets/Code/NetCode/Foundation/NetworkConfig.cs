using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KaymakNetwork.Network;
using System.Threading.Tasks;

internal static class NetworkConfig
{
    internal static Client socket;
    internal static ClientConnectionConfig config;

    internal static void InitNetwork()
    {
        if (!ReferenceEquals(socket, null)) return;

        socket = new Client(100);
        config = new ClientConnectionConfig();
        NetworkReceive.PacketRouter();

        Events.Item.OnRequestSpawnItem += NetworkSend.SendRequestItemSpawn;
        Events.Item.OnRequestDestroyItem += NetworkSend.SendRequestItemDestroy;

        Events.Item.Interact += NetworkSend.SendItemInteract;
        Events.Item.Reset += NetworkSend.SendItemReset;
        Events.Item.Hide += NetworkSend.SendItemHide;

        Events.Item.OnItemTransform += NetworkSend.SendItemTransform;

        Events.Inventory.OnAddItem += NetworkSend.SendInventoryAdd;
        Events.Inventory.OnRemoveItem += NetworkSend.SendInventoryRemove;

        Events.BipedAnimator.SetHandTargetPosition += NetworkSend.SendPlayerSetHandPosition;
        Events.BipedAnimator.EndCurrentHandTarget += NetworkSend.SendPlayerEndCurrentHandTarget;

        Events.PlayerStats.OnAlterHealth += NetworkSend.SendPlayerAlterHealth;
        Events.PlayerStats.OnAlterPower += NetworkSend.SendPlayerAlterPower;

        //Events.Inventory.OnInventoryChange += NetworkSend.SendPlayerInventoryUpdate;
    }

    internal static void ConnectLocal(int port = 5555)
    {
        socket.Connect("localhost", port);
    }

    internal static void ConnectToServer(ServerData server)
    {
        Debug.LogFormat("Attempting To Connect To Server At: {0}:{1}", server.remoteIP, server.port);
        socket.Connect(server.remoteIP, server.port);
    }

    internal static async void TimeOutCheck(bool retryLocal, ServerData server = null)
    {
        await Task.Delay(config.connectTimeout * 1000);
        if (socket.IsConnected == true)
            return;

        DisconnectFromServer();
        if(retryLocal)
        {
            Debug.Log("Failed To Connect On Public Address, Attempting on Local Address");
            ConnectLocal(server.port);
        }

    }

    internal static void DisconnectFromServer()
    {
        if(socket != null)
        {
            socket.Dispose();
            InitNetwork();
        }
    }

}

public class ClientConnectionConfig
{
    public int connectTimeout = 3;
}
