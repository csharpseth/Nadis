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
    public Transform LookingAt
    {
        get
        {
            RaycastHit hit;
            if (Physics.Raycast(CenterScreenRay, out hit))
                return hit.transform;
            return null;
        }
    }
    public PhysicalItem ItemLookingAt
    {
        get
        {
            Transform t = LookingAt;
            if (t != null)
                return t.GetComponent<PhysicalItem>();
            return null;
        }
    }

    public BipedProceduralAnimator Animator { get; private set; }
    
    private void Awake()
    {
        if (ins == null)
            ins = this;
    }

    private void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            RaycastHit hit;
            if (Physics.Raycast(CenterScreenRay, out hit))
            {
                Events.Item.OnRequestSpawnItem?.Invoke(0, hit.point, Vector3.zero, true);
            }
        }

        Inventory inv = Events.Inventory.GetInventory(NetworkManager.LocalPlayer.ID);
        if (inv != null && inv.ActiveItem != null)
        {
            Events.Item.Use?.Invoke(inv.ActiveItem.InstanceID, 2, Input.GetButton("Fire2"), true);
        }

        if (Input.GetButtonDown("Fire2"))
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

        if(Input.GetKeyDown(KeyCode.F))
        {
            PhysicalItem item = ItemLookingAt;
            PlayerSync ply = LookingAt.GetComponent<PlayerSync>();
            if (item != null) Events.Inventory.AddItem(item.InstanceID, NetworkManager.LocalPlayer.ID, true);
            if(ply != null)
            {
                for (int i = 0; i < inv.Content.Length; i++)
                {
                    if(inv.Content[i] != null)
                        Debug.Log("Player[" + ply.ID + "] :: Item[" + i + "]: " + inv.Content[i].meta.name);
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            Events.Inventory.RemoveActiveItem(NetworkManager.LocalPlayer.ID, true);
        }
    }

}
