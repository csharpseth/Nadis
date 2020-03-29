using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSync : MonoBehaviour
{
    public bool Local = false;
    public bool Send = true;
    public float moveThreshold = 2f;
    public float reachedThreshold = 0.2f;
    public float maxDistance = 5f;
    private float moveDataCheckDelay = 0.25f;

    private Vector3 prevPosition;
    private Vector3 prevRotation;
    private int id;
    private bool idSet = false;
    private float lerpSpeed = 10f;
    
    private Vector3 destination;
    private BipedProceduralAnimator animator;
    private MovementController moveController;

    private bool grounded;
    private Vector2 inputDir;
    private PlayerMoveState moveState;

    public int[] Inventory { get; private set; }

    public int ID { get { return id; } set
        {
            if (idSet == false) id = value;
            idSet = true;
        }
    }

    private void Awake()
    {
        if (Local && NetworkManager.localPlayer == null)
            NetworkManager.localPlayer = this;
        animator = GetComponent<BipedProceduralAnimator>();
        if (Send)
        {
            moveController = GetComponent<MovementController>();
            InteractionController.ins.OnInventoryChange += InventoryChange;
        }
    }

    private void Update()
    {
        if (Send == false)
        {
            if(destination != Vector3.zero)
            {
                transform.position = Vector3.Lerp(transform.position, destination, lerpSpeed * Time.deltaTime);
                if((destination - transform.position).sqrMagnitude <= (reachedThreshold * reachedThreshold))
                {
                    transform.position = destination;
                    destination = Vector3.zero;
                }
            }

            return;
        }

        if((transform.position - prevPosition).sqrMagnitude >= (moveThreshold * moveThreshold))
        {
            prevPosition = transform.position;
            NetworkSend.SendPlayerPosition(id, transform.position);
        }
        
        if(transform.eulerAngles != prevRotation)
        {
            prevRotation = transform.eulerAngles;
            NetworkSend.SendPlayerRotation(id, transform.eulerAngles);
        }

        UpdateMoveData();
    }

    float moveDataTimer = 0f;
    private void UpdateMoveData()
    {
        if (moveController == null)
            return;
        moveDataTimer += Time.deltaTime;
        if(moveDataTimer >= moveDataCheckDelay)
        {
            NetworkSend.SendPlayerMoveData(id, moveController.IsGrounded, moveController.InputDir, moveController.MoveState, moveController.Speed);
            moveDataTimer = 0f;
        }
        

    }

    public void SetPosition(Vector3 newPosition)
    {
        destination = newPosition;
    }

    public void SetRotation(Vector3 newRotation)
    {
        transform.eulerAngles = newRotation;
    }

    public void SetProceduralMoveData(bool grounded, Vector2 inputDir, PlayerMoveState moveState, float moveSpeed)
    {
        lerpSpeed = moveSpeed;
        if(animator != null)
        {
            animator.SetMoveData(grounded, inputDir, moveState);
        }
    }

    public void SetInventorySize(int size)
    {
        Inventory = new int[size];
    }
    public void UpdateInventory(int[] ids)
    {
        for (int i = 0; i < ids.Length; i++)
        {
            Inventory[i] = ids[i];
        }
    }
    public void InventoryChange(PhysicalItem[] items)
    {
        int[] ids = new int[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
                ids[i] = items[i].meta.id;
            else
                ids[i] = -1;
        }

    }

}
