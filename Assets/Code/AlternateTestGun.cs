using UnityEngine;

public class AlternateTestGun : MonoBehaviour
{
    public Vector3 aimOffset = new Vector3(0f, 0.2f, 0.5f);
    public Vector3 aimHandOffset = new Vector3(0f, -0.02f, -0.05f);
    public Vector3 aimRot = new Vector3(0, 0f, 0f);
    public Vector3 holdRot = new Vector3(90f, 0f, 0f);
    public float reqoil = 0.2f;
    public float aimMoveDuration = 0.75f;
    public bool fullAuto = false;
    public bool aim = false;
    private bool aiming = false;
    private BipedProceduralAnimator anim;

    private void Update()
    {
        /*if(aim && anim != null && aiming == false)
        {
            float dist = (transform.position - anim.rightHand.obj.position).sqrMagnitude;
            if (dist > (0.1f * 0.1f))
                transform.position = anim.rightHand.obj.position;
        }*/

        if (Inp.Interact.SecondaryDown)
        {
            aim = !aim;
            if(anim == null)
                anim = TesterMenu.GetAnimator(TesterMenu.LocalPlayerID);

            
        }

        if (aim)
        {
            if (Inp.Interact.PrimaryDown)
                Shoot(NetworkedPlayer.LocalID);
        }
    }

    private void Shoot(ulong playerID)
    {

        Vector3 recoilOffset = (Vector3.back * reqoil);
        recoilOffset += aimOffset;
        Tween.FromToPosition(transform, recoilOffset, 0.075f, Space.Local, true, null, (Transform t) =>
        {
            transform.localPosition = aimOffset;
        });
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, -transform.forward);
    }
}
