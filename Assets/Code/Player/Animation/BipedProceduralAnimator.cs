using System;
using UnityEngine;

public class BipedProceduralAnimator : MonoBehaviour, IEventAccessor, INetworkInitialized
{
    [Header("Configuration:")]
    public ProceduralAnimationData data;
    public AnimationConfig animations;
    [Space(10)]
    public BipedIKTargetGroup targets;

    #region Player Move Data
    public bool grounded;

    public Vector2 inputDir;
    public bool moving;
    private PlayerMoveState moveState;
    #endregion
    [Header("NetworkData:")]
    public int timesToCheclPerSecond = 2;
    private Vector2 lastInputDir;
    private bool lastGrounded;
    private float netSendTimer = 0f;


    [Header("Other:")]
    #region Other
    public bool handTargetPersistent = false;
    public bool overrideNormalHandCondition = false;

    private Vector3 nextRight;
    private Vector3 nextLeft;

    public float maxHandTargetDistFromHand = 0.1f;

    private bool right = true;
    private bool left = false;
    #endregion

    //Performance::
    public Pool<LerpData> lerpDatas;
    public Pool<FootLerpData> footLerpDatas;
    private bool initialized = false;


    //Network Shit...
    public int NetID { get; private set; }
    
    private void Update()
    {
        if (initialized == false || lerpDatas.IsNull || footLerpDatas.IsNull) return;

        RootMomentum();
        if(targets.rightHand.lerp == null)
            RightArmSwing();
        else
            if (targets.rightHand.lerp.Done == false)
                targets.rightHand.lerp.DoLerp();
            else
                targets.rightHand.Reset();

        if(targets.leftHand.lerp == null)
            LeftArmSwing();
        else
            if (targets.leftHand.lerp.Done == false)
                targets.leftHand.lerp.DoLerp();
            else
                targets.leftHand.Reset();
        
        if(targets.pelvis.lerp != null)
            if (targets.pelvis.lerp.Done == false)
                targets.pelvis.lerp.DoLerp();
            else
                targets.pelvis.Reset();

        if(grounded.Equals(false))
        {
            targets.rightFoot.Reset();
            targets.leftFoot.Reset();
            right = true;
            left = false;
        }

        FootStepping();
    }
    public void InitFromNetwork(int netID)
    {
        NetID = netID;
        lerpDatas = new Pool<LerpData>(100, true);
        footLerpDatas = new Pool<FootLerpData>(500, true);



        nextRight =  targets.rightFoot.position;
        nextLeft =  targets.leftFoot.position;

        targets.rightFoot.Init(this);
        targets.leftFoot.Init(this);
        targets.head.Init(this);
        targets.chest.Init(this);
        targets.pelvis.Init(this);
        targets.rightHand.Init(this);
        targets.leftHand.Init(this);
        Subscribe();
        initialized = true;
    }
    
