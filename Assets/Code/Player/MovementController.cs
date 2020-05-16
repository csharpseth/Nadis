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
    public bool disabled = false;
    private Vector3 dir;

    public bool canMove = true;
    public Vector3 Dir { get { return dir; } }
    public Vector2 InputSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void InitFromNetwork(int netID)
    {
        NetID = netID;
    }

    //Actual Movement Logic & State Determination
    private void Update()
    {
        Debug.Log("Move Update Before Early Out");
        if (disabled == true || canMove == false) return;
        Debug.Log("Move Update After Early Out");


        runToggle = Inp.Move.Sprint;
        if (runToggle == false)
        {
            crouch = Inp.Move.Crouch;
        }
        
        if (Inp.Move.InputDir != Vector2.zero)
        {
            if (runToggle)
                AlterDataState(PlayerMoveState.Running);
            else
                AlterDataState(PlayerMoveState.Walking);

            if (crouch)
                AlterDataState(PlayerMoveState.CrouchWalking);
        }
        else
        {
            if (crouch == false)
                AlterDataState(PlayerMoveState.None);
            else
                AlterDataState(PlayerMoveState.Crouching);
        }

    }

    public void AlterDataState(PlayerMoveState state)
    {
        Debug.Log("Alter Move Data");
        MovementData d = data;
        d.state = state;
        data = d;
    }

    private void FixedUpdate()
    {
        if (disabled == true || canMove == false) return;

        float speed = data.GetSpeed;
        InputSpeed = Inp.Move.InputDir * data.GetSpeedPercent;
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

    public float GetSpeedPercent
    {
        get
        {
            float percent = 0f;
            for (int i = 0; i < speedProfiles.Length; i++)
            {
                if(speedProfiles[i].moveState == state) { percent = speedProfiles[i].speedPercent; break; }
            }

            return percent;
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
