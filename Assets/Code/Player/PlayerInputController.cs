using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    public static Vector2 InputDir;
    public string crouch = "left ctrl";
    public string jump = "space";

    private bool crouched = false;

    private void Update()
    {
        Crouch();
        Jump();
    }

    private void Crouch()
    {
        if (Input.GetKey(crouch) && crouched == false)
        {
            Events.BipedAnimator.ExecuteAnimation(NetworkManager.LocalPlayer.ID, "crouch", null);
            crouched = true;
        }
        else if (Input.GetKey(crouch) == false)
        {
            Events.BipedAnimator.EndAnimation(NetworkManager.LocalPlayer.ID, "crouch");
            crouched = false;
        }
    }
    private void Jump()
    {
        if(Input.GetKeyDown(jump))
        {
            Events.BipedAnimator.ExecuteAnimation(NetworkManager.LocalPlayer.ID, "jump", GetComponent<MovementController>().Jump);
        }
    }

}
