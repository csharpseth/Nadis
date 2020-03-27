using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionController : MonoBehaviour
{
    public static InteractionController ins;
    
    [SerializeField]
    private new Camera camera;
    public float interactionReach = 6f;
    public LayerMask interactionMask;
    public ItemDatabase itemDB;

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
            return camera.ScreenPointToRay(CenterScreen);
        }
    }
    public Camera Cam { get { return camera; } }

    private BipedProceduralAnimator animator;

    private PhysicalItem[] items;
    public int inventorySize = 10;
    public int index = 0;
    public PhysicalItem CurrentItem { get { return items[index]; } }
    
    public Action<PhysicalItem, int> OnInventoryAdd;
    public Action<int> OnInventoryRemove;
    public Action<int> OnInventorySelect;
    public Action<Vector3, Side, bool> SetHandTargetPosition;
    public Action<Rigidbody, Side> SetHandTarget;
    public Action EndCurrentHandTarget;

    private void Awake()
    {
        if (ins == null)
            ins = this;

        animator = GetComponent<BipedProceduralAnimator>();
        if(animator != null)
        {
            SetHandTarget += animator.SetHandTarget;
            SetHandTargetPosition += animator.SetHandTargetPosition;
            EndCurrentHandTarget += animator.EndCurrentHandTarget;
        }

        InitInventory();
    }

    private void InitInventory()
    {
        items = new PhysicalItem[inventorySize];
    }

    private void Update()
    {
        if(Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            index++;
            if (index > (items.Length - 1))
                index = 0;
            OnInventorySelect?.Invoke(index);
        }else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            index--;
            if (index < 0)
                index = (items.Length - 1);
            OnInventorySelect?.Invoke(index);
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
                    itemDB.SpawnRandomItem(hit.point);
                }
            }
        }

        if(Input.GetButtonDown("Fire2"))
        {
            if (CurrentItem != null)
                CurrentItem.SecondaryUse();
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            Interact();
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            Drop(index);
        }
    }

    private void Interact()
    {
        Ray r = camera.ScreenPointToRay(CenterScreen);
        RaycastHit hit;

        if (Physics.Raycast(r, out hit, interactionReach, interactionMask))
        {
            PhysicalItem item = hit.transform.GetComponent<PhysicalItem>();
            if (item != null)
            {
                if (animator != null)
                {
                    PickupItem(item);
                }
            }
        }
    }

    private void PickupItem(PhysicalItem item)
    {
        if(CurrentItem != null)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if(items[i] == null)
                {
                    items[i] = item;
                    item.Interact(animator.rightHand);
                    item.Hide(true);
                    OnInventoryAdd?.Invoke(item, i);
                    break;
                }
            }
        }else
        {
            items[index] = item;
            item.Interact(animator.rightHand);
            OnInventoryAdd?.Invoke(item, index);
        }

        
    }

    private void Drop(int removeIndex)
    {
        if(items[removeIndex] != null)
        {
            items[removeIndex].ResetObjct();
            items[removeIndex] = null;
            OnInventoryRemove?.Invoke(removeIndex);
        }
    }

}
