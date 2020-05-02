using UnityEngine;

public abstract class ItemWeapon : Item, IWeapon
{
    public float Damage { get { return _damage; } set { _damage = value; } }
    public float Range { get { return _range; } set { _range = value; } }
    public AudioSource Source { get; set; }

    [Header("Weapon Data:")]
    [SerializeField]
    internal float _damage;
    [SerializeField]
    internal float _range;
    
    public override void InitFromNetwork(int netID)
    {
        base.InitFromNetwork(netID);
        Source = GetComponent<AudioSource>();
    }

    public override abstract void ActiveUpdate(int ownerID);
    public override abstract void Update();
}
