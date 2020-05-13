using UnityEngine;
using Nadis.Net;
using Nadis.Net.Client;
using Unity.Mathematics;

public class NetworkedPlayer : MonoBehaviour, INetworkInitialized, IEventAccessor
{
    public int NetID { get; private set; }
    public bool Send = false;
    public int timeToCheckTransformPerSecond = 40;
    public float maxMoveDistanceBeforeSending = 0.1f;
    public float maxRotBeforeSending = 2f;
    public int timesToCheckAnimatorPerSecond = 4;

    public float MoveCheckInterval => (1f / timeToCheckTransformPerSecond);
    public float AnimatorCheckInterval => (1f / timesToCheckAnimatorPerSecond);
    public float MaxDistance => (maxMoveDistanceBeforeSending * maxMoveDistanceBeforeSending);

    private Vector3 lastPosition;
    private float lastRotation;
    private float lastHeadRotation;
    private PlayerAnimatorController anim;
    
    public void InitFromNetwork(int playerID)
    {
        NetID = playerID;
        Send = (NetID == NetData.LocalPlayerID);
        anim = GetComponent<PlayerAnimatorController>();
        Subscribe();
    }
    
    private void LateUpdate()
    {
        if (Send == false) return;
        Sending();
    }

    float timer = 0f;
    float animTimer;

    private void Sending()
    {
        timer += Time.deltaTime;
        animTimer += Time.deltaTime;
        if(timer >= MoveCheckInterval)
        {
            float distance = (transform.position - lastPosition).sqrMagnitude;
            float angle = Mathf.Abs(transform.eulerAngles.y - lastRotation);
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
            timer = 0f;
        }

        if(animTimer >= AnimatorCheckInterval)
        {
            PacketPlayerAnimatorMoveData packet = new PacketPlayerAnimatorMoveData
            {
                playerID = NetID,
                forwardBlend = anim.forwardBlend,
                sideBlend = anim.sideBlend
            };
            Events.Net.SendAsClientUnreliable(NetID, packet);
            animTimer = 0f;
        }
    }
    private void ReceivePlayerPosition(IPacketData packet)
    {
        PacketPlayerPosition data = (PacketPlayerPosition)packet;
        if (NetID != data.playerID) return;

        Tween.FromToPosition(transform, data.playerPosition, MoveCheckInterval, Space.World, false);
    }
    private void ReceivePlayerRotation(IPacketData packet)
    {
        PacketPlayerRotation data = (PacketPlayerRotation)packet;
        if (NetID != data.playerID) return;

        Vector3 rot = transform.eulerAngles;
        rot.y = data.playerRotation;
        transform.eulerAngles = rot;
    }
    private void ReceivePlayerAnimatorMoveData(IPacketData packet)
    {
        PacketPlayerAnimatorMoveData data = (PacketPlayerAnimatorMoveData)packet;
        if (NetID != data.playerID) return;

        anim.forwardBlend = data.forwardBlend;
        anim.sideBlend = data.sideBlend;
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
        
        ClientPacketHandler.SubscribeTo((int)SharedPacket.PlayerDisconnected, PlayerDisconnected);
        ClientPacketHandler.SubscribeTo((int)SharedPacket.PlayerAnimatorMoveData, ReceivePlayerAnimatorMoveData);

        Events.Net.DisconnectClient += DisconnectLocalPlayer;
        Events.Player.GetPlayer += GetPlayer;

        Events.Player.UnSubscribe += UnSubscribe;
    }
    public void UnSubscribe(int netID)
    {
        if (NetID != netID) return;
        ClientPacketHandler.UnSubscribeFrom((int)SharedPacket.PlayerPosition, ReceivePlayerPosition);
        ClientPacketHandler.UnSubscribeFrom((int)SharedPacket.PlayerRotation, ReceivePlayerRotation);
        
        ClientPacketHandler.UnSubscribeFrom((int)SharedPacket.PlayerDisconnected, PlayerDisconnected);
        ClientPacketHandler.UnSubscribeFrom((int)SharedPacket.PlayerAnimatorMoveData, ReceivePlayerAnimatorMoveData);

        Events.Net.DisconnectClient -= DisconnectLocalPlayer;
        Events.Player.GetPlayer -= GetPlayer;

        Events.Player.UnSubscribe -= UnSubscribe;
    }
    
}
