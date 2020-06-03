using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AgilityController : MonoBehaviour, INetworkInitialized, IDisableIfRemotePlayer
{
    public int NetID { get; private set; }


    Rigidbody rb;
    MovementController move;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        move = GetComponent<MovementController>();
    }

    public float jumpForce = 100f;
    public float downForce = 100f;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundMask;

    private bool grounded;
    private bool jumped;

    private bool disabled = false;


    public void InitFromNetwork(int netID)
    {
        NetID = netID;
    }

    int groundCheckSkippedFrames = 0;
    private void Update()
    {
        grounded = Physics.CheckSphere(transform.position, groundCheckRadius, groundMask);

        if(Inp.Move.JumpDown && grounded && jumped == false)
        {
            Events.Player.SetAnimatorTrigger(NetID, "jump");
        }

        if(jumped)
        {
            if (grounded && groundCheckSkippedFrames >= 3)
            {
                Events.Player.SetAnimatorTrigger(NetID, "land");
                jumped = false;
                groundCheckSkippedFrames = 0;
            }
            else
                groundCheckSkippedFrames++;
        }
    }

    float lastY = 0f;

    private void FixedUpdate()
    {
        if(rb.position.y != lastY && grounded == false)
        {
            if(rb.position.y < lastY)
            {
                rb.AddForce(Vector3.down * downForce, ForceMode.Force);
            }
            lastY = rb.position.y;
        }
        
    }

    public void ApplyJumpForce()
    {
        jumped = true;
        rb.velocity = move.Dir;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void Disable(bool disabled)
    {
        this.disabled = disabled;
    }
}
