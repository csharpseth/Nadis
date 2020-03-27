using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : PhysicalItem
{
    public float range = 100f;
    public float force = 75f;
    

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
}
