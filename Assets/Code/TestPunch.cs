using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPunch : MonoBehaviour
{
    public float maxDistance = 2f;



    private void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            Ray r = InteractionController.ins.CenterScreenRay;
            RaycastHit hit;
            if (Physics.Raycast(r, out hit, maxDistance))
            {
                InteractionController.ins.SetHandTargetPosition(hit.point, Side.Right, 3f, false);
            }
        }
    }
}
