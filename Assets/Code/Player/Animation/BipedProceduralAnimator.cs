using System.Collections.Generic;
using UnityEngine;

public class BipedProceduralAnimator : MonoBehaviour
{
    public ProceduralAnimationData data;
    private PlayerSync player;

    #region Targets
    public Transform targets;
    public Transform root;
    public Transform head;
    public Transform chest;
    public Transform rightHip;
    public Transform leftHip;
    public Transform rightFoot;
    public Transform leftFoot;
    public Transform rightHand;
    public Transform leftHand;
    #endregion

    #region Defaults
    private Vector3 defaultRight;
    private Vector3 defaultLeft;

    private Vector3 defaultRightHand;
    private Vector3 defaultLeftHand;

    private Vector3 defaultHead;
    private Vector3 defaultChest;
    private Vector3 defaultRightHip;
    private Vector3 defaultLeftHip;

    #endregion

    #region Lerp Datas
    private LerpData rightFootLerp;
    private LerpData leftFootLerp;

    private HandLerpData rightHandLerp;
    private HandLerpData leftHandLerp;
    #endregion

    #region Player Move Data
    public bool grounded = true;
    public Vector2 inputDir;
    public PlayerMoveState moveState;
    #endregion
    
    #region Other
    public bool handTargetPersistent = false;
    public bool overrideNormalHandCondition = false;

    private Vector3 nextRight;
    private Vector3 nextLeft;

    public float maxHandTargetDistFromHand = 0.1f;

    private bool right = true;
    private bool left = false;

    public bool moving = false;
    #endregion

