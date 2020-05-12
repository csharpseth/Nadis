using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AgilityController : MonoBehaviour
{
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

    private void Update()
    {
        grounded = Physics.CheckSphere(transform.position, groundCheckRadius, groundMask);

        if(Inp.Move.JumpDown && grounded)
        {
            rb.velocity = move.Dir;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        
        move.canMove = grounded;
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

        /*
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit, 0.5f, groundMask))
        {
            Vector3 pos = rb.position;
            pos.y = hit.point.y;
            rb.position = pos;
        }
        */
    }

}
