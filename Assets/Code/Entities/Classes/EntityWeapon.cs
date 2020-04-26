using UnityEngine;

public abstract class EntityWeapon : Entity, IWeapon
{
    public float Damage { get { return _damage; } set { _damage = value; } }
    public float Range { get { return _range; } set { _range = value; } }
    public AudioSource Source { get; set; }

    [Header("Weapon Data:")]
    [SerializeField]
    internal float _damage;
    [SerializeField]
    internal float _range;

    internal override void Awake()
    {
        base.Awake();
        Source = GetComponent<AudioSource>();
    }
    public override abstract void ActiveUpdate();
    public override abstract void Update();
}
