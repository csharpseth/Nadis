using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalItem : MonoBehaviour
{
    [Header("Item Data:")]
    public ItemMetaData meta;
    public float grabSpeed = 5f;
    public float grabbedThreshold = 0.1f;
    public Vector3 heldEulerOffset;
    
    internal Rigidbody rb;
    private Collider col;
    private Transform parent;

    bool instanceIDSet = false;
    public int InstanceID { get; private set; }

    public void SetInstanceID(int id)
    {
        if(instanceIDSet == false)
        {
            InstanceID = id;
            instanceIDSet = true;
        }
    }

    [Header("Network Parameters:")]
    public bool Send = true;
    public bool Receive = true;
    public float positionSendThresholdDist = 0.5f;
    public float positionReachedThreshold = 0.1f;
    private bool receiving = false;
    private bool sending = false;
    private Vector3 lastPos;
    private Vector3 nextPos;
    private Vector3 lastRot;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        if(col == null)
        {
            col = GetComponentInChildren<Collider>();
        }
    }

    public virtual void PrimaryUse()
    {
        Debug.Log(meta.name + " Primary Use");
    }

    public virtual void SecondaryUse(bool state)
    {
        Debug.Log(meta.name + " Secondary Use");
    }

    public void Interact(Transform hand)
    {
        rb.isKinematic = true;
        col.enabled = false;
        transform.SetParent(hand);
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = heldEulerOffset;
        parent = hand;
    }

    public void ResetObjct()
    {
        transform.parent = null;
        parent = null;
        rb.isKinematic = false;
        col.enabled = true;
    }

    public void Hide(bool val)
    {
        gameObject.SetActive(!val);
    }

    private void Update()
    {
        if(Send && NetworkManager.ins != null)
        {
            if((transform.position - lastPos).sqrMagnitude >= (positionSendThresholdDist * positionSendThresholdDist) && receiving == false)
            {
                sending = true;
                NetworkSend.SendItemMove(InstanceID, transform.position);
                lastPos = transform.position;
            }else
            {
                sending = false;
            }

            if(lastRot != transform.eulerAngles && receiving == false)
            {
                NetworkSend.SendItemRotate(InstanceID, transform.eulerAngles);
                lastRot = transform.eulerAngles;
            }
        }

        if(Receive)
        {
            if (nextPos != Vector3.zero && sending == false)
            {
                receiving = true;
                rb.isKinematic = true;
                transform.position = nextPos;

                if ((nextPos - transform.position).sqrMagnitude <= (positionReachedThreshold * positionReachedThreshold))
                {
                    nextPos = Vector3.zero;
                    receiving = false;
                    rb.isKinematic = false;
                }

            }
        }
    }

    //Item Event Callbacks from 'ItemManager'
    public void ItemMove(int instanceID, Vector3 pos)
    {
        if(instanceID == InstanceID)
        {
            nextPos = pos;
        }
    }

    public void ItemRotate(int instanceID, Vector3 rot)
    {
        if(instanceID == InstanceID)
        {
            transform.eulerAngles = rot;
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

}

[System.Serializable]
public class ItemMetaData
{
    public string name;
    public int id = -1;
}
