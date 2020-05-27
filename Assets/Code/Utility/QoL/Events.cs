using System;
using UnityEngine;

public enum ItemEventType
{
    Interact = 1,
    Reset = 2,
    Hide = 3,
    Transform = 4,
    Spawn = 5,
    Destroy = 6,
}
public enum InventoryEventType
{
    AddItem = 1,
    RemoveItem = 2,
}
public enum PlayerStatsEventType
{
    AlterHealth = 1,
    AlterPower,
    Die
}

public static class Events
{
    public static InventoryEvents Inventory;
    public static PlayerEvents Player;
    public static PlayerStatEvents PlayerStats;
    public static Networking Net;
    public static AdministratorEvents admin;
}

public struct AdministratorEvents
{
    public delegate void SwitchActivePlayer(int fromID, int toID);
    public SwitchActivePlayer OnSwitchActivePlayer;
}

public struct InventoryEvents
{
    public Action<int> OnInventoryCreated;
    public Action<Item, int> OnItemAddedToInventory;
    public Action<int> OnItemRemovedFromInventory;
}

public struct PlayerEvents
{
    public delegate void OnGetPlayer(int playerID, ref NetworkedPlayer netPlayer);
    public delegate void PlayerPickupItem(int playerID, Item item);
    public delegate void OnSetPlayerAnimatorBool(int playerID, string id, bool value);
    public delegate void OnSetPlayerAnimatorFloat(int playerID, string id, float value);
    public delegate void OnSetPlayerAnimatorTrigger(int playerID, string id);

    public OnSetPlayerAnimatorBool SetAnimatorBool;
    public OnSetPlayerAnimatorFloat SetAnimatorFloat;
    public OnSetPlayerAnimatorTrigger SetAnimatorTrigger;
    public Action<int, float> SetAimOffset;

    public Action<int> Respawn;

    public Action<int> Shutdown;
    public Action<int> Startup;

    public PlayerPickupItem Pickup;
    public OnGetPlayer GetPlayer;
    public Action<int> UnSubscribe;
}
public struct PlayerStatEvents
{
    public delegate void OnAlter(float percent);
    public OnAlter OnAlterHealth;
    public OnAlter OnAlterPower;
}
public struct MapGeneratorEvents
{
    public Action<Vector3> RegisterSpawnPoint;
    public Action<int> GenerateMap;

    public delegate Vector3 GetMapPosition(float minH, float maxH, float maxAng);
    //public delegate SpawnPoint GetChargePoint(int index);
    //public delegate SpawnPoint[] GetChargePoints();
    //public GetMapPosition GetPosition;
    //public GetChargePoint GetChargePosition;

    public Action<float, float, float, int, int> CreateChargePoints;
    //public GetChargePoints GetChargePositions;
    public Action PlaceChargingStations;

}
public struct Notifications
{
    public delegate void NewNotification(NotificationType type, bool persistent = false);
    public NewNotification New;
    public Action<NotificationType> Remove;
}

public struct Networking
{
    public delegate void ClientDataCallback(int clientToSendAs, IPacketData packet);
    public delegate void ClientDisconnect();

    public ClientDataCallback SendAsClient;
    public ClientDataCallback SendAsClientUnreliable;
    public ClientDisconnect DisconnectClient;

}
