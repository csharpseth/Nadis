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
    public Vector3 Position { get { return transform.position; } set { transform.position = value; } }
    public Vector3 Rotation { get { return transform.eulerAngles; } set { transform.eulerAngles = value; } }
    public Vector3 LocalPosition { get { return transform.localPosition; } set { transform.localPosition = value; } }
    public Vector3 LocalRotation { get { return transform.localEulerAngles; } set { transform.localEulerAngles = value; } }

    public void SetInstanceID(int id)
    {
        if(instanceIDSet == false)
        {
            InstanceID = id;
            Events.Item.Interact += Interact;
            Events.Item.Reset += ResetItem;
            Events.Item.Hide += Hide;
            Events.Item.OnSetItemTransform += SetItemTransform;

            Events.Item.Use += Use;
        }
    }
    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
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

    internal int ownerID = -1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        if(col == null)
        {
            col = GetComponentInChildren<Collider>();
        }
    }

    public virtual void Use(int instanceID, int useIndex, bool useValue, bool send = false)
    {
        if (instanceID != InstanceID)
            return;

        switch(useIndex)
        {
            case (1):
                PrimaryUse();
                break;
            case (2):
                SecondaryUse(useValue);
                break;
            default:
                break;
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
        BipedProceduralAnimator animator = Events.Player.GetPlayerAnimator(playerID);
        if (animator == null) return;

        ownerID = playerID;

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
        ownerID = -1;
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