    //Boiler Plate Motion
    public void FootStepping()
    {
        if (footLerpDatas.IsNull) return;

        if (moving == false && grounded.Equals(true))
        {
            targets.rightFoot.Reset();
            targets.leftFoot.Reset();
            Vector3 tempR = targets.rightFoot.position;
            Vector3 tempL = targets.leftFoot.position;

            tempR.y = GroundHeight(targets.rightFoot);
            tempL.y = GroundHeight(targets.leftFoot);

            targets.rightFoot.position = tempR;
            targets.leftFoot.position = tempL;
        }else if(moving == false)
        {
            targets.rightFoot.Reset();
            targets.leftFoot.Reset();
        }

        if(moving)
        {
            float rightDist = (transform.position - targets.rightFoot.position).sqrMagnitude;
            float leftDist = (transform.position - targets.leftFoot.position).sqrMagnitude;
            float sqrDist = (data.stepData.maxDistBeforeNextStep * data.stepData.maxDistBeforeNextStep);
            if(right)
                RightFootStep(rightDist, sqrDist);
            else
            {
                nextRight.y = GroundHeight(targets.rightFoot);
                targets.rightFoot.position = nextRight;
                footLerpDatas.Release(targets.rightFoot.lerp);
            }

            if(left)
                LeftFootStep(leftDist, sqrDist);
            else
            {
                nextLeft.y = GroundHeight(targets.leftFoot);
                targets.leftFoot.position = nextLeft;
                footLerpDatas.Release(targets.leftFoot.lerp);
            }
        }

        if(grounded.Equals(true))
        {
            float leftY = (targets.leftFoot.localPosition.y - targets.leftFoot.defaultPosition.y);
            float rightY = (targets.rightFoot.localPosition.y - targets.rightFoot.defaultPosition.y);
            float avgY = (leftY + rightY) / 2f;
            if (targets.pelvis.lerp == null || targets.pelvis.lerp.Done == true)
            {
                Vector3 tempPelvis = targets.pelvis.defaultPosition;
                float plyY = 0.9f + avgY;
                tempPelvis.y = plyY;
                targets.pelvis.localPosition = Vector3.Lerp(targets.pelvis.localPosition, tempPelvis, 8f * Time.deltaTime);
            }

            if (targets.chest.lerp == null || targets.pelvis.lerp.Done == true)
            {
                Vector3 tempChest = targets.chest.defaultPosition;
                float chestY = 1.7f + avgY;
                tempChest.y = chestY;
                targets.chest.localPosition = Vector3.Lerp(targets.chest.localPosition, tempChest, 2f * Time.deltaTime);
            }
        }else
        {
            targets.pelvis.Reset();
            targets.chest.Reset();
        }

    }
    private void RightFootStep(float rightDist, float sqrDist)
    {
        if (targets.rightFoot.lerp == null || rightDist >= sqrDist || targets.rightFoot.lerp.done == false)
        {
            if (targets.rightFoot.lerp == null)
            {
                targets.rightFoot.lerp = footLerpDatas.GetInstance();
                if (targets.rightFoot.lerp == null) targets.rightFoot.lerp = new FootLerpData();
                targets.rightFoot.lerp.done = true;
            }
            else
            {
                targets.rightFoot.lerp.done = false;
                targets.rightFoot.localPosition = targets.rightFoot.lerp.DoLerpFrom(targets.rightFoot.localPosition);
            }

            if (targets.rightFoot.lerp.done == true)
            {
                Events.BipedAnimator.OnRightFootFinishStep?.Invoke(NetID);

                Vector3 origin = ConvertDir(data.stepData.rightFootRayOffset, inputDir, data.stepData.stepSize, data.stepData.sideStepSize);

                RaycastHit rightHit;
                Physics.Raycast(origin, -transform.up, out rightHit, data.stepData.stepRayLength);
                if (rightHit.transform != null)
                {
                    nextRight = rightHit.point;
                    FootLerpData lerp = new FootLerpData();
                    lerp.Init(WorldToLocal(nextRight, transform), targets.rightFoot.localPosition, data.stepData.footMoveSpeed, data.stepData.stepHeight, (data.stepData.stepSize * inputDir.y));
                    targets.rightFoot.lerp = lerp;
                    right = false;
                    left = true;
                    Events.BipedAnimator.OnLeftFootBeginStep?.Invoke(NetID);
                }
            }
            else
            {
                Events.BipedAnimator.OnRightFootStepping?.Invoke(NetID);
            }

        }
    }
    private void LeftFootStep(float leftDist, float sqrDist)
    {
        if (left == true)
        {
            if (targets.leftFoot.lerp == null || leftDist >= sqrDist || targets.leftFoot.lerp.done == false)
            {
                if (targets.leftFoot.lerp == null)
                {
                    targets.leftFoot.lerp = footLerpDatas.GetInstance();
                    if (targets.leftFoot.lerp == null) targets.leftFoot.lerp = new FootLerpData();
                    targets.leftFoot.lerp.done = true;
                }
                else
                {
                    targets.leftFoot.lerp.done = false;
                    targets.leftFoot.localPosition = targets.leftFoot.lerp.DoLerpFrom(targets.leftFoot.localPosition);
                }

                if (targets.leftFoot.lerp.done == true)
                {
                    Events.BipedAnimator.OnLeftFootFinishStep?.Invoke(NetID);

                    Vector3 origin = ConvertDir(data.stepData.leftFootRayOffset, inputDir, data.stepData.stepSize, data.stepData.sideStepSize);

                    RaycastHit leftHit;
                    Physics.Raycast(origin, -transform.up, out leftHit, data.stepData.stepRayLength);
                    if (leftHit.transform != null)
                    {
                        nextLeft = leftHit.point;
                        FootLerpData lerp = new FootLerpData();
                        lerp.Init(WorldToLocal(nextLeft, transform), targets.leftFoot.localPosition, data.stepData.footMoveSpeed, data.stepData.stepHeight, (data.stepData.stepSize * inputDir.y));
                        targets.leftFoot.lerp = lerp;
                        left = false;
                        right = true;
                        Events.BipedAnimator.OnRightFootBeginStep?.Invoke(NetID);
                    }
                }
                else
                {
                    Events.BipedAnimator.OnLeftFootStepping?.Invoke(NetID);
                }
            }
        }
    }
    private void RightArmSwing()
    {
        Vector3 rPos = targets.rightHand.defaultPosition;
        
        rPos.z = targets.leftFoot.localPosition.z * data.moveData.handFootMatchStrength;
        targets.rightHand.localPosition = rPos;
    }
    private void LeftArmSwing()
    {
        Vector3 lPos = targets.leftHand.defaultPosition;

        lPos.z = targets.rightFoot.localPosition.z * data.moveData.handFootMatchStrength;
        targets.leftHand.localPosition = lPos;
    }

