using System;
using System.Collections.Generic;
using UnityEngine;

public class BipedProceduralAnimator : MonoBehaviour
{
    [Header("Configuration:")]
    public ProceduralAnimationData data;
    public AnimationConfig animations;
    private PlayerSync player;

    [Space(10)]
    [Header("Targets:")]
    #region Targets
    public Transform targets;
    public Target root;
    public Target head;
    public Target chest;
    public Target pelvis;
    public Target rightHip;
    public Target leftHip;
    public FootTarget rightFoot;
    public FootTarget leftFoot;
    public Target rightHand;
    public Target leftHand;
    #endregion
    
    #region Player Move Data
    private bool grounded = true;
    private Vector2 inputDir;
    private PlayerMoveState moveState;
    #endregion

    [Header("Other:")]
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
        if(rightHand.lerp == null)
        {
            RightArmSwing();
        }else
        {
            if (rightHand.lerp.Done == false)
            {
                rightHand.lerp.DoLerp();
            }
            else
            {
                rightHand.Reset();
            }
        }

        if(leftHand.lerp == null)
        {
            LeftArmSwing();
        }else
        {
            if (leftHand.lerp.Done == false)
                leftHand.lerp.DoLerp();
            else
            {
                leftHand.Reset();
            }
        }
        
        if(pelvis.lerp != null)
        {
            if (pelvis.lerp.Done == false)
            {
                pelvis.lerp.DoLerp();
            }
            else
            {
                pelvis.Reset();
            }
        }

        if(grounded == false)
        {
            rightFoot.Reset();
            leftFoot.Reset();
            right = true;
            left = false;
        }

