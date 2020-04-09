using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    public static Vector2 InputDir;
    public string crouch = "left ctrl";
    public string jump = "space";

    private bool crouched = false;
    private bool climbing = false;
    private MovementController moveController;

    private void Awake()
    {
        moveController = GetComponent<MovementController>();
    }

    private void Update()
    {
        Crouch();
        Jump();
    }

    private void Crouch()
    {
        if (Input.GetKey(crouch) && crouched == false)
        {
            Events.BipedAnimator.ExecuteAnimation?.Invoke(NetworkManager.LocalPlayer.ID, "crouch", null);
            crouched = true;
        }
        else if (Input.GetKey(crouch) == false)
        {
            Events.BipedAnimator.EndAnimation?.Invoke(NetworkManager.LocalPlayer.ID, "crouch");
            crouched = false;
        }
    }
    private void Jump()
    {
        if (climbing == true && moveController.IsClimbing == false)
        {
            Events.BipedAnimator.EndCurrentHandTarget(NetworkManager.LocalPlayer.ID, false);
            climbing = false;
        }

        if(Input.GetKeyDown(jump))
        {
            moveController.Climb();
            if(moveController.IsClimbing && climbing == false)
            {
                Vector3 rightDest = moveController.ClimbDestination + (transform.right * 0.3f);
                Vector3 leftDest = moveController.ClimbDestination - (transform.right * 0.3f);
                Events.BipedAnimator.SetHandTargetPosition(NetworkManager.LocalPlayer.ID, rightDest, Side.Right, 10f, AnimatorTarget.None, true, false);
                Events.BipedAnimator.SetHandTargetPosition(NetworkManager.LocalPlayer.ID, leftDest, Side.Left, 10f, AnimatorTarget.None, true, false);
                climbing = true;
            }else
            {
                Events.BipedAnimator.ExecuteAnimation(NetworkManager.LocalPlayer.ID, "jump", moveController.Jump);
            }
        }
    }

}
