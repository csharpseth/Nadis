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

public static class Events
{
    public struct InventoryEvents
    {
        public Action<PhysicalItem, int> OnInventoryAdd;
        public Action<int> OnInventoryRemove;
        public Action<int> OnInventorySelect;
        public Action<PhysicalItem[]> OnInventoryChange;
    }
    public struct ItemEvents
    {
        public Action<int, Vector3, Vector3, bool> OnRequestSpawnItem;
        public Action<int, bool> OnRequestDestroyItem;

        public Action<int, int, Side, bool> OnItemInteract;
        public Action<int, bool> OnItemReset;
        public Action<int, bool, bool> OnItemHide; 
        public Action<int, Vector3, Vector3, bool> OnItemTransform;
        public Action<int, Vector3, Vector3> OnSetItemTransform;
    }
    public struct BipedAnimatorEvents
    {
        public Action OnRightFootBeginStep;
        public Action OnLeftFootBeginStep;

        public Action<float> OnRightFootStepping;
        public Action<float> OnLeftFootStepping;

        public Action OnRightFootFinishStep;
        public Action OnLeftFootFinishStep;
    }
    public struct PlayerEvents
    {
        public delegate PlayerSync GetPlayerSync(int playerID);
        public delegate BipedProceduralAnimator GetPlayerAnimator(int playerID);

        public GetPlayerSync OnGetPlayerSync;
        public GetPlayerAnimator OnGetPlayerAnimator;

    }

    public static InventoryEvents Inventory;
    public static ItemEvents Item;
    public static BipedAnimatorEvents BipedAnimator;
    public static PlayerEvents Player;

    

}