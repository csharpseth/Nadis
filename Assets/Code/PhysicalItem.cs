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
            Events.Item.OnItemInteract += Interact;
            Events.Item.OnItemReset += ResetItem;
            Events.Item.OnItemHide += Hide;
            Events.Item.OnSetItemTransform += SetItemTransform;
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
        
    }

    public virtual void SecondaryUse(bool state)
    {
        
    }

    public void Interact(int instID, int playerID, Side handSide, bool send = false)
    {
        if (InstanceID != instID) return;
        BipedProceduralAnimator animator = Events.Player.OnGetPlayerAnimator(playerID);
        if (animator == null) return;

        Send = false;
        Receive = false;

        Transform hand = (handSide == Side.Right) ? animator.rightHand : animator.leftHand;

        rb.isKinematic = true;
        col.enabled = false;
        transform.SetParent(hand);
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = heldEulerOffset;
        parent = hand;
    }

    public void ResetItem(int instanceID, bool send = false)
    {
        if (instanceID != InstanceID)
            return;
        
        transform.parent = null;
        parent = null;
        rb.isKinematic = false;
        col.enabled = true;
        Send = true;
        Receive = true;
    }

    public void Hide(int instanceID, bool val, bool send = true)
    {
        if (instanceID != InstanceID) return;
        
        gameObject.SetActive(!val);
    }

    private void Update()
    {
        if(Send && NetworkManager.ins != null)
        {
            if((transform.position - lastPos).sqrMagnitude >= (positionSendThresholdDist * positionSendThresholdDist) || lastRot != transform.eulerAngles && receiving == false)
            {
                sending = true;
                Events.Item.OnItemTransform(InstanceID, transform.position, transform.eulerAngles, true);
                lastPos = transform.position;
                lastRot = transform.eulerAngles;
            }
            else
            {
                sending = false;
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
    public void SetItemTransform(int instanceID, Vector3 pos, Vector3 rot)
    {
        if(instanceID == InstanceID)
        {
            nextPos = pos;
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
