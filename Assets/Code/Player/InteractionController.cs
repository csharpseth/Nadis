using System;
using UnityEngine;

public class InteractionController : MonoBehaviour
{
    public static InteractionController ins;
    
    public float interactionReach = 6f;
    public LayerMask interactionMask;

    public Vector2 CenterScreen
    {
        get
        {
            return new Vector2(Screen.width / 2, Screen.height / 2);
        }
    }
    public Ray CenterScreenRay
    {
        get
        {
            return Camera.ScreenPointToRay(CenterScreen);
        }
    }
    public Camera Camera
    {
        get
        {
            return PerspectiveController.ins.ActiveCamera;
        }
    }

    public BipedProceduralAnimator Animator { get; private set; }
    public int inventorySize = 10;
    public int index = 0;
    public PhysicalItem CurrentItem { get { return Inventory[index]; } }
    public PhysicalItem[] Inventory { get; private set; }
    
    public Action<Vector3, Side, float, Transform, bool> SetHandTargetPosition;
    public Action EndCurrentHandTarget;

    private void Awake()
    {
        if (ins == null)
            ins = this;

        Animator = GetComponent<BipedProceduralAnimator>();
        if(Animator != null)
        {
            SetHandTargetPosition += Animator.SetHandTargetPosition;
            EndCurrentHandTarget += Animator.EndCurrentHandTarget;
        }
    }

    public void InitInventory(int size = -1)
    {
        if(size > -1)
        {
            inventorySize = size;
        }

        Inventory = new PhysicalItem[inventorySize];
    }

    private void Update()
    {
        if(Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            index++;
            if (index > (Inventory.Length - 1))
                index = 0;
            Events.Inventory.OnInventorySelect?.Invoke(index);
        }else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            index--;
            if (index < 0)
                index = (Inventory.Length - 1);
            Events.Inventory.OnInventorySelect?.Invoke(index);
        }

        if(Input.GetButtonDown("Fire1"))
        {
            if (CurrentItem != null)
                CurrentItem.PrimaryUse();
            else
            {
                RaycastHit hit;
                if (Physics.Raycast(CenterScreenRay, out hit))
                {
                    Events.Item.OnRequestSpawnItem?.Invoke(0, hit.point, Vector3.zero, true);
                }
            }
        }

        if (CurrentItem != null) CurrentItem.SecondaryUse(Input.GetButton("Fire2"));

        if(Input.GetButtonDown("Fire2"))
        {
            if (CurrentItem == null)
            {
                RaycastHit hit;
                if (Physics.Raycast(CenterScreenRay, out hit))
                {
                    PhysicalItem item = hit.transform.GetComponent<PhysicalItem>();
                    if (item != null && ItemManager.ins != null)
                    {
                        Events.Item.OnRequestDestroyItem?.Invoke(item.InstanceID, true);
                    }
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            Interact(true);
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            Drop(index);
        }
    }

    private void Interact(bool localRequest = true, int instanceID = -1, int playerID = -1, int side = -1)
    {
        if(localRequest == true)
        {
            Ray r = CenterScreenRay;
            RaycastHit hit;

            if (Physics.Raycast(r, out hit, interactionReach, interactionMask))
            {
                PhysicalItem item = hit.transform.GetComponent<PhysicalItem>();
                if (item != null)
                {
                    Events.Item.OnItemInteract?.Invoke(item.InstanceID, NetworkManager.LocalPlayer.ID, Side.Right, true);
                    PickupItem(item);

            }
            }
        }else
        {
            Events.Item.OnItemInteract?.Invoke(instanceID, playerID, (Side)side, false);
        }
    }

    private void PickupItem(PhysicalItem item)
    {
        if(CurrentItem != null)
        {
            for (int i = 0; i < Inventory.Length; i++)
            {
                if(Inventory[i] == null)
                {
                    Inventory[i] = item;
                    Events.Item.OnItemHide?.Invoke(item.InstanceID, true, true);
                    Events.Inventory.OnInventoryAdd?.Invoke(item, i);
                    break;
                }
            }
        }else
        {
            Inventory[index] = item;
            Events.Inventory.OnInventoryAdd?.Invoke(item, index);
        }

        Events.Inventory.OnInventoryChange?.Invoke(Inventory);
    }

    private void Drop(int removeIndex)
    {
        if(Inventory[removeIndex] != null)
        {
            Events.Item.OnItemReset?.Invoke(Inventory[removeIndex].InstanceID, true);
            Inventory[removeIndex] = null;
            Events.Inventory.OnInventoryRemove?.Invoke(removeIndex);
            Events.Inventory.OnInventoryChange?.Invoke(Inventory);
        }
    }

}
