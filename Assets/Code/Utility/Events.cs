using System;
using System.Collections.Generic;
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

public static class Events
{
    public struct InventoryEvents
    {
        public delegate void ItemPlayerEvent(int instanceID, int playerID, bool send);

        //Networked
        public Action<int, PhysicalItem, int, bool> OnAddItem;
        public Action<int, int, bool> OnRemoveItem;

        //Not Networked
        public delegate Inventory OnGetInventory(int playerID);
        public OnGetInventory GetInventory;
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
        public Action<int, int, bool, bool> Use;
        public Action<int, Vector3, Vector3, bool> OnItemTransform;
        public Action<int, Vector3, Vector3> OnSetItemTransform;

        public Action<int, int, bool, bool> OnItemUse;

        public delegate PhysicalItem OnGetItem(int instanceID);
        public OnGetItem GetItem;
    }
    public struct BipedAnimatorEvents
    {
        public Action OnRightFootBeginStep;
        public Action OnLeftFootBeginStep;

        public Action<float> OnRightFootStepping;
        public Action<float> OnLeftFootStepping;

        public Action OnRightFootFinishStep;
        public Action OnLeftFootFinishStep;

        public Action<int, Vector3, Side, float, AnimatorTarget, bool, bool> SetHandTargetPosition;
        public Action<int, bool> EndCurrentHandTarget;
    }
    public struct PlayerEvents
    {
        public delegate PlayerSync OnGetPlayerSync(int playerID);
        public delegate BipedProceduralAnimator OnGetPlayerAnimator(int playerID);

        public OnGetPlayerSync GetPlayerSync;
        public OnGetPlayerAnimator GetPlayerAnimator;

    }

    public static InventoryEvents Inventory;
    public static ItemEvents Item;
    public static BipedAnimatorEvents BipedAnimator;
    public static PlayerEvents Player;

    

}