    //Hand IK Interaction Functions ( Use to override the boiler plate for a specific or both hands
    // and move it however specified)
    public void SetHandTargetPosition(int netID, Vector3 position, Side side, float speed = 0f, AnimatorTarget target = AnimatorTarget.None, bool persistent = false)
    {
        if (NetID != netID || lerpDatas.IsNull) return;

        Transform parent = null;
        parent = TargetFrom(target).target;
        if (parent == null)
            parent = targets.targets;

        if(parent != null)
        {
            if(side == Side.Right || side == Side.Both && targets.rightHand.lerp == null)
            {
                if (parent == targets.targets)
                    parent = targets.rightHand.defaultParent;
                targets.rightHand.SetParent(parent);
                targets.rightHand.lerp = lerpDatas.GetInstance();
                targets.rightHand.lerp.Init(targets.rightHand, position, speed, true, true, null);
                targets.rightHand.localRotation = Quaternion.identity;
            }

            if (side == Side.Left || side == Side.Both && targets.leftHand.lerp == null)
            {
                if (parent == targets.targets)
                    parent = targets.leftHand.defaultParent;
                targets.leftHand.SetParent(parent);
                targets.leftHand.lerp = lerpDatas.GetInstance();
                targets.leftHand.lerp.Init(targets.leftHand, position, speed, false, true, null);
                targets.leftHand.localRotation = Quaternion.identity;
            }
        }
    }
    public void SetHandTargetPositions(Vector3[] positions, Side side, float speed = 0f, Transform parent = null, bool local = true)
    {
        if (side == Side.Right || side == Side.Both && targets.rightHand.lerp == null)
        {
            if (parent != null)
                targets.rightHand.SetParent(parent);
            
            //targets.rightHand.lerp = new LerpData(targets.rightHand, positions, speed, local, targets);
            targets.rightHand.localRotation = Quaternion.identity;
        }

        if (side == Side.Left || side == Side.Both && targets.leftHand.lerp == null)
        {
            if (parent != null)
                targets.leftHand.SetParent(parent);

            //targets.leftHand.lerp = new LerpData(targets.leftHand, positions, speed, local, targets);
            targets.leftHand.localRotation = Quaternion.identity;
        }
    }
    public void SetHandTarget(Transform target, Side side, Vector3 offset = default, Vector3 rot = default)
    {
        if (lerpDatas.IsNull) return;

        if (side == Side.Right || side == Side.Both && targets.rightHand.lerp == null)
        {
            targets.rightHand.lerp = lerpDatas.GetInstance();
            targets.rightHand.lerp.Init(targets.rightHand, target, offset, rot);
        }

        if (side == Side.Left || side == Side.Both && targets.leftHand.lerp == null)
        {
            targets.leftHand.lerp = lerpDatas.GetInstance();
            targets.leftHand.lerp.Init(targets.leftHand, target, offset, rot);
        }
    }
    public void EndCurrentHandTarget(int netID)
    {
        if (NetID != netID) return;

        targets.rightHand.Reset();
        targets.leftHand.Reset();
    }

    //This is where predefined points will be sought out by whatever the predetermined Target is.
    public void ExecuteAnimation(int netID, string identifier, Action endEvent = null)
    {
        if (NetID != netID || lerpDatas.IsNull) return;

        AnimationConfig.ConfigData config = animations.GetAnimation(identifier);
        IKTarget target = TargetFrom(config.target);

        target.lerp = lerpDatas.GetInstance();
        target.lerp.Init(target, config.localPoints, true, config.persistent, endEvent);

        target.lerp.Animation = config.identifier;
    }
    public void EndAnimation(int netID, string identifier)
    {
        if (NetID != netID || lerpDatas.IsNull) return;

        AnimationConfig.ConfigData anim = animations.GetAnimation(identifier);
        IKTarget t = TargetFrom(anim.target);
        if (t.Null) return;
        if (t.lerp != null && anim.identifier == t.lerp.Animation)
            t.Reset();
    }
    
