using UnityEngine;

public abstract class ItemWeapon : Item
{
    [Header("Weapon Data:")]
    public Ballistics ballistics;
    [Range(0, 20)]
    public int powerConsumptionPerUse;

    public override abstract void ActiveUpdate(int ownerID);
    public override abstract void Update();
}