    private void Start()
    {
        InitialSetup();
    }
    private void Update()
    {
        RootMomentum();
        if(rightHandLerp == null)
        {
            RightArmSwing();
        }else
        {
            if (rightHandLerp.Done == false)
            {
                rightHandLerp.DoLerp();
            }
            else
            {
                rightHandLerp = null;
                rightHand.localPosition = defaultRightHand;
            }
        }

        if(leftHandLerp == null)
        {
            LeftArmSwing();
        }else
        {
            if (leftHandLerp.Done == false)
                leftHandLerp.DoLerp();
            else
            {
                leftHandLerp = null;
                leftHand.localPosition = defaultLeftHand;
            }
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

        player = GetComponent<PlayerSync>();

        Events.BipedAnimator.SetHandTargetPosition += SetHandTargetPosition;
        Events.BipedAnimator.EndCurrentHandTarget += EndCurrentHandTarget;
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

        if (moving == true)
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
                        Events.BipedAnimator.OnRightFootFinishStep?.Invoke();
                        Vector3 origin = transform.position + (transform.forward * (data.stepData.stepSize * inputDir.y)) + transform.up + (transform.right * inputDir.x * data.stepData.sideStepSize);

                        RaycastHit rightHit;
                        Physics.Raycast(origin, -transform.up, out rightHit, 1.5f);
                        if (rightHit.transform != null)
                        {
                            nextRight = rightHit.point;
                            rightFootLerp = new LerpData(WorldToLocal(nextRight, transform),  rightFoot.localPosition, data.stepData.footMoveSpeed, data.stepData.stepHeight, (data.stepData.stepSize * inputDir.y));
                            right = false;
                            left = true;
                            Events.BipedAnimator.OnLeftFootBeginStep?.Invoke();
                        }
                    }else
                    {
                        Events.BipedAnimator.OnRightFootStepping?.Invoke(Mathf.Abs(rightHip.localRotation.x));
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
                        Events.BipedAnimator.OnLeftFootFinishStep?.Invoke();
                        Vector3 origin = transform.position + (transform.forward * (data.stepData.stepSize * inputDir.y)) + transform.up + (transform.right * inputDir.x * data.stepData.sideStepSize);

                        RaycastHit leftHit;
                        Physics.Raycast(origin, -transform.up, out leftHit, 1.5f);
                        if (leftHit.transform != null)
                        {
                            nextLeft = leftHit.point;
                            leftFootLerp = new LerpData(WorldToLocal(nextLeft, transform),  leftFoot.localPosition, data.stepData.footMoveSpeed, data.stepData.stepHeight, (data.stepData.stepSize * inputDir.y));
                            left = false;
                            right = true;
                            Events.BipedAnimator.OnRightFootBeginStep?.Invoke();
                        }
                    }else
                    {
                        Events.BipedAnimator.OnLeftFootStepping?.Invoke(Mathf.Abs(leftHip.localRotation.x));
                    }
                }
            }

            if (leftFootLerp != null && rightFootLerp != null && data.doBob == true)
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
    
    private void RightArmSwing()
    {
        Vector3 rPos = defaultRightHand;
        
        rPos.z = leftFoot.localPosition.z * data.moveData.handFootMatchStrength;
        rightHand.localPosition = rPos;
    }
    private void LeftArmSwing()
    {
        Vector3 lPos = defaultLeftHand;

        lPos.z = rightFoot.localPosition.z * data.moveData.handFootMatchStrength;
        leftHand.localPosition = lPos;
    }

    public void SetHandTargetPosition(int playerID, Vector3 position, Side side, float speed = 0f, AnimatorTarget target = AnimatorTarget.None, bool persistent = false, bool send = true)
    {
        if (playerID != player.ID) return;

        Transform parent = null;

        if(target == AnimatorTarget.Head)
        {
            parent = head;
        }else if(target == AnimatorTarget.Chest)
        {
            parent = chest;
        }

        if(parent != null)
        {
            if(side == Side.Right || side == Side.Both && rightHandLerp == null)
            {
                rightHand.SetParent(parent);

                rightHandLerp = new HandLerpData(rightHand, position, speed, true, true, targets);
                
                rightHand.localRotation = Quaternion.identity;
            }

            if (side == Side.Left || side == Side.Both && leftHandLerp == null)
            {
                leftHand.SetParent(parent);

                leftHandLerp = new HandLerpData(leftHand, position, speed, true, true, targets);

                leftHand.localRotation = Quaternion.identity;
            }
        }
    }
    
    public void SetHandTargetPositions(Vector3[] positions, Side side, float speed = 0f, Transform parent = null, bool local = true)
    {
        if (side == Side.Right || side == Side.Both && rightHandLerp == null)
        {
            if (parent != null)
                rightHand.SetParent(parent);
            
            rightHandLerp = new HandLerpData(rightHand, positions, speed, local, targets);
            rightHand.localRotation = Quaternion.identity;
        }

        if (side == Side.Left || side == Side.Both && leftHandLerp == null)
        {
            if (parent != null)
                leftHand.SetParent(parent);

            leftHandLerp = new HandLerpData(leftHand, positions, speed, local, targets);
            leftHand.localRotation = Quaternion.identity;
        }
    }

    public void EndCurrentHandTarget(int playerID, bool send = true)
    {
        if (playerID != player.ID) return;

        Debug.Log("End Hand Position");

        overrideNormalHandCondition = false;
        handTargetPersistent = false;

        rightHand.SetParent(targets);
        leftHand.SetParent(targets);

        rightHandLerp = null;
        leftHandLerp = null;

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

public enum Side
{
    Right = 1,
    Left  = 2,
    Both  = 3
}

public enum AnimatorTarget
{
    None,
    Head,
    Chest
}

public class HandLerpData
{
    private const float FinishThreshold = (0.05f * 0.05f);
    private Queue<Vector3> posQueue;
    private Vector3 nextPos;
    private float speed;
    private Transform target;
    private Transform defParent;
    private bool local;
    private bool persistent;
    public bool Done { get; private set; }

    public HandLerpData(Transform tar, Vector3 pos, float speed, bool local = true, bool persistent = false, Transform defParent = null)
    {
        target = tar;
        nextPos = pos;
        this.speed = speed;
        this.local = local;
        this.defParent = defParent;
        this.persistent = persistent;
    }
    public HandLerpData(Transform tar, Vector3[] pos, float speed, bool local = true, Transform defParent = null)
    {
        target = tar;
        this.speed = speed;
        posQueue = new Queue<Vector3>();
        for (int i = 0; i < pos.Length; i++)
        {
            if(pos[i] != Vector3.zero)
            {
                posQueue.Enqueue(pos[i]);
            }
        }
        this.local = local;
        this.defParent = defParent;
    }

    public void DoLerp()
    {
        if(posQueue == null || posQueue.Count == 0)
        {
            if(nextPos == Vector3.zero && persistent == false)
            {
                Done = true;
                if(defParent != null)
                    target.SetParent(defParent);
                return;
            }
        }

        SetNextPosition();

        if (local)
            LocalLerp();
        else
            GlobalLerp();
    }

    private void SetNextPosition()
    {
        if (nextPos != Vector3.zero)
            return;
        if (posQueue == null || posQueue.Count == 0)
            return;

        nextPos = posQueue.Dequeue();

    }

    private void LocalLerp()
    {
        if (nextPos == Vector3.zero)
            return;

        target.localPosition = Vector3.Lerp(target.localPosition, nextPos, speed * Time.deltaTime);
        float dist = (nextPos - target.localPosition).sqrMagnitude;
        if(dist <= FinishThreshold)
        {
            nextPos = Vector3.zero;
        }

    }

    private void GlobalLerp()
    {
        if (nextPos == Vector3.zero)
            return;

        target.position = Vector3.Lerp(target.position, nextPos, speed * Time.deltaTime);
        float dist = (nextPos - target.position).sqrMagnitude;
        if (dist <= FinishThreshold)
        {
            nextPos = Vector3.zero;
        }
    }

}