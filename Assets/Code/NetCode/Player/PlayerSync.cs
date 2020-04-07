using UnityEngine;

public class PlayerSync : MonoBehaviour
{
    public bool Local = false;
    public bool Send = true;
    public float moveThreshold = 2f;
    public float reachedThreshold = 0.2f;
    public float maxDistance = 5f;
    private float moveDataCheckDelay = 0.25f;

    private Vector3 prevPosition = new Vector3(-1000f, 1000f, -1000f);
    private float prevRotation = -180f;
    private int id;
    private bool idSet = false;
    private float lerpSpeed = 10f;
    
    private Vector3 destination;
    private BipedProceduralAnimator animator;
    private MovementController moveController;

    private bool grounded;
    private Vector2 inputDir;
    private PlayerMoveState moveState;
    
    public BipedProceduralAnimator Animator { get { return animator; } }

    public int ID
    {
        get { return id; }
        set
        {
            if(idSet == false)
            {
                id = value;
                idSet = true;

                SubscribeEvents();
            }
        }
    }

    public bool isLocal
    {
        get
        {
            return (ID == NetworkManager.LocalPlayer.ID);
        }
    }

    private void Awake()
    {
        if (Local && NetworkManager.LocalPlayer == null)
            NetworkManager.LocalPlayer = this;
        animator = GetComponent<BipedProceduralAnimator>();
        if (Send)
        {
            moveController = GetComponent<MovementController>();
        }
    }
    
    private void Update()
    {
        if (idSet == false) return;

        if (Send == false)
        {
            if(destination != Vector3.zero)
            {
                transform.position = Vector3.Lerp(transform.position, destination, lerpSpeed * Time.deltaTime);
                if((destination - transform.position).sqrMagnitude <= (reachedThreshold * reachedThreshold))
                {
                    transform.position = destination;
                    destination = Vector3.zero;
                }
            }

            return;
        }

        if((transform.position - prevPosition).sqrMagnitude >= (moveThreshold * moveThreshold))
        {
            prevPosition = transform.position;
            Events.Player.OnMove?.Invoke(ID, prevPosition);
        }

        if(transform.eulerAngles.y != prevRotation)
        {
            prevRotation = transform.eulerAngles.y;
            Events.Player.OnRotate?.Invoke(ID, prevRotation);
        }

        UpdateMoveData();
    }
    
    float moveDataTimer = 0f;
    private void SubscribeEvents()
    {
        Events.Player.SetPos += SetPosition;
        Events.Player.SetRot += SetRotation;
        Events.Player.Disconnect += Disconnected;
    }

    private void UnSubscribeEvents()
    {
        Events.Player.SetPos -= SetPosition;
        Events.Player.SetRot -= SetRotation;
        Events.Player.Disconnect -= Disconnected;
    }

    private void UpdateMoveData()
    {
        if (moveController == null)
            return;
        moveDataTimer += Time.deltaTime;
        if(moveDataTimer >= moveDataCheckDelay)
        {
            Events.Player.OnMoveData(id, moveController.IsGrounded, moveController.InputDir, moveController.MoveState, moveController.Speed);
            moveDataTimer = 0f;
        }
    }

    public void SetPosition(int playerID, Vector3 newPosition)
    {
        if (playerID != ID) return;

        destination = newPosition;
    }

    public void SetRotation(int playerID, float newRotation)
    {
        if (playerID != ID) return;

        Vector3 temp = transform.eulerAngles;
        temp.y = newRotation;
        transform.eulerAngles = temp;
    }

    public void SetProceduralMoveData(bool grounded, Vector2 inputDir, PlayerMoveState moveState, float moveSpeed)
    {
        lerpSpeed = moveSpeed;
        if(animator != null)
        {
            animator.SetMoveData(grounded, inputDir, moveState);
        }
    }
    
    public void Disconnected(int playerID)
    {
        if (playerID != ID) return;

        //Later, replace this with a sleeper or Death function( the player ragdolls and drops all items )
        UnSubscribeEvents();
        Destroy(gameObject);
    }
}
