using System.Collections.Generic;
using UnityEngine;

public class PlayerItemManager : MonoBehaviour, INetworkInitialized, IEventAccessor
{
    public int NetID { get; private set; }

    public Transform hand;
    
    public void InitFromNetwork(int netID)
    {
        NetID = netID;
        Subscribe();
    }

    public void PickupItem(int netID, Item item)
    {
        if (netID != NetID) return;
        
        item.transform.SetParent(hand);
    }

    public void Subscribe()
    {
        Events.Player.Pickup += PickupItem;
    }

    public void UnSubscribe(int netID)
    {
        throw new System.NotImplementedException();
    }
}
