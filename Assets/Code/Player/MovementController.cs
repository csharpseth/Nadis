using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MovementController : MonoBehaviour, INetworkInitialized, IDisableIfRemotePlayer
{
    //Data
    public MovementData data;
    private bool runToggle = false;
    private bool crouch = false;
    public int NetID { get; private set; }

    //Setup Stuffs
    private Rigidbody rb;
    private BipedProceduralAnimator anim;
    public bool disabled = false;
    private Vector3 dir;

    public bool canMove = true;
    public Vector3 Dir { get { return dir; } }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<BipedProceduralAnimator>();
    }

    public void InitFromNetwork(int netID)
    {
        NetID = netID;
    }

    //Actual Movement Logic & State Determination
    private void Update()
    {
        if (disabled == true || canMove == false) return;

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
                AlterDataState(PlayerMoveState.Running);
            else
                AlterDataState(PlayerMoveState.Walking);

            if (crouch)
                AlterDataState(PlayerMoveState.CrouchWalking);
            
            anim.SetMoveData(true, (int)Inp.Move.InputDir.x, (int)Inp.Move.InputDir.y, (int)data.state);
        }
        else
        {
            if (crouch == false)
                AlterDataState(PlayerMoveState.None);
            else
                AlterDataState(PlayerMoveState.Crouching);
            
            anim.SetMoveData(true, 0, 0, (int)data.state);
        }

    }

    public void AlterDataState(PlayerMoveState state)
    {
        MovementData d = data;
        d.state = state;
        data = d;
    }

    private void FixedUpdate()
    {
        if (disabled == true || canMove == false) return;

        float speed = data.GetSpeed;
        dir = InputToWorld(Inp.Move.InputDir) * speed;
        Debug.Log(speed);
        rb.MovePosition(rb.position + (dir * Time.fixedDeltaTime));
    }
    private Vector3 InputToWorld(Vector2 input)
    {
        Vector3 newDir = Vector3.zero;
        newDir += transform.forward * input.y;
        newDir += transform.right * input.x;
        return newDir;
    }

    public void Disable(bool disabled)
    {
        this.disabled = disabled;
        rb.isKinematic = disabled;
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
    [Range(0f, 1.5f)]
    public float speedPercent;
}
