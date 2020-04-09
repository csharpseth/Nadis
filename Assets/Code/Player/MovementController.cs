using UnityEngine;

public class MovementController : MonoBehaviour
{
    public PlayerPhysicalDimensions sizeInfo;
    public PlayerMovementStats moveInfo;
    public PlayerClimbingInfo climbInfo;
    public float jumpForce = 600f;
    public bool useBetterFall = false;
    public float betterFallMultiplier = 0.8f;
    public float fallingMovementModifier = 0.25f;
    public float groundedRadius = 0.5f;
    public LayerMask groundedMask;
    public float yDistToStopGrabbing = 0.5f;
    Vector3 dir;
    Rigidbody rb;
    CapsuleCollider physicsCollider;
    public Vector3 ClimbDestination { get; private set; }
    BipedProceduralAnimator animator;

    public Vector2 InputDir { get; private set; }

    float prevHeight;

    float time = 0f;
    Vector3 prevPosition;

    public bool CanCrouch
    {
        get
        {
            return (IsGrounded == true && MoveState != PlayerMoveState.Running);
        }
    }
    public bool CanWalk
    {
        get
        {
            return (IsGrounded == true && IsClimbing == false);
        }
    }
    public bool CanRun
    {
        get
        {
            return (IsClimbing == false && IsGrounded == true && MoveState != PlayerMoveState.Crouching);
        }
    }
    public bool CanClimb
    {
        get
        {
            return (MoveState != PlayerMoveState.Crouching && ClimbDestination != Vector3.zero);
        }
    }

