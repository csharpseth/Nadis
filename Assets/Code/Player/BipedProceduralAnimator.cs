using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BipedProceduralAnimator : MonoBehaviour
{
    public ProceduralAnimationData data;

    public Transform root;
    public Transform head;
    public Transform chest;
    public Transform rightHip;
    public Transform leftHip;
    public Transform rightFoot;
    public Transform leftFoot;
    public Transform rightHand;
    public Transform leftHand;

    private Vector3 nextRight;
    private Vector3 defaultRight;
    private Vector3 nextLeft;
    private Vector3 defaultLeft;

    private Vector3 defaultHead;
    private Vector3 defaultChest;
    private Vector3 defaultRightHip;
    private Vector3 defaultLeftHip;


    private LerpData rightFootLerp;
    private LerpData leftFootLerp;

    
    public bool handsFollowFeet = true;
    public float maxHandTargetDistFromHand = 0.1f;

    private bool right = true;
    private bool left = false;

    private Vector3 lastPos;
    private MovementController moveController;

    public Transform RightHand { get { return rightHand; } }
    public Transform LeftHand { get { return leftHand; } }
    

    public bool moving = false;

    private void Awake()
    {
        InitialSetup();

    }

    private void Update()
    {
        RootMomentum();
        if(handsFollowFeet)
        {
            Hands();
        }
        
        if(moveController.IsGrounded == false)
        {
            rightFootLerp = null;
            leftFootLerp = null;
            right = true;
            left = false;
        }

        FootStepping();
    }

    private void InitialSetup()
    {
        nextRight =  rightFoot.position;
        nextLeft =  leftFoot.position;

        defaultRight =  rightFoot.localPosition;
        defaultLeft =  leftFoot.localPosition;

        //defaultRightHip =  rightHip.localPosition;
        //defaultLeftHip =  leftHip.localPosition;

        defaultHead =  head.localPosition;
        defaultChest =  chest.localPosition;

        rightFootLerp = null;
        leftFootLerp = null;

        moveController = GetComponent<MovementController>();
    }

    public void FootStepping()
    {
        if (moving)
        {
            float rightDist = (transform.position -  rightFoot.position).sqrMagnitude;
            float leftDist = (transform.position -  leftFoot.position).sqrMagnitude;
            float sqrDist = (data.stepData.maxDistBeforeNextStep * data.stepData.maxDistBeforeNextStep);

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
                         rightFoot.localPosition = rightFootLerp.DoLerpFrom( rightFoot.localPosition);
                    }

                    if (rightFootLerp.done == true)
                    {
                        Vector3 origin = transform.position + (transform.forward * (data.stepData.stepSize * moveController.InputDir.y)) + transform.up + (transform.right * moveController.InputDir.x * data.stepData.sideStepSize);

                        RaycastHit rightHit;
                        Physics.Raycast(origin, -transform.up, out rightHit, 1.5f);
                        if (rightHit.transform != null)
                        {
                            nextRight = rightHit.point;
                            rightFootLerp = new LerpData(WorldToLocal(nextRight, transform),  rightFoot.localPosition, data.stepData.footMoveSpeed, data.stepData.stepHeight, (data.stepData.stepSize * moveController.InputDir.y));
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
                         leftFoot.localPosition = leftFootLerp.DoLerpFrom( leftFoot.localPosition);
                    }

                    if (leftFootLerp.done == true)
                    {
                        Vector3 origin = transform.position + (transform.forward * (data.stepData.stepSize * moveController.InputDir.y)) + transform.up + (transform.right * moveController.InputDir.x * data.stepData.sideStepSize);

                        RaycastHit leftHit;
                        Physics.Raycast(origin, -transform.up, out leftHit, 1.5f);
                        if (leftHit.transform != null)
                        {
                            nextLeft = leftHit.point;
                            leftFootLerp = new LerpData(WorldToLocal(nextLeft, transform),  leftFoot.localPosition, data.stepData.footMoveSpeed, data.stepData.stepHeight, (data.stepData.stepSize * moveController.InputDir.y));
                            left = false;
                            right = true;
                        }
                    }
                }
            }

            if (leftFootLerp != null && rightFootLerp != null)
            {
                float bobPercent = ((rightFootLerp.currentHeight + leftFootLerp.currentHeight) / 2f) / rightFootLerp.height;
                float bobModifier = data.moveData.bobCurve.Evaluate(bobPercent);

                Vector3 newHead = defaultHead;
                Vector3 newChest = defaultChest;
                //Vector3 newRightHip =  rightHip.localPosition;
                //Vector3 newLeftHip =  leftHip.localPosition;

                //newRightHip.y = (defaultRightHip.y + bobModifier);
                //newLeftHip.y = (defaultLeftHip.y + bobModifier);

                newHead.y += bobModifier;
                newChest.y += bobModifier;

                 //rightHip.localPosition = newRightHip;
                 //leftHip.localPosition = newLeftHip;

                 head.localPosition = newHead;
                 chest.localPosition = newChest;

            }

            if (right == false)
            {
                nextRight.y = GroundHeight();

                 rightFoot.position = nextRight;
            }

            if (left == false)
            {
                nextLeft.y = GroundHeight();

                 leftFoot.position = nextLeft;
            }
        }
        else
        {


             rightFoot.localPosition = defaultRight;
             leftFoot.localPosition = defaultLeft;

            Vector3 tempR =  rightFoot.position;
            Vector3 tempL =  leftFoot.position;

            tempR.y = GroundHeight();
            tempL.y = GroundHeight();

             rightFoot.position = tempR;
             leftFoot.position = tempL;
        }
    }

    public void Hands()
    {
        Vector3 lPos =  leftHand.localPosition;
        Vector3 rPos =  rightHand.localPosition;

        lPos.z =  rightFoot.localPosition.z * data.moveData.handFootMatchStrength;
        rPos.z =  leftFoot.localPosition.z * data.moveData.handFootMatchStrength;

         leftHand.localPosition = lPos;
         rightHand.localPosition = rPos;
    }

    public void RootMomentum()
    {
        Vector3 rootRot =  root.localEulerAngles;
        float forward = Mathf.LerpAngle(rootRot.x, moveController.InputDir.y * data.moveData.maxRootForwardAngle, 5f * Time.deltaTime);
        float side = Mathf.LerpAngle(rootRot.y, moveController.InputDir.x * data.moveData.maxRootSideAngle, 5f * Time.deltaTime);

        Vector3 newRot = new Vector3(forward,  root.localEulerAngles.y, side);
         root.localEulerAngles = newRot;
    }

    private float GroundHeight()
    {
        Vector3 origin = transform.position + transform.up;
        RaycastHit hit;
        Physics.Raycast(origin, -transform.up, out hit, 1.5f);

        return hit.point.y;
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
            Gizmos.DrawWireSphere( rightFoot.TransformVector(rightFootLerp.Mid), 0.2f);
            Gizmos.DrawWireSphere( rightFoot.TransformVector(rightFootLerp.destination), 0.2f);
            Vector3 origin = transform.position + (transform.forward * data.stepData.stepSize) + transform.up;
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
    public float currentHeight = 0f;
    
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
        if(Close(target, destination) == true)
        {
            done = true;
        }
        currentHeight = target.y;
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

[System.Serializable]
public struct ProceduralStepData
{
    public float footMoveSpeed;
    public float stepSize;
    public float sideStepSize;
    public float stepHeight;
    public float maxDistBeforeNextStep;
}

[System.Serializable]
public struct ProceduralMoveData
{
    public float handFootMatchStrength;
    public AnimationCurve bobCurve;
    public float maxRootForwardAngle;
    public float maxRootSideAngle;
}

[CreateAssetMenu(fileName = "New Procedural Animation Data", menuName = "Procedural Data/Procedural Animation Data")]
public class ProceduralAnimationData : ScriptableObject
{
    public ProceduralStepData stepData;
    public ProceduralMoveData moveData;
}
