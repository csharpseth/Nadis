using UnityEngine;

public abstract class EntityWeapon : Entity, IWeapon
{
    public float Damage { get { return _damage; } set { _damage = value; } }
    public float Range { get { return _range; } set { _range = value; } }
    public string PrimaryUseAnimation { get { return _primaryUseAnimation; } set { _primaryUseAnimation = value; } }
    public string SecondaryUseAnimation { get { return _secondaryUseAnimation; } set { _secondaryUseAnimation = value; } }
    
    [Header("Weapon Data:")]
    [SerializeField]
    internal float _damage;
    [SerializeField]
    internal float _range;
    [SerializeField]
    internal string _primaryUseAnimation;
    [SerializeField]
    internal string _secondaryUseAnimation;

    public override abstract void ActiveUpdate();
    public override abstract void Update();
}
