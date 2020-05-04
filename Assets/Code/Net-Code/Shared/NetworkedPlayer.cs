using UnityEngine;
using Nadis.Net;
using Nadis.Net.Client;

public class NetworkedPlayer : MonoBehaviour, INetworkInitialized, IEventAccessor
{
    public int NetID { get; private set; }
    public BipedProceduralAnimator Animator { get { return animator; } }
    public bool Send = false;
    [Range(1, 30)]
    public int timeToCheckPerSecond = 1;
    public float maxMoveDistanceBeforeSending = 0.1f;
    public float maxRotBeforeSending = 2f;

    public float CheckInterval => (1f / timeToCheckPerSecond);
    public float MaxDistance => (maxMoveDistanceBeforeSending * maxMoveDistanceBeforeSending);

    private Vector3 lastPosition;
    private float lastRotation;
    private float lastHeadRotation;

    private Transform head;

    private BipedProceduralAnimator animator;

    public void InitFromNetwork(int playerID)
    {
        NetID = playerID;
        Send = (NetID == NetData.LocalPlayerID);
        animator = GetComponent<BipedProceduralAnimator>();

        head = animator.targets.head.defaultParent;

        Subscribe();
    }

    private void Update()
    {
        if (Send == false) return;

        //TEST
        if(Input.GetKeyDown(KeyCode.T))
        {
            SendPlayerAnimatorTargetSet(new Vector3(0.2f, -0.2f, 0.5f), new Vector3(0f, 0f, 180f), AnimatorTarget.Hands, 5f, Space.Local, true, AnimatorTarget.Head, Side.Right);
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            SendPlayerAnimatorTargetEnd(AnimatorTarget.Hands, Side.Right);
        }
    }
    private void LateUpdate()
    {
        if (Send == false) return;
        Sending();
    }

    float timer = 0f;
    private void Sending()
    {
        timer += Time.deltaTime;
        if(timer >= CheckInterval)
        {
            float distance = (transform.position - lastPosition).sqrMagnitude;
            float angle = Mathf.Abs(transform.eulerAngles.y - lastRotation);
            float headAngle = head.localEulerAngles.x;
            float headAngleDiff = Mathf.Abs(headAngle - lastHeadRotation);

            if(distance >= MaxDistance)
            {
                PacketPlayerPosition packet = new PacketPlayerPosition
                {
                    playerID = NetID,
                    playerPosition = transform.position
                };
                Events.Net.SendAsClientUnreliable(NetID, packet);

                lastPosition = transform.position;
            }

            if (angle >= maxRotBeforeSending)
            {
                PacketPlayerRotation packet = new PacketPlayerRotation
                {
                    playerID = NetID,
                    playerRotation = transform.eulerAngles.y
                };
                Events.Net.SendAsClientUnreliable(NetID, packet);

                lastRotation = transform.eulerAngles.y;
            }

            if(headAngleDiff >= maxRotBeforeSending)
            {
                PacketPlayerAnimatorHeadData packet = new PacketPlayerAnimatorHeadData
                {
                    playerID = NetID,
                    headAngle = headAngle
                };
                Events.Net.SendAsClientUnreliable(NetID, packet);
                lastHeadRotation = headAngle;
            }

            timer = 0f;
        }
    }
    private void ReceivePlayerPosition(IPacketData packet)
    {
        PacketPlayerPosition data = (PacketPlayerPosition)packet;
        if (NetID != data.playerID) return;

        Tween.FromToPosition(transform, data.playerPosition, CheckInterval, Space.World, false);
    }
    private void ReceivePlayerRotation(IPacketData packet)
    {
        PacketPlayerRotation data = (PacketPlayerRotation)packet;
        if (NetID != data.playerID) return;

        Vector3 rot = transform.eulerAngles;
        rot.y = data.playerRotation;
        transform.eulerAngles = rot;
    }

    private void ReceivePlayerAnimatorTargetSet(IPacketData packet)
    {
        PacketPlayerAnimatorTargetSet data = (PacketPlayerAnimatorTargetSet)packet;
        if (NetID != data.playerID) return;
        animator.SetTarget(data.targetsNewPosition, data.targetsNewRotation, data.target, data.speed, data.space, data.persistent, data.targetParent, data.side, null);
    }
    private void ReceivePlayerAnimatorEndTarget(IPacketData packet)
    {
        PacketPlayerAnimatorTargetEnd data = (PacketPlayerAnimatorTargetEnd)packet;
        if (NetID != data.playerID) return;

        animator.EndTarget(data.target, data.side);
    }

