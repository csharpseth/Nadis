using UnityEngine;

public class PlayerAnimatorEventController : MonoBehaviour
{
    public AgilityController agility;


    public void ApplyJumpForce()
    {
        agility.ApplyJumpForce();
    }

}