        FootStepping();
    }

    private void InitialSetup()
    {
        nextRight =  rightFoot.position;
        nextLeft =  leftFoot.position;

        rightFoot.Init();
        leftFoot.Init();
        head.Init();
        chest.Init();
        pelvis.Init();
        rightHand.Init();
        leftHand.Init();
        
        player = GetComponent<PlayerSync>();

        Events.BipedAnimator.SetHandTargetPosition += SetHandTargetPosition;
        Events.BipedAnimator.EndCurrentHandTarget += EndCurrentHandTarget;
        Events.BipedAnimator.ExecuteAnimation += ExecuteAnimation;
        Events.BipedAnimator.EndAnimation += EndAnimation;
        Events.Player.UnSubscribe += UnSubscribe;
    }

    private void UnSubscribe(int playerID)
    {
        if (player.ID != playerID) return;

        Events.BipedAnimator.SetHandTargetPosition -= SetHandTargetPosition;
        Events.BipedAnimator.EndCurrentHandTarget -= EndCurrentHandTarget;
        Events.BipedAnimator.ExecuteAnimation -= ExecuteAnimation;
        Events.BipedAnimator.EndAnimation -= EndAnimation;
        Events.Player.UnSubscribe -= UnSubscribe;
    }

    public void FootStepping()
    {
        if(moving == false)
        {
            rightFoot.Reset();
            leftFoot.Reset();

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
                if (rightFoot.lerp == null || rightDist >= sqrDist || rightFoot.lerp.done == false)
                {
                    if (rightFoot.lerp == null)
                    {
                        rightFoot.lerp = new FootLerpData();
                        rightFoot.lerp.done = true;
                    }
                    else
                    {
                        rightFoot.lerp.done = false;
                         rightFoot.localPosition = rightFoot.lerp.DoLerpFrom( rightFoot.localPosition);
                    }

                    if (rightFoot.lerp.done == true)
                    {
                        Events.BipedAnimator.OnRightFootFinishStep?.Invoke(player.ID);

                        Vector3 origin = ConvertDir(data.stepData.rightFootRayOffset, inputDir, data.stepData.stepSize, data.stepData.sideStepSize);

                        RaycastHit rightHit;
                        Physics.Raycast(origin, -transform.up, out rightHit, data.stepData.stepRayLength);
                        if (rightHit.transform != null)
                        {
                            nextRight = rightHit.point;
                            rightFoot.lerp = new FootLerpData(WorldToLocal(nextRight, transform),  rightFoot.localPosition, data.stepData.footMoveSpeed, data.stepData.stepHeight, (data.stepData.stepSize * inputDir.y));
                            right = false;
                            left = true;
                            Events.BipedAnimator.OnLeftFootBeginStep?.Invoke(player.ID);
                        }
                    }else
                    {
                        Events.BipedAnimator.OnRightFootStepping?.Invoke(player.ID);
                    }

                }
            }

            if (left == true)
            {
                if (leftFoot.lerp == null || leftDist >= sqrDist || leftFoot.lerp.done == false)
                {
                    if (leftFoot.lerp == null)
                    {
                        leftFoot.lerp = new FootLerpData();
                        leftFoot.lerp.done = true;
                    }
                    else
                    {
                        leftFoot.lerp.done = false;
                         leftFoot.localPosition = leftFoot.lerp.DoLerpFrom( leftFoot.localPosition);
                    }

                    if (leftFoot.lerp.done == true)
                    {
                        Events.BipedAnimator.OnLeftFootFinishStep?.Invoke(player.ID);

                        Vector3 origin = ConvertDir(data.stepData.leftFootRayOffset, inputDir, data.stepData.stepSize, data.stepData.sideStepSize);

                        RaycastHit leftHit;
                        Physics.Raycast(origin, -transform.up, out leftHit, data.stepData.stepRayLength);
                        if (leftHit.transform != null)
                        {
                            nextLeft = leftHit.point;
                            leftFoot.lerp = new FootLerpData(WorldToLocal(nextLeft, transform),  leftFoot.localPosition, data.stepData.footMoveSpeed, data.stepData.stepHeight, (data.stepData.stepSize * inputDir.y));
                            left = false;
                            right = true;
                            Events.BipedAnimator.OnRightFootBeginStep?.Invoke(player.ID);
                        }
                    }else
                    {
                        Events.BipedAnimator.OnLeftFootStepping?.Invoke(player.ID);
                    }
                }
            }

            if (rightFoot.lerp != null && rightFoot.lerp != null && data.doBob == true)
            {
                float bobPercent = ((rightFoot.lerp.currentHeight + leftFoot.lerp.currentHeight) / 2f) / rightFoot.lerp.height;
                float bobModifier = data.moveData.bobCurve.Evaluate(bobPercent);

                Vector3 newHead = head.defaultPosition;
                Vector3 newChest = chest.defaultPosition;
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
    
    private Vector3 ConvertDir(Vector3 input, Vector2 inputDir, float stepSize, float sideStepSize)
    {
        Vector3 forw = transform.forward;
        Vector3 right = transform.right;

        Vector3 temp = Vector3.zero;

        temp += ((forw * stepSize) * inputDir.y) + (forw * input.z);
        temp += ((right * sideStepSize) * inputDir.x) + (right * input.x);
        temp += Vector3.up;

        temp += transform.position;

        return temp;
    }

    private void RightArmSwing()
    {
        Vector3 rPos = rightHand.defaultPosition;
        
        rPos.z = leftFoot.localPosition.z * data.moveData.handFootMatchStrength;
        rightHand.localPosition = rPos;
    }
    private void LeftArmSwing()
    {
        Vector3 lPos = leftHand.defaultPosition;

        lPos.z = rightFoot.localPosition.z * data.moveData.handFootMatchStrength;
        leftHand.localPosition = lPos;
    }

    public void SetHandTargetPosition(int playerID, Vector3 position, Side side, float speed = 0f, AnimatorTarget target = AnimatorTarget.None, bool persistent = false, bool send = true)
    {
        if (playerID != player.ID) return;

        Transform parent = null;

        if(target == AnimatorTarget.Head)
        {
            parent = head.obj;
        }else if(target == AnimatorTarget.Chest)
        {
            parent = chest.obj;
        }

        if(parent != null)
        {
            if(side == Side.Right || side == Side.Both && rightHand.lerp == null)
            {
                rightHand.SetParent(parent);

                rightHand.lerp = new LerpData(rightHand, position, speed, true, true, null);
                
                rightHand.localRotation = Quaternion.identity;
            }

            if (side == Side.Left || side == Side.Both && leftHand.lerp == null)
            {
                leftHand.SetParent(parent);

                leftHand.lerp = new LerpData(leftHand, position, speed, true, true, null);

                leftHand.localRotation = Quaternion.identity;
            }
        }
    }
    public void SetHandTargetPositions(Vector3[] positions, Side side, float speed = 0f, Transform parent = null, bool local = true)
    {
        if (side == Side.Right || side == Side.Both && rightHand.lerp == null)
        {
            if (parent != null)
                rightHand.SetParent(parent);
            
            rightHand.lerp = new LerpData(rightHand, positions, speed, local, targets);
            rightHand.localRotation = Quaternion.identity;
        }

        if (side == Side.Left || side == Side.Both && leftHand.lerp == null)
        {
            if (parent != null)
                leftHand.SetParent(parent);

            leftHand.lerp = new LerpData(leftHand, positions, speed, local, targets);
            leftHand.localRotation = Quaternion.identity;
        }
    }
    public void EndCurrentHandTarget(int playerID, bool send = true)
    {
        if (playerID != player.ID) return;

        rightHand.Reset();
        leftHand.Reset();
    }

    public void ExecuteAnimation(int playerID, string identifier, Action endEvent = null)
    {
        if (player.ID != playerID) return;

        AnimationConfig.ConfigData config = animations.GetAnimation(identifier);
        Debug.Log(config.identifier);
        Target target = TargetFrom(config.target);
        
        target.lerp = new LerpData(target, config.localPoints, config.speed, true, config.persistent, endEvent);
        target.lerp.Animation = config.identifier;
    }
    public void EndAnimation(int playerID, string identifier)
    {
        if (player.ID != playerID) return;

        AnimationConfig.ConfigData anim = animations.GetAnimation(identifier);
        Target t = TargetFrom(anim.target);
        if (t.Null) return;
        if (t.lerp != null && anim.identifier == t.lerp.Animation)
            t.Reset();
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
    private Target TargetFrom(AnimatorTarget target, Side side = Side.Both)
    {
        switch (target)
        {
            case AnimatorTarget.None:
                return Target.Empty;
            case AnimatorTarget.Head:
                return head;
            case AnimatorTarget.Chest:
                return chest;
            case AnimatorTarget.Pelvis:
                return pelvis;
            case AnimatorTarget.Hands:
                if (side == Side.Right)
                    return rightHand;
                else if (side == Side.Left)
                    return leftHand;
                break;
            default:
                break;
        }

        return Target.Empty;
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

public class FootLerpData
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

    public FootLerpData()
    {

    }
    public FootLerpData(Vector3 destination, Vector3 origin, float speed, float height, float stepSize)
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

[Serializable]
public struct ProceduralStepData
{
    public float footMoveSpeed;
    public float stepSize;
    public float sideStepSize;
    public float stepHeight;
    public float maxDistBeforeNextStep;
    public float stepRayLength;
    public Vector3 rightFootRayOffset;
    public Vector3 leftFootRayOffset;
}

[Serializable]
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
    Chest,
    Pelvis,
    Hands
}

public class LerpData
{
    private float dist;
    private const float FinishThreshold = (0.1f * 0.1f);
    private Queue<Vector3> posQueue;
    private Vector3 nextPos;
    private float speed;
    private Target target;
    private Action doneCallback;
    private bool local;
    private bool persistent;
    private Vector3 finalDestination;

    public string Animation { get; set; }
    public bool Done { get; private set; }

    public LerpData(Target tar, Vector3 pos, float speed, bool local = true, bool persistent = false, Action doneCallback = null)
    {
        target = tar;
        nextPos = pos;
        finalDestination = pos;
        this.speed = speed;
        this.local = local;
        this.doneCallback = doneCallback;
        this.persistent = persistent;
    }
    public LerpData(Target tar, Vector3[] pos, float speed, bool local = true, bool persistent = false, Action doneCallback = null)
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

        finalDestination = pos[pos.Length - 1];
        
        this.local = local;
        this.doneCallback = doneCallback;
        this.persistent = persistent;
    }

    public void DoLerp()
    {
        if(posQueue == null || posQueue.Count == 0)
        {
            if(nextPos == Vector3.zero && persistent == false)
            {
                Done = true;
                doneCallback?.Invoke();
                return;
            }
        }
        
        SetNextPosition();

        if (local)
            LocalLerp();
        else
            GlobalLerp();
    }

    private bool SetNextPosition()
    {
        if (nextPos != Vector3.zero)
            return false;
        if (posQueue == null || posQueue.Count == 0)
            return false;

        nextPos = posQueue.Dequeue();
        return true;

    }

    private void LocalLerp()
    {
        if (nextPos == Vector3.zero)
            return;

        target.localPosition = Vector3.Lerp(target.localPosition, nextPos, speed * Time.deltaTime);
        dist = (nextPos - target.localPosition).sqrMagnitude;
        
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

[Serializable]
public class AnimationConfig
{
    [Serializable]
    public struct ConfigData
    {
        public string identifier;
        public Vector3[] localPoints;
        public float speed;
        public bool persistent;
        public AnimatorTarget target;
    }
    public ConfigData[] configs;

    public ConfigData GetAnimation(string identifier)
    {
        for (int i = 0; i < configs.Length; i++)
        {
            if (configs[i].identifier.ToLower() == identifier.ToLower())
            {
                return configs[i];
            }
        }

        return default(ConfigData);
    }
}

[Serializable]
public class Target
{
    public Transform obj;
    [HideInInspector]
    public Vector3 defaultPosition;
    [HideInInspector]
    public Transform defaultParent;
    [HideInInspector]
    public LerpData lerp;

    public bool Lerping { get { return (lerp != null); } }
    public bool Null { get { return !obj; } }

    public Vector3 localPosition
    {
        get
        {
            return obj.localPosition;
        }
        set
        {
            obj.localPosition = value;
        }
    }
    public Vector3 position { get { return obj.position; } set { obj.position = value; } }
    public Quaternion rotation { get { return obj.rotation; } set { obj.rotation = value; } }
    public Quaternion localRotation { get { return obj.localRotation; } set { obj.localRotation = value; } }

    public Vector3 localEulerAngles { get { return obj.localEulerAngles; } set { obj.localEulerAngles = value; } }
    public void SetParent(Transform t) { obj.SetParent(t); }
    
    public static Target Empty { get { return default(Target); } }

    public void Init()
    {
        defaultPosition = obj.localPosition;
        defaultParent = obj.parent;
    }
    public void Reset()
    {
        SetParent(defaultParent);
        obj.localPosition = defaultPosition;
        lerp = null;
    }
}

[Serializable]
public struct FootTarget
{
    public Transform obj;
    [HideInInspector]
    public Vector3 defaultPosition;
    [HideInInspector]
    public Transform defaultParent;
    [HideInInspector]
    public FootLerpData lerp;

    public bool Lerping { get { return (lerp != null); } }

    public Vector3 localPosition
    {
        get
        {
            return obj.localPosition;
        }
        set
        {
            obj.localPosition = value;
        }
    }
    public Vector3 position { get { return obj.position; } set { obj.position = value; } }
    public Quaternion rotation { get { return obj.rotation; } set { obj.rotation = value; } }
    public Quaternion localRotation { get { return obj.localRotation; } set { obj.localRotation = value; } }

    public Vector3 localEulerAngles { get { return obj.localEulerAngles; } set { obj.localEulerAngles = value; } }
    public void SetParent(Transform t) { obj.SetParent(t); }

    public static Target Empty { get { return default(Target); } }

    public void Init()
    {
        defaultPosition = obj.localPosition;
        defaultParent = obj.parent;
    }
    public void Reset()
    {
        SetParent(defaultParent);
        obj.localPosition = defaultPosition;
        lerp = null;
    }
}