    private void ReceivePlayerAnimatorHeadData(IPacketData packet)
    {
        PacketPlayerAnimatorHeadData data = (PacketPlayerAnimatorHeadData)packet;
        if (data.playerID != NetID) return;

        Vector3 rot = head.transform.localEulerAngles;
        rot.x = data.headAngle;
        head.transform.localEulerAngles = rot;
    }

    private void PlayerDisconnected(IPacketData packet)
    {
        PacketDisconnectPlayer data = (PacketDisconnectPlayer)packet;
        if (data.playerID != NetID) return;

        Events.Player.UnSubscribe(NetID);
        Destroy(gameObject);
    }
    private void DisconnectLocalPlayer()
    {
        Events.Player.UnSubscribe?.Invoke(NetID);
        Destroy(gameObject);
    }

    private void GetPlayer(int playerID, ref NetworkedPlayer netPlayer)
    {
        if (playerID != NetID) return;
        netPlayer = this;
    }

    public void SendPlayerAnimatorTargetSet(Vector3 pos, Vector3 rot, AnimatorTarget target, float speed, Space space, bool persistent, AnimatorTarget parent, Side side)
    {
        PacketPlayerAnimatorTargetSet packet = new PacketPlayerAnimatorTargetSet
        {
            playerID = NetID,
            targetsNewPosition = pos,
            targetsNewRotation = rot,
            target = target,
            speed = speed,
            space = space,
            persistent = persistent,
            targetParent = parent,
            side = side
        };

        Events.Net.SendAsClientUnreliable(NetID, packet);

        //animator.SetTarget(pos, rot, target, speed, space, persistent, parent, side, null);
    }
    public void SendPlayerAnimatorTargetEnd(AnimatorTarget target, Side side)
    {
        PacketPlayerAnimatorTargetEnd packet = new PacketPlayerAnimatorTargetEnd
        {
            playerID = NetID,
            target = target,
            side = side
        };
        Events.Net.SendAsClientUnreliable(NetID, packet);

        //animator.EndTarget(target, side);
    }

    public void RequestDamageThisPlayer(int amount)
    {
        PacketRequestDamagePlayer packet = new PacketRequestDamagePlayer
        {
            playerID = NetID,
            alterAmount = amount
        };
        Events.Net.SendAsClient(NetData.LocalPlayerID, packet);
    }

    public void Subscribe()
    {
        ClientPacketHandler.SubscribeTo((int)SharedPacket.PlayerPosition, ReceivePlayerPosition);
        ClientPacketHandler.SubscribeTo((int)SharedPacket.PlayerRotation, ReceivePlayerRotation);

        ClientPacketHandler.SubscribeTo((int)SharedPacket.PlayerAnimatorTargetSet, ReceivePlayerAnimatorTargetSet);
        ClientPacketHandler.SubscribeTo((int)SharedPacket.PlayerAnimatorTargetEnd, ReceivePlayerAnimatorEndTarget);

        ClientPacketHandler.SubscribeTo((int)SharedPacket.PlayerAnimatorHeadData, ReceivePlayerAnimatorHeadData);
        ClientPacketHandler.SubscribeTo((int)SharedPacket.PlayerDisconnected, PlayerDisconnected);

        Events.Net.DisconnectClient += DisconnectLocalPlayer;
        Events.Player.GetPlayer += GetPlayer;

        Events.Player.UnSubscribe += UnSubscribe;
    }
    public void UnSubscribe(int netID)
    {
        if (NetID != netID) return;
        ClientPacketHandler.UnSubscribeFrom((int)SharedPacket.PlayerPosition, ReceivePlayerPosition);
        ClientPacketHandler.UnSubscribeFrom((int)SharedPacket.PlayerRotation, ReceivePlayerRotation);

        ClientPacketHandler.UnSubscribeFrom((int)SharedPacket.PlayerAnimatorTargetSet, ReceivePlayerAnimatorTargetSet);
        ClientPacketHandler.UnSubscribeFrom((int)SharedPacket.PlayerAnimatorTargetEnd, ReceivePlayerAnimatorEndTarget);

        ClientPacketHandler.UnSubscribeFrom((int)SharedPacket.PlayerAnimatorHeadData, ReceivePlayerAnimatorHeadData);
        ClientPacketHandler.UnSubscribeFrom((int)SharedPacket.PlayerDisconnected, PlayerDisconnected);

        Events.Net.DisconnectClient -= DisconnectLocalPlayer;
        Events.Player.GetPlayer -= GetPlayer;

        Events.Player.UnSubscribe -= UnSubscribe;
    }
    
}
