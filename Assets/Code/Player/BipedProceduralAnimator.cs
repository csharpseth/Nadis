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

    private Vector3 nextRightHand;
    private Vector3 nextLeftHand;
    private Vector3 defaultRightHand;
    private Vector3 defaultLeftHand;
    public bool handTargetPersistent = false;
    public bool overrideNormalHandCondition = false;

    private Vector3 defaultHead;
    private Vector3 defaultChest;
    private Vector3 defaultRightHip;
    private Vector3 defaultLeftHip;


    private LerpData rightFootLerp;
    private LerpData leftFootLerp;

    private Rigidbody rightHandRB;
    private Rigidbody leftHandRB;
    private Joint rightHandJoint;
    private Joint leftHandJoint;
    
    public float maxHandTargetDistFromHand = 0.1f;

    private bool right = true;
    private bool left = false;

    private Vector3 lastPos;

    public Transform RightHand { get { return rightHand; } }
    public Transform LeftHand { get { return leftHand; } }

    public bool grounded = true;
    public Vector2 inputDir;
    public PlayerMoveState moveState;
    

    public bool moving = false;

    private void Awake()
    {
        InitialSetup();

    }

    private void Update()
    {
        RootMomentum();
        if(overrideNormalHandCondition == false)
        {
            DefaultHandAction();
        }
        
        if(grounded == false)
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

        defaultRightHand = rightHand.localPosition;
        defaultLeftHand = leftHand.localPosition;

        rightHandRB = rightHand.GetComponent<Rigidbody>();
        leftHandRB = leftHand.GetComponent<Rigidbody>();
    }

    public void FootStepping()
    {
        if(moving == false)
        {
            rightFoot.localPosition = defaultRight;
            leftFoot.localPosition = defaultLeft;

            Vector3 tempR = rightFoot.position;
            Vector3 tempL = leftFoot.position;

            tempR.y = GroundHeight();
            tempL.y = GroundHeight();

            rightFoot.position = tempR;
            leftFoot.position = tempL;
        }

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
                        Vector3 origin = transform.position + (transform.forward * (data.stepData.stepSize * inputDir.y)) + transform.up + (transform.right * inputDir.x * data.stepData.sideStepSize);

                        RaycastHit rightHit;
                        Physics.Raycast(origin, -transform.up, out rightHit, 1.5f);
                        if (rightHit.transform != null)
                        {
                            nextRight = rightHit.point;
                            rightFootLerp = new LerpData(WorldToLocal(nextRight, transform),  rightFoot.localPosition, data.stepData.footMoveSpeed, data.stepData.stepHeight, (data.stepData.stepSize * inputDir.y));
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
                        Vector3 origin = transform.position + (transform.forward * (data.stepData.stepSize * inputDir.y)) + transform.up + (transform.right * inputDir.x * data.stepData.sideStepSize);

                        RaycastHit leftHit;
                        Physics.Raycast(origin, -transform.up, out leftHit, 1.5f);
                        if (leftHit.transform != null)
                        {
                            nextLeft = leftHit.point;
                            leftFootLerp = new LerpData(WorldToLocal(nextLeft, transform),  leftFoot.localPosition, data.stepData.footMoveSpeed, data.stepData.stepHeight, (data.stepData.stepSize * inputDir.y));
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
    }

    public void DefaultHandAction()
    {
        if (rightHandRB != null)
            rightHandRB.isKinematic = true;
        if (leftHandRB != null)
            leftHandRB.isKinematic = true;

        Vector3 lPos =  leftHand.localPosition;
        Vector3 rPos =  rightHand.localPosition;

        lPos.z =  rightFoot.localPosition.z * data.moveData.handFootMatchStrength;
        rPos.z =  leftFoot.localPosition.z * data.moveData.handFootMatchStrength;

         leftHand.localPosition = lPos;
         rightHand.localPosition = rPos;
    }

    public void LerpHandTargetPosition()
    {
        bool rightDone = true;
        bool leftDone = true;

        if(nextRightHand != Vector3.zero)
        {
            Vector3 tempRight = rightHand.localPosition;
            tempRight = Vector3.Lerp(tempRight, nextRightHand, data.moveData.handMoveSpeed * Time.deltaTime);
            rightHand.localPosition = tempRight;
            if((nextRightHand - tempRight).sqrMagnitude > (0.1f * 0.1f))
            {
                rightDone = false;
            }else
            {
                nextRightHand = Vector3.zero;

            }
        }

        if (nextLeftHand != Vector3.zero)
        {
            Vector3 tempLeft = LeftHand.localPosition;
            tempLeft = Vector3.Lerp(tempLeft, nextLeftHand, data.moveData.handMoveSpeed * Time.deltaTime);
            leftHand.localPosition = tempLeft;
            if ((nextLeftHand - tempLeft).sqrMagnitude > (0.1f * 0.1f))
            {
                leftDone = false;
            }else
            {
                nextLeftHand = Vector3.zero;
            }
        }

        overrideNormalHandCondition = !(rightDone == true && leftDone == true && handTargetPersistent == false);

    }

    public void SetHandTargetPosition(Vector3 position, Side side, bool persistent = false)
    {
        handTargetPersistent = persistent;

        if(side == Side.Right || side == Side.Both)
        {
            nextRightHand = position;
            overrideNormalHandCondition = true;
        }

        if (side == Side.Left || side == Side.Both)
        {
            nextLeftHand = position;
            overrideNormalHandCondition = true;
        }
    }

    public void SetHandTarget(Rigidbody rb, Side side)
    {
        overrideNormalHandCondition = true;

        if(side == Side.Right || side == Side.Both)
        {
            rightHand.position = rb.transform.position;
            rightHandRB.isKinematic = false;
            rightHandJoint = rightHand.gameObject.AddComponent<FixedJoint>();
            rightHandJoint.connectedBody = rb;
        }

        if(side == Side.Left || side == Side.Both)
        {
            leftHand.position = rb.transform.position;
            leftHandRB.isKinematic = false;
            leftHandJoint = leftHand.gameObject.AddComponent<FixedJoint>();
            leftHandJoint.connectedBody = rb;
        }
    }

    public void EndCurrentHandTarget()
    {
        if (rightHandJoint != null) Destroy(rightHandJoint);
        if (leftHandJoint != null) Destroy(leftHandJoint);
        overrideNormalHandCondition = false;
        rightHand.localPosition = defaultRightHand;
        leftHand.localPosition = defaultLeftHand;
    }

    public void RootMomentum()
    {
        Vector3 rootRot =  root.localEulerAngles;
        float forward = Mathf.LerpAngle(rootRot.x, inputDir.y * data.moveData.maxRootForwardAngle, 5f * Time.deltaTime);
        float side = Mathf.LerpAngle(rootRot.y, inputDir.x * data.moveData.maxRootSideAngle, 5f * Time.deltaTime);

        Vector3 newRot = new Vector3(forward,  root.localEulerAngles.y, side);
         root.localEulerAngles = newRot;
    }

    public void SetMoveData(bool grounded, Vector2 inputDir, PlayerMoveState moveState)
    {
        this.grounded = grounded;
        this.inputDir = inputDir;
        this.moveState = moveState;
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
        if(moveState != PlayerMoveState.None)
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
    public float handMoveSpeed;
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

public enum Side
{
    Right,
    Left,
    Both
}