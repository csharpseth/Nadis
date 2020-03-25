using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObject : MonoBehaviour
{
    public SyncType syncType;
    public int registryID = -1;
    
    public GameObject prefab;
    public bool smoothMove = false;
    public float speed;
    public float nextPositionThreshold = 0.3f;
    public bool send = true;
    public float sendPositionThreshold = 0.25f;
    public int frameWait = 6;


    private int spawnedID = -1;
    private Queue<Vector3> newPositions = new Queue<Vector3>();
    private Vector3 nextPosition;
    private Vector3 lastPosition = Vector3.zero;
    private Vector3 lastRotation = Vector3.zero;
    private Rigidbody rb;

    private bool receiving = false;
    private int frameCount = 0;

    public int SpawnID {
        get { return spawnedID; }
        set
        {
            if (spawnedID == -1)
                spawnedID = value;
        }
    }

    private void Update()
    {
        if(syncType == SyncType.Transform)
        {
            frameCount++;
            if(frameCount == frameWait)
            {
                receiving = (newPositions.Count > 0 || nextPosition != Vector3.zero);
                frameCount = 0;
            }

            if (newPositions.Count > 0 && nextPosition == Vector3.zero)
            {
                nextPosition = newPositions.Dequeue();
            }

            transform.position = Vector3.Lerp(transform.position, nextPosition, speed * Time.deltaTime);

            if ((nextPosition - transform.position).sqrMagnitude <= (nextPositionThreshold * nextPositionThreshold))
            {
                nextPosition = Vector3.zero;
            }

            if (send && receiving == false)
            {
                if(transform.eulerAngles != lastRotation)
                {
                    NetworkSend.SendRotateNetObjectRequest(SpawnID, transform.eulerAngles);
                    lastRotation = transform.eulerAngles;
                }
                //Send the current Transform.Position and the ObjectID to The Server to be relayed to other clients
                float sqrDist = (transform.position - lastPosition).sqrMagnitude;
                if(sqrDist >= (sendPositionThreshold * sendPositionThreshold))
                {
                    //Send New Position
                    NetworkSend.SendMoveNetObjectRequest(SpawnID, transform.position);
                    lastPosition = transform.position;
                }


            }
        }
    }

    private void FixedUpdate()
    {
        if (syncType == SyncType.Rigidbody)
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();

            frameCount++;
            if (frameCount == frameWait)
            {
                receiving = (newPositions.Count > 0 || nextPosition != Vector3.zero);
                frameCount = 0;
            }
            rb.isKinematic = receiving;

            if (send && receiving == false)
            {
                if(rb.rotation.eulerAngles != lastRotation)
                {
                    NetworkSend.SendRotateNetObjectRequest(SpawnID, rb.rotation.eulerAngles);
                    lastRotation = rb.rotation.eulerAngles;
                }
                //Send the current Rigidbody.Position & ObjectID to the server to be relayed across the clients
                float sqrDist = (rb.position - lastPosition).sqrMagnitude;
                if (sqrDist >= (sendPositionThreshold * sendPositionThreshold))
                {
                    //Send New Position
                    NetworkSend.SendMoveNetObjectRequest(SpawnID, rb.position);
                    lastPosition = rb.position;
                }
            }

            //If there are no NEW positions to seek & the next position being seeked is ZERO then dont LERP
            if (nextPosition == Vector3.zero && newPositions.Count == 0)
                return;

            //If there are new positions and the next position has already been reached then
            if(newPositions.Count > 0 && nextPosition == Vector3.zero)
            {
                nextPosition = newPositions.Dequeue();
            }

            rb.position = Vector3.Lerp(rb.position, nextPosition, speed * Time.fixedDeltaTime);

            if ((nextPosition - rb.position).sqrMagnitude <= (nextPositionThreshold * nextPositionThreshold))
            {
                nextPosition = Vector3.zero;
            }

        }
    }

    public void OnNetDestroy(int id, NetworkObjectsManager objManager)
    {
        if (id != spawnedID)
            return;

        objManager.RemoveFromEvents(this, spawnedID);
        Destroy(gameObject);
    }

    public void OnNetMove(int id, Vector3 newPos)
    {
        if(id == spawnedID)
        {
            newPositions.Enqueue(newPos);
        }
    }

    public void OnNetRotate(int id, Vector3 newRot)
    {
        if (id == spawnedID)
        {
            if(syncType == SyncType.Transform)
            {
                transform.eulerAngles = newRot;
            }else if(syncType == SyncType.Rigidbody)
            {
                rb.rotation = Quaternion.Euler(newRot);
            }
        }
    }

}

public enum SyncType
{
    Rigidbody,
    Transform
}
