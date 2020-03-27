using System.Collections;
using System.Collections.Generic;
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
    bool falling;
    bool climbing;
    bool grounded;
    bool crouching;

    float speed;
    Vector3 dir;
    Rigidbody rb;
    CapsuleCollider physicsCollider;
    Vector3 climbDestination;
    Vector2 inputDir;

    public Vector2 InputDir { get { return inputDir; } }

    float prevHeight;

    float time = 0f;
    Vector3 prevPosition;

    [SerializeField]
    private PlayerMoveState moveState;

    public bool CanCrouch
    {
        get
        {
            return (grounded == true && moveState != PlayerMoveState.Running);
        }
    }
    public bool CanWalk
    {
        get
        {
            return (grounded == true && climbing == false);
        }
    }
    public bool CanRun
    {
        get
        {
            return (climbing == false && grounded == true && moveState != PlayerMoveState.Crouching);
        }
    }
    public bool CanClimb
    {
        get
        {
            return (moveState != PlayerMoveState.Crouching);
        }
    }

    public bool IsCrouching { get { return crouching; } }
    public bool IsGrounded { get { return grounded; } }
    public bool IsClimbing { get { return climbing; } }
    public bool IsFalling { get { return falling; } }

    public PlayerMoveState MoveState { get { return moveState; } }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        physicsCollider = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        dir = Vector3.zero;
        dir += transform.forward * inputDir.y;
        dir += transform.right * inputDir.x;

        Collider[] cols = Physics.OverlapSphere(transform.position, groundedRadius, groundedMask);
        grounded = (cols.Length > 0);


        if (Input.GetButtonDown("Jump"))
        {
            Climb();
            Jump();
        }

        Climbing();
        Crouching();
        
        physicsCollider.center = new Vector3(0f, (physicsCollider.height / 2f), 0f);

        if (Input.GetButton("Sprint"))
        {
            Running();
        }else if(dir != Vector3.zero)
        {
            Walking();
        }

        if(dir == Vector3.zero && moveState != PlayerMoveState.Crouching)
        {
            moveState = PlayerMoveState.None;
        }

        //rbController.SetSpeed(speed);
    }

    private void Climb()
    {
        if (CanClimb == false)
        {
            Debug.Log("Cannot Climb");
            return;
        }

        if (climbing == false)
        {
            RaycastHit hit;
            if (Physics.Raycast((climbInfo.checkOrigin.position), Vector3.down, out hit, climbInfo.rayDist))
            {
                climbDestination = hit.point;
            }
            else
            {
                climbDestination = Vector3.zero;
            }
        }
    }

    private void Jump()
    {
        if (climbing == true)
            return;
        
        if (transform.position.y != prevHeight)
        {
            if (prevHeight > transform.position.y)
            {
                if (!grounded) falling = true;
                else falling = false;
            }
            prevHeight = transform.position.y;
        }


        if (grounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            rb.velocity = (dir * speed * 0.8f);
        }
    }

    private void Walking()
    {
        if (CanWalk == false)
            return;
        if(moveState != PlayerMoveState.Crouching)
            moveState = PlayerMoveState.Walking;
        speed = Mathf.Lerp(speed, moveInfo.walkSpeed, moveInfo.smoothedLerpSpeed * Time.deltaTime);
    }

    private void Running()
    {
        if (CanRun == false)
            return;
        
        moveState = PlayerMoveState.Running;
        speed = Mathf.Lerp(speed, moveInfo.runSpeed, moveInfo.smoothedLerpSpeed * Time.deltaTime);
    }

    private void Crouching()
    {
        if (Input.GetButton("Crouch") && CanCrouch)
        {
            moveState = PlayerMoveState.Crouching;
            crouching = true;
        }

        if (CanCrouch == false)
        {
            Debug.Log("Cannot Crouch");
            return;
        }

        

        if (moveState != PlayerMoveState.Crouching)
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
            moveState = PlayerMoveState.None;
            crouching = false;
            return;
        }


        speed = Mathf.Lerp(speed, moveInfo.crouchSpeed, moveInfo.smoothedLerpSpeed * Time.deltaTime);
        physicsCollider.height = Mathf.Lerp(physicsCollider.height, sizeInfo.crouchHeight, sizeInfo.smoothedLerpSpeed * Time.deltaTime);
        speed = Mathf.Lerp(speed, moveInfo.crouchSpeed, moveInfo.smoothedLerpSpeed * Time.deltaTime);
        if (sizeInfo.camera != null)
        {
            sizeInfo.camera.transform.localPosition = Vector3.Lerp(sizeInfo.camera.transform.localPosition, sizeInfo.cameraCrouchOffset, sizeInfo.smoothedLerpSpeed * Time.deltaTime);
        }
    }

    private void Climbing()
    {
        climbing = (climbDestination != Vector3.zero);

        if (climbing && climbDestination != Vector3.zero)
        {
            Vector3 temp = transform.position;
            if((climbDestination - temp).sqrMagnitude > (climbInfo.distToEnd * climbInfo.distToEnd))
            {
                climbing = false;
                transform.position = climbDestination;
                climbDestination = Vector3.zero;
                rb.isKinematic = false;
                return;
            }

            if (Mathf.Abs(climbDestination.y - temp.y) > 0.2f)
            {
                temp.y = Mathf.Lerp(temp.y, climbDestination.y, climbInfo.speed * Time.deltaTime);
            }
            else
            {
                temp = Vector3.Lerp(temp, climbDestination, climbInfo.speed * Time.deltaTime);
            }
            //rbController.SetLeftHandPosition(climbDestination);
            //rbController.SetRightHandPosition(climbDestination);
            transform.position = temp;

            float sqrDist = (climbDestination - temp).sqrMagnitude;

            if(Mathf.Abs(climbDestination.y - temp.y) <= yDistToStopGrabbing)
            {
                //rbController.ResetHands();
            }

            if (sqrDist <= (climbInfo.finishClimbDistance * climbInfo.finishClimbDistance))
            {
                climbing = false;
                transform.position = climbDestination;
                climbDestination = Vector3.zero;
                rb.isKinematic = false;
            }
        }
    }


    private void FixedUpdate()
    {
        float determinedSpeed = (grounded ? speed : (speed * fallingMovementModifier));
        rb.position += (dir * determinedSpeed * Time.fixedDeltaTime);
        

        if(useBetterFall == true && falling == true)
        {
            rb.AddForce(Vector3.down * betterFallMultiplier, ForceMode.VelocityChange);
        }
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(climbInfo.checkOrigin.position, Vector3.down * climbInfo.rayDist);
        if (climbDestination != Vector3.zero)
            Gizmos.DrawCube(climbDestination, Vector3.one * 0.2f);

        if (grounded)
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
    None,
    Walking,
    Running,
    Crouching
}
