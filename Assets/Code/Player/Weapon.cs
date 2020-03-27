using System.Collections;
using System.Collections.Generic;
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

    public override void SecondaryUse()
    {
        aimed = !aimed;

        if(aimed)
        {
            if (idleParent == null)
                idleParent = transform.parent;

            if (aimParent == null)
                transform.parent = InteractionController.ins.Cam.transform;
            else
                transform.parent = aimParent;

            transform.localPosition = aimOffset;
            transform.localEulerAngles = heldEulerOffset;

            InteractionController.ins.SetHandTarget(rb, Side.Right);
        }else
        {
            transform.parent = idleParent;
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = heldEulerOffset;
            InteractionController.ins.EndCurrentHandTarget();
        }
    }
}
