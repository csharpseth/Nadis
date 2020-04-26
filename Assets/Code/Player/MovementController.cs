using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MovementController : MonoBehaviour, INetworkInitialized
{
    //Data
    public MovementData data;
    private bool runToggle = false;
    private bool crouch = false;
    public int NetID { get; private set; }
    private bool initialized = false;

    //Setup Stuffs
    private Rigidbody rb;
    private BipedProceduralAnimator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<BipedProceduralAnimator>();
    }

    public void InitFromNetwork(int netID)
    {
        NetID = netID;
        initialized = true;
    }

    //Actual Movement Logic & State Determination
    private void Update()
    {
        if (initialized == false) return;

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
            
            //InvokeClientRpcOnEveryone(anim.SetMoveData, true, Inp.Move.InputDir, (int)data.state);
            //InvokeClientRpcOnEveryone(anim.RPCSetMoveData, true, Inp.Move.InputDir.x, Inp.Move.InputDir.y, (int)data.state);
            anim.SetMoveData(true, (int)Inp.Move.InputDir.x, (int)Inp.Move.InputDir.y, (int)data.state);
        }
        else
        {
            if (crouch == false)
                data.state = PlayerMoveState.None;
            else
                data.state = PlayerMoveState.Crouching;

            //InvokeClientRpcOnEveryone(anim.SetMoveData, true, Vector2.zero, (int)data.state);
            //InvokeClientRpcOnEveryone(anim.RPCSetMoveData, true, 0f, 0f, (int)data.state);
            anim.SetMoveData(true, 0, 0, (int)data.state);
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
