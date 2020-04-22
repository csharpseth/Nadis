using MLAPI;
using UnityEngine;

public abstract class Entity : IItem, IUsable, IComponent
{
    public string Name { get { return _name; } set { _name = value; } }
    public int ID { get { return _id; } set { _id = value; } }
    public ulong NetworkID { get { return _networkID; } set { _networkID = value; } }
    public bool Active { get { return _active; } set { _active = true; } }
    public EntityComponent Container { get; set; }
    
    internal bool _active;
    internal string _name;
    internal int _id;
    internal ulong _networkID;
    internal Rigidbody _rigidbody;
    internal Collider _collider;

    internal virtual void Awake()
    {
        _networkID = GetComponent<NetworkedObject>().NetworkId;
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        Inventory.RegisterEntity(this);
    }

    public virtual void Interact(ulong interactorID)
    {
        BipedProceduralAnimator anim = TesterMenu.GetAnimator(interactorID);
        if (anim == null) return;

        if(Inventory.AddItem(interactorID, this))
        {
            transform.SetParent(anim.rightHand.obj);
            _rigidbody.isKinematic = true;
            _collider.enabled = false;

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            Hide(true);
        }
    }

    public virtual void Hide(bool hide)
    {
        gameObject.SetActive(!hide);
    }

    public virtual void Destroy(ulong netID)
    {
        if (netID != _networkID) return;

        //IDK how  to network this with MLAPI yet
    }

    public abstract void ActiveUpdate();
    
    private void OnDestroy()
    {
        Inventory.DeRegisterEntity(this);
    }
    public void OnUpdate(float deltaTime)
    {
        throw new System.NotImplementedException();
    }
}
