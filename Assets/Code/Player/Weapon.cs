using UnityEngine;

public class Weapon : PhysicalItem
{
    [Header("Weapon Data:")]
    public float range = 100f;
    public float force = 75f;
    public Vector3 aimOffset;

    bool aimed = false;
    Transform idleParent;
    Transform aimParent;

    public override void PrimaryUse()
    {
        RaycastHit hit;
        if(Physics.Raycast(InteractionController.ins.CenterScreenRay, out hit, range))
        {
            Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
            if(rb != null)
            {
                rb.AddForceAtPosition((rb.transform.position - transform.position).normalized * force, hit.point, ForceMode.Impulse);
            }
        }
    }

    public override void SecondaryUse(bool state)
    {
        if(state == true)
        {
            InteractionController.ins.SetHandTargetPosition(aimOffset, Side.Right, grabSpeed, InteractionController.ins.Camera.transform, true);
        }
        else
        {
            InteractionController.ins.EndCurrentHandTarget();
        }
    }
}
