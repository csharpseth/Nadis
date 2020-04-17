using System;
using UnityEngine;
using Nadis.Net;

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
    public struct InventoryEvents
    {
        public delegate void ItemPlayerEvent(int instanceID, int playerID, bool send);

        //Networked
        public Action<int, PhysicalItem, int, bool> OnAddItem;
        public Action<int, int, bool> OnRemoveItem;
        public Action<int, bool> DropAllItems;

        //Not Networked
        public ItemPlayerEvent AddItem;
        public ItemPlayerEvent RemoveItem;
        public Action<int, bool> RemoveActiveItem;

        public Action<int> OnActive;
    }
    public struct ItemEvents
    {
        public Action<int, Vector3, Vector3, bool> OnRequestSpawnItem;
        public Action<int, bool> OnRequestDestroyItem;

        public delegate void OnItemInteract(int instanceID, int playerID, Side side, bool send);
        public OnItemInteract Interact;
        public Action<int, bool> Reset;
        public Action<int, bool, bool> Hide;
        public Action<int, Vector3, Vector3, bool> OnItemTransform;
        public Action<int, Vector3, Vector3> OnSetItemTransform;
        
        public delegate PhysicalItem OnGetItem(int instanceID);
        public OnGetItem GetItem;
    }
    public struct BipedAnimatorEvents
    {
        public Action<int> OnRightFootBeginStep;
        public Action<int> OnLeftFootBeginStep;

        public Action<int> OnRightFootStepping;
        public Action<int> OnLeftFootStepping;

        public Action<int> OnRightFootFinishStep;
        public Action<int> OnLeftFootFinishStep;

        public Action<int, Vector3, Side, float, AnimatorTarget, bool, bool> SetHandTargetPosition;
        public Action<int, bool> EndCurrentHandTarget;

        public Action<int, string, Action> ExecuteAnimation;
        public Action<int, string> EndAnimation;
    }
    public struct PlayerEvents
    {
        public delegate PlayerSync OnGetPlayerSync(int playerID);
        public delegate BipedProceduralAnimator OnGetPlayerAnimator(int playerID);
        public delegate Inventory OnGetInventory(int playerID);
        public delegate int OnGetLocalPlayerID();

        public OnGetPlayerSync GetPlayerSync;
        public OnGetPlayerAnimator GetPlayerAnimator;
        public OnGetInventory GetInventory;
        public OnGetLocalPlayerID GetLocalID;

        //These Are commands that are sent to the player, all are executed locally but are usually called by a Networked Function
        public Action<int> Respawn;
        public Action<int, Vector3> SetPos;
        public Action<int, float> SetRot;
        //Event Hooks up to the NetworkManager to create either a local or remote player
        public Action<int, int, bool> Create;
        public Action<int> UnSubscribe;
        public Action<int> Disconnect;

        //These Events Are Called by the Local PlayerSync script and are subscribed to by NetworkSend in NetworkConnfig
        public Action<int, Vector3> OnMove;
        public Action<int, float> OnRotate;
        public Action<int, bool, Vector2, PlayerMoveState, float> OnMoveData;

        public Action<int, bool, Vector2, PlayerMoveState, float> SetMoveData;
        public Action<Vector3, Quaternion> CreateRagdoll;

    }
    public struct PlayerStatEvents
    {
        public delegate void StatEvent(int playerID, int amount, bool send);
        public delegate void OnStatEvent(int playerID, float percent, bool send);
        public StatEvent Heal;
        public StatEvent Damage;
        public OnStatEvent SetHealth;
        public StatEvent AlterPower;
        public OnStatEvent SetPower;
        public Action<int> Die;

        public OnStatEvent OnAlterHealth;
        public OnStatEvent OnAlterPower;

        public Action<PlayerStats> SetDefaults;
    }
    public struct MapGeneratorEvents
    {
        public Action<Vector3> RegisterSpawnPoint;
        public Action<int> GenerateMap;

        public delegate Vector3 GetMapPosition(float minH, float maxH, float maxAng);
        public delegate SpawnPoint GetChargePoint(int index);
        public delegate SpawnPoint[] GetChargePoints();
        public GetMapPosition GetPosition;
        public GetChargePoint GetChargePosition;

        public Action<float, float, float, int, int> CreateChargePoints;
        public GetChargePoints GetChargePositions;
        public Action PlaceChargingStations;

    }

    public static InventoryEvents Inventory;
    public static ItemEvents Item;
    public static BipedAnimatorEvents BipedAnimator;
    public static PlayerEvents Player;
    public static PlayerStatEvents PlayerStats;
    public static MapGeneratorEvents MapGenerator;
}