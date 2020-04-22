using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MovementController : MonoBehaviour
{
    //Data
    public MovementData data;
    private bool runToggle = false;
    private bool crouch = false;

    //Setup Stuffs
    private Rigidbody rb;
    private BipedProceduralAnimator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<BipedProceduralAnimator>();
    }
    
    //Actual Movement Logic & State Determination
    private void Update()
    {
        if(Inp.Move.SprintDown)
        {
            runToggle = !runToggle;
        }
        if(Inp.Move.CrouchDown && runToggle == false)
        {
            crouch = !crouch;
        }

        if (Inp.Move.InputDir != Vector2.zero)
        {
            if (runToggle)
                data.state = PlayerMoveState.Running;
            else
                data.state = PlayerMoveState.Walking;

            if (crouch)
                data.state = PlayerMoveState.CrouchWalking;

            anim.SetMoveData(true, Inp.Move.InputDir, data.state);
        }
        else
        {
            if (crouch == false)
                data.state = PlayerMoveState.None;
            else
                data.state = PlayerMoveState.Crouching;

            anim.SetMoveData(true, Vector2.zero, data.state);
        }

    }
    private void FixedUpdate()
    {
        float speed = data.GetSpeed;
        Vector3 dir = InputToWorld(Inp.Move.InputDir) * speed;
        rb.MovePosition(rb.position + (dir * Time.fixedDeltaTime));
    }
    private Vector3 InputToWorld(Vector2 input)
    {
        Vector3 dir = Vector3.zero;
        dir += transform.forward * input.y;
        dir += transform.right * input.x;
        return dir;
    }

}

[System.Serializable]
public struct MovementData
{
    public float maxSpeed; //This is the maximum speed the player can move
    [HideInInspector]
    public float speed; //This is a value from 0.0 - 1.0

    [HideInInspector]
    public PlayerMoveState state;
    [HideInInspector]
    public bool grounded;
    public SpeedProfile[] speedProfiles;
    
    public float GetSpeed
    {
        get
        {
            float speed = 0f;
            for (int i = 0; i < speedProfiles.Length; i++)
            {
                if (speedProfiles[i].moveState == state) { speed = speedProfiles[i].speedPercent; break; }
            }

            return speed * maxSpeed;
        }
    }
}

[System.Serializable]
public struct SpeedProfile
{
    public PlayerMoveState moveState;
    public float speedPercent;
}
