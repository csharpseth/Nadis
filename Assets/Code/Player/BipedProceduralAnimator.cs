using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BipedProceduralAnimator : MonoBehaviour
{
    public float footMoveSpeed = 8f;
    public float stepSize = 0.5f;

    public Transform rightFoot;
    public Transform leftFoot;

    public Vector3 nextRight;
    private Vector3 defaultRight;
    public Vector3 nextLeft;
    private Vector3 defaultLeft;


    public LerpData rightFootLerp;
    public LerpData leftFootLerp;

    public float maxDistBeforeNextStep = 1f;

    public bool right = true;
    public bool left = false;

    private Vector3 lastPos;
    private MovementController moveController;
    public bool moving = false;

    private void Awake()
    {
        nextRight = rightFoot.position;
        nextLeft = leftFoot.position;

        defaultRight = rightFoot.localPosition;
        defaultLeft = leftFoot.localPosition;

        rightFootLerp = null;
        leftFootLerp = null;

        moveController = GetComponent<MovementController>();

    }

    private void Update()
    {
        if(moving)
        {
            float rightDist = (transform.position - rightFoot.position).sqrMagnitude;
            float leftDist = (transform.position - leftFoot.position).sqrMagnitude;
            float sqrDist = (maxDistBeforeNextStep * maxDistBeforeNextStep);

            //Debug.LogFormat("DistToNext:{0}", Vector3.Distance(rightFoot.localPosition, nextRightLocal));
            //Debug.LogFormat("Done:{0}",rightFootLerp.done);

            if (right == true)
            {
                if (rightFootLerp == null || rightDist >= sqrDist || rightFootLerp.done == false)
                {
                    if (rightFootLerp == null)
                    {
                        rightFootLerp = new LerpData();
                        rightFootLerp.done = true;
                    }
                    else
                    {
                        rightFootLerp.done = false;
                        rightFoot.localPosition = rightFootLerp.DoLerpFrom(rightFoot.localPosition);
                    }

                    if (rightFootLerp.done == true)
                    {
                        Vector3 origin = transform.position + (transform.forward * stepSize) + transform.up;

                        RaycastHit rightHit;
                        Physics.Raycast(origin, -transform.up, out rightHit, 1.5f);
                        if (rightHit.transform != null)
                        {
                            nextRight = rightHit.point;
                            rightFootLerp = new LerpData(WorldToLocal(nextRight, transform), rightFoot.localPosition, footMoveSpeed, 0.8f, stepSize);
                            right = false;
                            left = true;
                        }
                    }
                }
            }

            if (left == true)
            {
                if (leftFootLerp == null || leftDist >= sqrDist || leftFootLerp.done == false)
                {
                    if (leftFootLerp == null)
                    {
                        leftFootLerp = new LerpData();
                        leftFootLerp.done = true;
                    }
                    else
                    {
                        leftFootLerp.done = false;
                        leftFoot.localPosition = leftFootLerp.DoLerpFrom(leftFoot.localPosition);
                    }

                    if (leftFootLerp.done == true)
                    {
                        Vector3 origin = transform.position + (transform.forward * stepSize) + transform.up;

                        RaycastHit leftHit;
                        Physics.Raycast(origin, -transform.up, out leftHit, 1.5f);
                        if (leftHit.transform != null)
                        {
                            nextLeft = leftHit.point;
                            leftFootLerp = new LerpData(WorldToLocal(nextLeft, transform), leftFoot.localPosition, footMoveSpeed, 0.8f, stepSize);
                            left = false;
                            right = true;
                        }
                    }
                }
            }

            if (right == false)
            {
                rightFoot.position = nextRight;
            }

            if (left == false)
            {
                leftFoot.position = nextLeft;
            }
        }
        else
        {
            rightFoot.localPosition = defaultRight;
            leftFoot.localPosition = defaultLeft;
        }

        //DebugUIController.ins.AppendNewLine("Right Done:" + rightFootLerp.done + "RightLerp:" + rightFootLerp + "Dist Percent:" + (rightDist / sqrDist) + "\n");
        //DebugUIController.ins.AppendNewLine("Left Done:" + leftFootLerp.done + "LeftLerp:" + leftFootLerp + "Dist Percent:" + (leftDist / sqrDist) + "\n");

        

    }

    private Vector3 WorldToLocal(Vector3 world, Transform t)
    {
        Transform temp = new GameObject("temp").transform;
        temp.SetParent(t);
        temp.position = world;
        Vector3 local = temp.localPosition;

        Destroy(temp.gameObject);

        return local;
    }

    private void LateUpdate()
    {
        //rightRayWheel.Rotate(Vector3.right, Time.time * rayWheelSpeed, Space.Self);
        if(moveController.MoveState != PlayerMoveState.None)
        {
            moving = true;
        }else
        {
            moving = false;
        }
    }

    private void OnDrawGizmos()
    {
        if(rightFootLerp != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(rightFoot.TransformVector(rightFootLerp.Mid), 0.2f);
            Gizmos.DrawWireSphere(rightFoot.TransformVector(rightFootLerp.destination), 0.2f);
            Vector3 origin = transform.position + (transform.forward * stepSize) + transform.up;
            Gizmos.DrawRay(origin, -transform.up * 1.5f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(nextRight, 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(nextLeft, 0.1f);
    }
}

public class LerpData
{
    public const float FinishThreshold = 0.2f;
    public Vector3 destination;
    public Vector3 initialOrigin;
    public float speed;
    public float height;
    public float stepSize;
    private bool midReached = false;
    public bool done = false;
    
    public Vector3 Mid
    {
        get
        {
            Vector3 m = (destination + initialOrigin) / 2f;
            m.y += height;
            return m;
        }
    }

    public LerpData()
    {

    }
    public LerpData(Vector3 destination, Vector3 origin, float speed, float height, float stepSize)
    {
        this.destination = destination;
        this.initialOrigin = origin;
        this.speed = speed;
        this.height = height;
        this.stepSize = stepSize;
    }

    public Vector3 DoLerpFrom(Vector3 target)
    {
        Debug.Log(destination);
        if(Close(target, destination) == true)
        {
            done = true;
        }

        if (midReached == false)
        {
            Vector3 newPos = Vector3.Lerp(target, Mid, speed * Time.deltaTime);

            if(Close(Mid, newPos))
            {
                midReached = true;
            }

            return newPos;
        }
        return Vector3.Lerp(target, destination, speed * Time.deltaTime);

    }

    public bool Close(Vector3 origin, Vector3 destination)
    {
        float sqrDist = (destination - origin).sqrMagnitude;
        return (sqrDist <= (FinishThreshold * FinishThreshold));

    }
}
