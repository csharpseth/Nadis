using Nadis.Net;
using Nadis.Net.Client;
using UnityEngine;

public abstract class Item : MonoBehaviour, IItem, IUsable, INetworkID, INetworkInitialized, IEventAccessor
{
    public string Name { get { return _name; } set { _name = value; } }
    public int ID { get { return _id; } set { _id = value; } }
    public int NetID { get; private set; }
    public bool Active { get { return gameObject.activeSelf; } }

    [Header("Net Data:")]
    [SerializeField]
    private int timesToCheckPerSecond = 2;
    [SerializeField]
    private float distanceSendThreshold = 0.5f;
    private Vector3 lastPosition;

    private float SqrDist { get { return (distanceSendThreshold * distanceSendThreshold); } }

    [Header("Entity Data:")]
    [SerializeField]
    internal string _name;
    internal int _id;
    internal Rigidbody _rigidbody;
    internal Collider _collider;
    internal int ownerID = -1;

    public virtual void InitFromNetwork(int netID)
    {
        NetID = netID;
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        Inventory.RegisterItem(this);
        Subscribe();
    }
    
    public virtual void Interact(int playerID)
    {
        BipedProceduralAnimator anim = null;
        Events.Player.GetPlayerAnimator?.Invoke(playerID, ref anim);
        if (anim == null) { Debug.LogError("Anim is Null"); return; }

        if(Inventory.AddItem(playerID, this))
        {
            transform.SetParent(anim.TargetFrom(AnimatorTarget.Hands, Side.Right).obj);
            _rigidbody.isKinematic = true;
            _collider.enabled = false;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            ownerID = playerID;
            Hide(true);
        }
    }

    public virtual void Drop()
    {
        transform.SetParent(null);
        _rigidbody.isKinematic = false;
        _collider.enabled = true;
        ownerID = -1;
        Hide(false);
    }

    public virtual void Hide(bool hide)
    {
        gameObject.SetActive(!hide);
    }

    public virtual void Destroy()
    {
        Destroy(gameObject);
    }

    public abstract void ActiveUpdate(int ownerID);

    public abstract void Update();

    float timer = 0f;
    private void LateUpdate()
    {
        if (ownerID != -1) return;

        timer += Time.deltaTime;
        if(timer >= (1f / timesToCheckPerSecond))
        {
            float sqrDist = (transform.position - lastPosition).sqrMagnitude;
            if(sqrDist >= SqrDist)
            {
                PacketItemPosition packet = new PacketItemPosition
                {
                    NetworkID = NetID,
                    ItemPosition = transform.position
                };
                Events.Net.SendAsClientUnreliable(NetData.LocalPlayerID, packet);

                lastPosition = transform.position;
            }

            timer = 0f;
        }
    }

    private void OnDestroy()
    {
        Inventory.DeRegisterItem(this);
        UnSubscribe(NetID);
    }

    private void NetSetPosition(IPacketData packet)
    {
        PacketItemPosition data = (PacketItemPosition)packet;
        if (data.NetworkID != NetID) return;

        transform.position = data.ItemPosition;
    }

    public void Subscribe()
    {
        ClientPacketHandler.SubscribeTo((int)SharedPacket.ItemPosition, NetSetPosition);
    }

    public void UnSubscribe(int netID)
    {
        ClientPacketHandler.UnSubscribeFrom((int)SharedPacket.ItemPosition, NetSetPosition);
    }
}
