using UnityEngine;

public class TestPunch : MonoBehaviour
{
    public float maxDistance = 2f;
    public Vector3[] testPoints;
    private BipedProceduralAnimator animator;

    private void Awake()
    {
        testPoints = new Vector3[3];
        animator = GetComponent<BipedProceduralAnimator>();
    }



    private void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            Ray r = InteractionController.ins.CenterScreenRay;
            RaycastHit hit;
            if (Physics.Raycast(r, out hit, maxDistance))
            {
                Vector3 dest = hit.point;
                Vector3[] temp = new Vector3[2];
                temp[0] = (hit.point + (Vector3.right * 2f));
                temp[1] = dest;
                animator.SetHandTargetPositions(temp, Side.Right, 10f, null, false);
            }
        }
    }
}
