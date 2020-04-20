using System;
using System.Collections.Generic;
using UnityEngine;

public class BipedProceduralAnimator : MonoBehaviour
{
    [Header("Configuration:")]
    public ProceduralAnimationData data;
    public AnimationConfig animations;

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

    //Network Shit...
    public int NetID { get; private set; }

    private void Start()
    {
        if(Nadis.Net.NetworkManager.ins == null)
        {
            InitialSetup();
        }
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

    public void InitialSetup(int netID = 0)
    {
        NetID = netID;

        nextRight =  rightFoot.position;
        nextLeft =  leftFoot.position;

        rightFoot.Init();
        leftFoot.Init();
        head.Init();
        chest.Init();
        pelvis.Init();
        rightHand.Init();
        leftHand.Init();
        
        Events.BipedAnimator.SetHandTargetPosition += SetHandTargetPosition;
        Events.BipedAnimator.EndCurrentHandTarget += EndCurrentHandTarget;
        Events.BipedAnimator.ExecuteAnimation += ExecuteAnimation;
        Events.BipedAnimator.EndAnimation += EndAnimation;
        Events.Player.UnSubscribe += UnSubscribe;
    }

    private void UnSubscribe(int playerID)
    {
        if (NetID != playerID) return;

        Events.BipedAnimator.SetHandTargetPosition -= SetHandTargetPosition;
        Events.BipedAnimator.EndCurrentHandTarget -= EndCurrentHandTarget;
        Events.BipedAnimator.ExecuteAnimation -= ExecuteAnimation;
        Events.BipedAnimator.EndAnimation -= EndAnimation;
        Events.Player.UnSubscribe -= UnSubscribe;
    }

    public void FootStepping()
    {
        if (moving == false && grounded == true)
        {
            rightFoot.Reset();
            leftFoot.Reset();
            Vector3 tempR = rightFoot.position;
            Vector3 tempL = leftFoot.position;

            tempR.y = GroundHeight(rightFoot);
            tempL.y = GroundHeight(leftFoot);

            rightFoot.position = tempR;
            leftFoot.position = tempL;
        }else if(moving == false)
        {
            rightFoot.Reset();
            leftFoot.Reset();
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
                        Events.BipedAnimator.OnRightFootFinishStep?.Invoke(NetID);

                        Vector3 origin = ConvertDir(data.stepData.rightFootRayOffset, inputDir, data.stepData.stepSize, data.stepData.sideStepSize);

                        RaycastHit rightHit;
                        Physics.Raycast(origin, -transform.up, out rightHit, data.stepData.stepRayLength);
                        if (rightHit.transform != null)
                        {
                            nextRight = rightHit.point;
                            rightFoot.lerp = new FootLerpData(WorldToLocal(nextRight, transform),  rightFoot.localPosition, data.stepData.footMoveSpeed, data.stepData.stepHeight, (data.stepData.stepSize * inputDir.y));
                            right = false;
                            left = true;
                            Events.BipedAnimator.OnLeftFootBeginStep?.Invoke(NetID);
                        }
                    }else
                    {
                        Events.BipedAnimator.OnRightFootStepping?.Invoke(NetID);
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
                        Events.BipedAnimator.OnLeftFootFinishStep?.Invoke(NetID);

                        Vector3 origin = ConvertDir(data.stepData.leftFootRayOffset, inputDir, data.stepData.stepSize, data.stepData.sideStepSize);

                        RaycastHit leftHit;
                        Physics.Raycast(origin, -transform.up, out leftHit, data.stepData.stepRayLength);
                        if (leftHit.transform != null)
                        {
                            nextLeft = leftHit.point;
                            leftFoot.lerp = new FootLerpData(WorldToLocal(nextLeft, transform),  leftFoot.localPosition, data.stepData.footMoveSpeed, data.stepData.stepHeight, (data.stepData.stepSize * inputDir.y));
                            left = false;
                            right = true;
                            Events.BipedAnimator.OnRightFootBeginStep?.Invoke(NetID);
                        }
                    }else
                    {
                        Events.BipedAnimator.OnLeftFootStepping?.Invoke(NetID);
                    }
                }
            }
            
            if (right == false)
            {
                nextRight.y = GroundHeight(rightFoot);
                rightFoot.position = nextRight;
            }

