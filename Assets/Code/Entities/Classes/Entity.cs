using UnityEngine;

public abstract class Entity : MonoBehaviour, IItem, IUsable
{
    public string Name { get { return _name; } set { _name = value; } }
    public int ID { get { return _id; } set { _id = value; } }
    public ulong NetworkID { get { return _networkID; } set { _networkID = value; } }
    public bool Active { get { return gameObject.activeSelf; } }

    [Header("Entity Data:")]
    [SerializeField]
    internal string _name;
    internal int _id;
    internal ulong _networkID;
    internal Rigidbody _rigidbody;
    internal Collider _collider;

    internal virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        Inventory.RegisterEntity(this);
    }

    public virtual void Interact(ulong interactorID)
    {
        BipedProceduralAnimator anim = null;// = TesterMenu.GetAnimator(interactorID);
        if (anim == null) return;

        if(Inventory.AddItem(interactorID, this))
        {
            transform.SetParent(anim.TargetFrom(AnimatorTarget.Hands, Side.Right).obj);
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

    public abstract void Update();

    private void OnDestroy()
    {
        Inventory.DeRegisterEntity(this);
    }
}
