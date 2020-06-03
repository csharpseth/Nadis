using UnityEngine;
using System.Collections;
using Nadis.Net;

public class PlayerInteractionController : MonoBehaviour, INetworkInitialized, IDisableIfRemotePlayer
{
    public static PlayerInteractionController instance;

    private int ActiveIndex { get { return _activeIndex; } set { _activeIndex = Mathf.Clamp(value, 0, Inventory.GetSize(0) - 1); Inventory.DisableAllExcept(0, _activeIndex); } }
    private Item ActiveItem { get { return Inventory.GetItemAt(NetID, ActiveIndex); } }
    
    public int NetID { get; private set; }

    public float reach = 5f;
    public LayerMask interactionMask;
    public LayerMask playerMask;
    public LayerMask ignorePlayerMask;
    
    private int _activeIndex = 0;
    public bool disabled = false;
    
    private void Update()
    {
        if (disabled) return;

        if (Inp.Interact.Next)
            ActiveIndex++;
        else if (Inp.Interact.Previous)
            ActiveIndex--;

        if(ActiveItem != null)
        {
            if (ActiveItem.Active == false)
            {
                Inventory.HideItem(ActiveItem, false);
                //Inventory.HideAllExcept(ActiveItem) :: TODO
            }

            ActiveItem.ActiveUpdate(NetID);

            if(Inp.Interact.DropDown)
            {
                Inventory.RequestDropItem(ActiveItem.NetID, NetID);
            }
        }else
        {
            if(Inp.Interact.PrimaryDown)
            {
                RaycastHit hit;
                if(Physics.Raycast(PlayerMouseController.Instance.CenterScreenRay, out hit, reach, ignorePlayerMask))
                {
                    //TesterMenu.SpawnObject(testItem, hit.point);
                    Inventory.RequestSpawnItem(0, hit.point + (Vector3.up * 0.5f));
                }
            }

            if (Inp.Interact.InteractDown)
            {
                Interact();
            }
        }
    }

    private void Interact()
    {
        RaycastHit hit;
        if(Physics.Raycast(PlayerMouseController.Instance.CenterScreenRay, out hit, reach, interactionMask))
        {
            Item item = hit.transform.GetComponent<Item>();
            if(item != null)
            {
                //ent.Interact(NetworkedPlayer.LocalID);
                Inventory.RequestPickupItem(item.NetID, NetID);
            }
        }

        if(Physics.Raycast(PlayerMouseController.Instance.CenterScreenRay, out hit, reach, playerMask))
        {
            NetworkedPlayer ply = null;
            hit.transform.gameObject.TryGetComponent(out ply);
            if (ply != null)
            {
                Debug.Log("CLIENT::Try Revive Player");
                PacketRequestRevivePlayer packet = new PacketRequestRevivePlayer
                {
                    playerID = NetID,
                    playerToReviveID = ply.NetID
                };
                Events.Net.SendAsClientUnreliable(NetID, packet);
            }
        }
    }

    public void InitFromNetwork(int netID)
    {
        if (disabled) return;

        NetID = netID;
        if (instance != null) return;
        instance = this;
        
    }

    public void Disable(bool disabled)
    {
        this.disabled = disabled;
        Debug.Log(gameObject.name);
    }
}