    public bool IsCrouching { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsClimbing { get; private set; }
    public bool IsFalling { get; private set; }
    public bool JustGrounded { get; private set; }

    public float Speed { get; private set; }

    public PlayerMoveState MoveState { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<BipedProceduralAnimator>();
        physicsCollider = GetComponent<CapsuleCollider>();
    }

    bool lastGroundState = true;
    private void Update()
    {
        InputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        dir = Vector3.zero;
        dir += transform.forward * InputDir.y;
        dir += transform.right * InputDir.x;

        if (IsGrounded == false)
            lastGroundState = false;

        Collider[] cols = Physics.OverlapSphere(transform.position, groundedRadius, groundedMask);
        IsGrounded = (cols.Length > 0);
        
        if(IsGrounded == true && lastGroundState == false)
        {
            Events.BipedAnimator.ExecuteAnimation?.Invoke(NetworkManager.LocalPlayer.ID, "land", null);
            lastGroundState = true;
        }

        Climbing();
        Crouching();
        
        physicsCollider.center = new Vector3(0f, (physicsCollider.height / 2f), 0f);

        if (Input.GetButton("Sprint") && dir != Vector3.zero)
        {
            Running();
        }else if(dir != Vector3.zero)
        {
            Walking();
        }

        if(dir == Vector3.zero || IsGrounded == false && MoveState != PlayerMoveState.Crouching)
        {
            MoveState = PlayerMoveState.None;
        }

        if (transform.position.y != prevHeight)
        {
            if (prevHeight > transform.position.y)
            {
                if (!IsGrounded) IsFalling = true;
                else IsFalling = false;
            }
            prevHeight = transform.position.y;
        }

        //rbController.SetSpeed(speed);
    }

    public void Climb()
    {
        if (IsClimbing == false)
        {
            RaycastHit hit;
            if (Physics.Raycast((climbInfo.checkOrigin.position), Vector3.down, out hit, climbInfo.rayDist))
            {
                ClimbDestination = hit.point;
                IsClimbing = true;
            }
            else
            {
                ClimbDestination = Vector3.zero;
                IsClimbing = false;
            }
        }
    }

    public void Jump()
    {
        if (IsClimbing == true)
            return;
        
        if (IsGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            rb.velocity = (dir * Speed * 0.8f);
        }
    }

    private void Walking()
    {
        if (CanWalk == false)
            return;
        if(MoveState != PlayerMoveState.Crouching)
            MoveState = PlayerMoveState.Walking;
        Speed = moveInfo.walkSpeed;
    }

    private void Running()
    {
        if (CanRun == false)
            return;
        
        MoveState = PlayerMoveState.Running;
        Speed = moveInfo.runSpeed;
    }

    private void Crouching()
    {
        if (Input.GetButton("Crouch") && CanCrouch)
        {
            MoveState = PlayerMoveState.Crouching;
            IsCrouching = true;
        }

        if (CanCrouch == false)
        {
            return;
        }

        

        if (MoveState != PlayerMoveState.Crouching)
        {
            physicsCollider.height = Mathf.Lerp(physicsCollider.height, sizeInfo.normalHeight, sizeInfo.smoothedLerpSpeed * Time.deltaTime);
            if (sizeInfo.camera != null)
            {
                sizeInfo.camera.transform.localPosition = Vector3.Lerp(sizeInfo.camera.transform.localPosition, sizeInfo.cameraNormalOffset, sizeInfo.smoothedLerpSpeed * Time.deltaTime);
            }
            return;
        }

        if(!Input.GetButton("Crouch"))
        {
            MoveState = PlayerMoveState.None;
            IsCrouching = false;
            return;
        }


        Speed = moveInfo.crouchSpeed;
        physicsCollider.height = Mathf.Lerp(physicsCollider.height, sizeInfo.crouchHeight, sizeInfo.smoothedLerpSpeed * Time.deltaTime);
        if (sizeInfo.camera != null)
        {
            sizeInfo.camera.transform.localPosition = Vector3.Lerp(sizeInfo.camera.transform.localPosition, sizeInfo.cameraCrouchOffset, sizeInfo.smoothedLerpSpeed * Time.deltaTime);
        }
    }

    private void Climbing()
    {
        IsClimbing = (ClimbDestination != Vector3.zero);
        rb.isKinematic = IsClimbing;
        
        if (IsClimbing)
        {
            Debug.Log("Climbing");
            Vector3 temp = transform.position;
            if((ClimbDestination - temp).sqrMagnitude > (climbInfo.distToEnd * climbInfo.distToEnd))
            {
                IsClimbing = false;
                temp.y = Mathf.Lerp(temp.y, ClimbDestination.y, climbInfo.speed * Time.deltaTime);
                ClimbDestination = Vector3.zero;
                return;
            }

            if (Mathf.Abs(ClimbDestination.y - temp.y) > 0.2f)
            {
                temp.y = Mathf.Lerp(temp.y, ClimbDestination.y, climbInfo.speed * Time.deltaTime);
            }
            else
            {
                temp = Vector3.Lerp(temp, ClimbDestination, climbInfo.speed * Time.deltaTime);
            }

            transform.position = temp;

            float sqrDist = (ClimbDestination - temp).sqrMagnitude;

            if(Mathf.Abs(ClimbDestination.y - temp.y) <= yDistToStopGrabbing)
            {
                //rbController.ResetHands();
            }

            if (sqrDist <= (climbInfo.finishClimbDistance * climbInfo.finishClimbDistance))
            {
                IsClimbing = false;
                transform.position = ClimbDestination;
                ClimbDestination = Vector3.zero;
                rb.isKinematic = false;
            }
        }
    }

    private void LateUpdate()
    {
        if (animator != null)
            animator.SetMoveData(IsGrounded, InputDir, MoveState);
    }

    private void FixedUpdate()
    {
        float determinedSpeed = (IsGrounded ? Speed : (Speed * fallingMovementModifier));
        rb.position += (dir * determinedSpeed * Time.fixedDeltaTime);
        

        if(useBetterFall == true && IsFalling == true)
        {
            rb.AddForce(Vector3.down * betterFallMultiplier, ForceMode.VelocityChange);
        }
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(climbInfo.checkOrigin.position, Vector3.down * climbInfo.rayDist);
        if (ClimbDestination != Vector3.zero)
            Gizmos.DrawCube(ClimbDestination, Vector3.one * 0.2f);

        if (IsGrounded)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.red;

        Gizmos.DrawSphere(transform.position, groundedRadius);
    }

}

[System.Serializable]
public struct PlayerPhysicalDimensions
{
    public float normalHeight;
    public float crouchHeight;
    public Camera camera;
    public Vector3 cameraNormalOffset;
    public Vector3 cameraCrouchOffset;
    public float smoothedLerpSpeed;
}

[System.Serializable]
public struct PlayerMovementStats
{
    public float walkSpeed;
    public float runSpeed;
    public float crouchSpeed;
    public float smoothedLerpSpeed;
}

[System.Serializable]
public struct PlayerClimbingInfo
{
    public Transform checkOrigin;
    public float rayDist;
    public float speed;
    public float finishClimbDistance;
    public float distToEnd;
}

public enum PlayerMoveState
{
    None = 1,
    Walking = 2,
    Running = 3,
    Crouching = 4,
    CrouchWalking = 5,
}