            if (left == false)
            {
                nextLeft.y = GroundHeight(leftFoot);
                leftFoot.position = nextLeft;
            }
            
        }
        
        if (pelvis.lerp == null || pelvis.lerp.Done == true && grounded)
        {
            Vector3 tempPelvis = pelvis.defaultPosition;
            float leftY = (leftFoot.localPosition.y - leftFoot.defaultPosition.y);
            float rightY = (rightFoot.localPosition.y - rightFoot.defaultPosition.y);
            float avgY = (leftY + rightY) / 2f;
            float plyY = 0.9f + avgY;
            tempPelvis.y = plyY;
            pelvis.localPosition = Vector3.Lerp(pelvis.localPosition, tempPelvis, 8f * Time.deltaTime);
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
        if (playerID != NetID) return;

        Transform parent = targets;

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

                rightHand.lerp = new LerpData(rightHand, position, speed, false, true, null);
                
                rightHand.localRotation = Quaternion.identity;
            }

            if (side == Side.Left || side == Side.Both && leftHand.lerp == null)
            {
                leftHand.SetParent(parent);

                leftHand.lerp = new LerpData(leftHand, position, speed, false, true, null);

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
            
            //rightHand.lerp = new LerpData(rightHand, positions, speed, local, targets);
            rightHand.localRotation = Quaternion.identity;
        }

        if (side == Side.Left || side == Side.Both && leftHand.lerp == null)
        {
            if (parent != null)
                leftHand.SetParent(parent);

            //leftHand.lerp = new LerpData(leftHand, positions, speed, local, targets);
            leftHand.localRotation = Quaternion.identity;
        }
    }
    public void EndCurrentHandTarget(int playerID, bool send = true)
    {
        if (playerID != NetID) return;

        rightHand.Reset();
        leftHand.Reset();
    }

    public void ExecuteAnimation(int playerID, string identifier, Action endEvent = null)
    {
        if (NetID != playerID) return;

        AnimationConfig.ConfigData config = animations.GetAnimation(identifier);
        Target target = TargetFrom(config.target);
        
        target.lerp = new LerpData(target, config.localPoints, true, config.persistent, endEvent);
        target.lerp.Animation = config.identifier;
    }
    public void EndAnimation(int playerID, string identifier)
    {
        if (NetID != playerID) return;

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
    
    private float GroundHeight(FootTarget target)
    {
        Vector3 origin = target.position + transform.up;
        RaycastHit hit;
        Physics.Raycast(origin, -transform.up, out hit, 1.5f);

        if(hit.transform != null)
            return hit.point.y;

        return transform.position.y;
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
    private Target TargetFrom(AnimatorTarget target, Side side = Side.Right)
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

    private void OnDrawGizmos()
    {
        /*
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(pelvis.position, 0.2f);
        */

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(rightFoot.position, 0.05f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(leftFoot.position, 0.05f);

        float leftY = (leftFoot.position.y);
        float rightY = (rightFoot.position.y);
        float avgY = (leftY + rightY) / 2f;
        Vector3 pos = transform.position;
        pos.y = avgY + 1f;

        //pelvis.position = pos;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pos, 0.1f);
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
    private Action callback;
    
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
    public FootLerpData(Vector3 destination, Vector3 origin, float speed, float height, float stepSize, Action callback = null)
    {
        this.destination = destination;
        this.initialOrigin = origin;
        this.speed = speed;
        this.height = height;
        this.stepSize = stepSize;
        this.callback = callback;
    }

    public Vector3 DoLerpFrom(Vector3 target)
    {
        if(Close(target, destination) == true)
        {
            done = true;
            callback?.Invoke();
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
    private Queue<LerpPoint> posQueue;
    private LerpPoint nextPos;
    private Target target;
    private Action doneCallback;
    private bool local;
    private bool persistent;
    private LerpPoint finalDestination;

    public string Animation { get; set; }
    public bool Done { get; private set; }

    public LerpData(Target tar, Vector3 pos, float speed, bool local = true, bool persistent = false, Action doneCallback = null)
    {
        target = tar;
        nextPos.position = pos;
        nextPos.speed = speed;
        finalDestination.position = pos;
        this.local = local;
        this.doneCallback = doneCallback;
        this.persistent = persistent;
    }
    public LerpData(Target tar, LerpPoint[] pos, bool local = true, bool persistent = false, Action doneCallback = null)
    {
        target = tar;
        posQueue = new Queue<LerpPoint>();
        for (int i = 0; i < pos.Length; i++)
        {
            posQueue.Enqueue(pos[i]);
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
            if(nextPos.position == Vector3.zero && persistent == false)
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
        if (nextPos.position != Vector3.zero)
            return false;
        if (posQueue == null || posQueue.Count == 0)
            return false;

        nextPos = posQueue.Dequeue();
        return true;

    }

    private void LocalLerp()
    {
        if (nextPos.position == Vector3.zero)
            return;

        target.localPosition = Vector3.Lerp(target.localPosition, nextPos.position, nextPos.speed * Time.deltaTime);
        dist = (nextPos.position - target.localPosition).sqrMagnitude;
        
        if(dist <= FinishThreshold)
        {
            nextPos.position = Vector3.zero;
        }

    }

    private void GlobalLerp()
    {
        if (nextPos.position == Vector3.zero)
            return;

        target.position = Vector3.Lerp(target.position, nextPos.position, nextPos.speed * Time.deltaTime);
        float dist = (nextPos.position - target.position).sqrMagnitude;
        if (dist <= FinishThreshold)
        {
            nextPos.position = Vector3.zero;
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
        public LerpPoint[] localPoints;
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
public struct LerpPoint
{
    public Vector3 position;
    public float speed;
}

[Serializable]
public class Target
{
    public const float ReturnSpeed = 8f;

    public Transform obj;
    [HideInInspector]
    public Vector3 defaultPosition;
    [HideInInspector]
    public Transform defaultParent;
    [HideInInspector]
    public LerpData lerp = null;

    private bool reset = false;

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
        if(reset == true) { lerp = null; reset = false; obj.localPosition = defaultPosition; return; }

        reset = true;
        SetParent(defaultParent);
        lerp = new LerpData(this, defaultPosition, ReturnSpeed, true, false, Reset);
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
        lerp = null;
        SetParent(defaultParent);
        localPosition = defaultPosition;
    }
}