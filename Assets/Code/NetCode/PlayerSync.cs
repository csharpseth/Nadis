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
    public int maxQueueSize = 15;
    private float moveDataCheckDelay = 0.25f;

    private Vector3 prevPosition;
    private Vector3 prevRotation;
    private int id;
    private bool idSet = false;
    private float lerpSpeed = 15f;

    private Queue<Vector3> positionQueue = new Queue<Vector3>();
    private Vector3 destination;
    private BipedProceduralAnimator animator;
    private MovementController moveController;

    private bool grounded;
    private Vector2 inputDir;
    private PlayerMoveState moveState;

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
    }

    private void Update()
    {
        if(positionQueue.Count > 0 || destination != Vector3.zero)
        {
            if (positionQueue.Count > maxQueueSize)
                positionQueue.Dequeue();

            if(destination == Vector3.zero)
            {
                destination = positionQueue.Dequeue();
            }

            transform.position = Vector3.Lerp(transform.position, destination, lerpSpeed * Time.deltaTime);
            float sqrDist = (destination - transform.position).sqrMagnitude;
            if (sqrDist >= (maxDistance * maxDistance))
                transform.position = destination;

            if (sqrDist <= (reachedThreshold * reachedThreshold))
            {
                destination = Vector3.zero;
            }

        }

        if (Send == false)
            return;

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
        positionQueue.Enqueue(newPosition);
    }

    public void SetRotation(Vector3 newRotation)
    {
        transform.eulerAngles = newRotation;
    }

    public void SetProceduralMoveData(bool grounded, Vector2 inputDir, PlayerMoveState moveState, float moveSpeed)
    {
        Debug.Log(moveSpeed);
        lerpSpeed = moveSpeed;
        if(animator != null)
        {
            animator.SetMoveData(grounded, inputDir, moveState);
        }
    }

}
