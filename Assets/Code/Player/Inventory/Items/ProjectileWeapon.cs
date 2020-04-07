using UnityEngine;

public class ProjectileWeapon : Weapon
{
    public enum FireType
    {
        Single,
        Semi,
        Full
    }

    [Header("Projectile Weapon:")]
    public FireType fireType;
    public float fireDelay = 0.2f;
    bool canFire = true;
    float timer = 0f;



    public override void PrimaryUse(bool value)
    {
        if (fireType == FireType.Single)
            FireSingle();
        else if (fireType == FireType.Semi)
            FireSemi();
        else if (fireType == FireType.Full)
            FireFull();
    }

    private void FireSingle()
    {

    }

    private void FireSemi()
    {

    }

    private void FireFull()
    {

    }

}