    //Gives the player a lean when moving
    public void RootMomentum()
    {
        Vector3 rootRot =  targets.root.localEulerAngles;
        float forward = Mathf.LerpAngle(rootRot.x, inputDir.y * data.moveData.maxRootForwardAngle, 3f * Time.deltaTime);
        float side = Mathf.LerpAngle(rootRot.y, inputDir.x * data.moveData.maxRootSideAngle, 3f * Time.deltaTime);

        Vector3 newRot = new Vector3(forward, targets.root.localEulerAngles.y, side);
        targets.root.localEulerAngles = newRot;
    }

    //Helper Functions
    public void SetMoveData(IPacketData packet)
    {
        PacketPlayerAnimatorData data = (PacketPlayerAnimatorData)packet;
        if (data.playerID != NetID) return;

        grounded = data.playerGrounded;
        inputDir = data.playerInputDir;
        moveState = data.playerMoveState;
        moving = (moveState != PlayerMoveState.None && moveState != PlayerMoveState.Crouching);
    }
    public void SetMoveData(bool grounded, int inputX, int inputY, int moveState)
    {
        this.grounded = grounded;
        inputDir.x = inputX;
        inputDir.y = inputY;
        this.moveState = (PlayerMoveState)moveState;
        moving = (this.moveState != PlayerMoveState.None && this.moveState != PlayerMoveState.Crouching);
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
    public IKTarget TargetFrom(AnimatorTarget target, Side side = Side.Right)
    {
        switch (target)
        {
            case AnimatorTarget.None:
                return IKTarget.Empty;
            case AnimatorTarget.head:
                return targets.head;
            case AnimatorTarget.chest:
                return targets.chest;
            case AnimatorTarget.pelvis:
                return targets.pelvis;
            case AnimatorTarget.Hands:
                if (side == Side.Right)
                    return targets.rightHand;
                else if (side == Side.Left)
                    return targets.leftHand;
                break;
            default:
                break;
        }

        return IKTarget.Empty;
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

    //IEventAccessor
    public void Subscribe()
    {
        Events.BipedAnimator.SetHandTargetPosition += SetHandTargetPosition;
        Events.BipedAnimator.EndCurrentHandTarget += EndCurrentHandTarget;
        Events.BipedAnimator.ExecuteAnimation += ExecuteAnimation;
        Events.BipedAnimator.EndAnimation += EndAnimation;

        Nadis.Net.Client.ClientPacketHandler.SubscribeTo((int)SharedPacket.PlayerAnimatorData, SetMoveData);

        Events.Player.UnSubscribe += UnSubscribe;
    }
    public void UnSubscribe(int netID)
    {
        if (NetID != netID) return;

        Events.BipedAnimator.SetHandTargetPosition -= SetHandTargetPosition;
        Events.BipedAnimator.EndCurrentHandTarget -= EndCurrentHandTarget;
        Events.BipedAnimator.ExecuteAnimation -= ExecuteAnimation;
        Events.BipedAnimator.EndAnimation -= EndAnimation;
        Events.Player.UnSubscribe -= UnSubscribe;
    }

    //Other
    private void LateUpdate()
    {
        if (NetID != NetData.LocalPlayerID) return;

        netSendTimer += Time.deltaTime;
        if(netSendTimer >= (1f / timesToCheclPerSecond))
        {
            if(inputDir != lastInputDir || grounded != lastGrounded)
            {
                PacketPlayerAnimatorData packet = new PacketPlayerAnimatorData
                {
                    playerID = NetID,
                    playerMoveState = moveState,
                    playerInputDir = inputDir,
                    playerGrounded = grounded
                };
                Events.Net.SendAsClient(NetID, packet);

                lastInputDir = inputDir;
                lastGrounded = grounded;
            }

            netSendTimer = 0f;
        }
    }
    private void OnDrawGizmos()
    {
        /*
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(targets.pelvis.position, 0.2f);
        */

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(targets.rightFoot.position, 0.05f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(targets.leftFoot.position, 0.05f);

        float leftY = (targets.leftFoot.position.y);
        float rightY = (targets.rightFoot.position.y);
        float avgY = (leftY + rightY) / 2f;
        Vector3 pos = transform.position;
        pos.y = avgY + 1f;

        //targets.pelvis.position = pos;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pos, 0.1f);
    }

}

