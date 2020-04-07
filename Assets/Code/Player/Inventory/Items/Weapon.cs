using UnityEngine;

public class Weapon : PhysicalItem
{
    [Header("Weapon Data:")]
    public float range = 100f;
    public float force = 75f;
    public int damage = 25;
    public Vector3 aimOffset;

    bool aimed = false;
    Transform idleParent;
    Transform aimParent;

    public override void PrimaryUse(bool useValue)
    {
        if (ownerID == -1) return;

        base.PrimaryUse(useValue);

        RaycastHit hit;
        if(Physics.Raycast(InteractionController.ins.CenterScreenRay, out hit, range))
        {
            Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
            if(rb != null)
                rb.AddForceAtPosition((rb.transform.position - transform.position).normalized * force, hit.point, ForceMode.Impulse);

            PlayerSync ply = hit.transform.GetComponent<PlayerSync>();
            if(ply != null)
                Events.PlayerStats.Damage(ply.ID, damage, true);
        }
    }

    public override void SecondaryUse(bool state)
    {
        if (ownerID == -1) return;

        //Debug.LogFormat("State:{0}   Aimed:{1}", state, aimed);

        if (state == true && aimed == false)
        {
            aimed = true;
            Events.BipedAnimator.SetHandTargetPosition(ownerID, aimOffset, Side.Right, grabSpeed, AnimatorTarget.Head, true, true);
        }


        if (state == false && aimed == true)
        {
            aimed = false;
            Events.BipedAnimator.EndCurrentHandTarget(ownerID, true);
        }
    }
}
