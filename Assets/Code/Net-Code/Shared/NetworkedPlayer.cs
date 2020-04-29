using UnityEngine;
using System.Collections;

public class NetworkedPlayer : MonoBehaviour, INetworkInitialized, IEventAccessor
{
    public int NetID { get; private set; }
    public bool Send = false;
    [Range(1, 30)]
    public int timeToCheckPerSecond = 1;
    public float maxMoveDistanceBeforeSending = 0.1f;
    public float maxRotBeforeSending = 2f;

    public float CheckInterval => (1f / timeToCheckPerSecond);
    public float MaxDistance => (maxMoveDistanceBeforeSending * maxMoveDistanceBeforeSending);

    private Vector3 lastPosition;
    private float lastRotation;

    public void InitFromNetwork(int playerID)
    {
        NetID = playerID;
        Send = (NetID == NetData.LocalPlayerID);
        Subscribe();
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
            if(distance >= MaxDistance)
            {
                PacketPlayerPosition packet = new PacketPlayerPosition
                {
                    playerID = NetID,
                    playerPosition = transform.position
                };
                Events.Net.SendAsClient(NetID, packet);

                lastPosition = transform.position;
            }

            if (angle >= maxRotBeforeSending)
            {
                PacketPlayerRotation packet = new PacketPlayerRotation
                {
                    playerID = NetID,
                    playerRotation = transform.eulerAngles.y
                };
                Events.Net.SendAsClient(NetID, packet);

                lastRotation = transform.eulerAngles.y;
            }

            timer = 0f;
        }
    }
    private void ReceivePlayerPosition(IPacketData packet)
    {
        PacketPlayerPosition data = (PacketPlayerPosition)packet;
        if (NetID != data.playerID) return;

        transform.position = data.playerPosition;
    }
    private void ReceivePlayerRotation(IPacketData packet)
    {
        PacketPlayerRotation data = (PacketPlayerRotation)packet;
        if (NetID != data.playerID) return;

        Vector3 rot = transform.eulerAngles;
        rot.y = data.playerRotation;
        transform.eulerAngles = rot;
    }


    public void Subscribe()
    {
        Nadis.Net.Client.ClientPacketHandler.SubscribeTo((int)SharedPacket.PlayerPosition, ReceivePlayerPosition);
        Nadis.Net.Client.ClientPacketHandler.SubscribeTo((int)SharedPacket.PlayerRotation, ReceivePlayerRotation);
        Events.Player.UnSubscribe += UnSubscribe;
    }

    public void UnSubscribe(int netID)
    {
        if (NetID != netID) return;
        Nadis.Net.Client.ClientPacketHandler.UnSubscribeFrom((int)SharedPacket.PlayerPosition, ReceivePlayerPosition);
        Events.Player.UnSubscribe -= UnSubscribe;
    }
}
