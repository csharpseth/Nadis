using UnityEngine;
using System.Collections;

public class NetworkedPlayer : MonoBehaviour, INetworkInitialized, IEventAccessor
{
    public int NetID { get; private set; }
    public bool Send = false;

    public int checkInterval = 1;
    public float distanceToSend = 0.5f;
    public float rotationDifftoSend = 2f;
    private Vector3 prevPosition;
    private float prevRotation;

    private float CheckInterval { get { return (1f / checkInterval); } }
    private float SendDistance { get { return (distanceToSend * distanceToSend); } }
    private float LookRotation { get { return transform.eulerAngles.y; } }

    public void InitFromNetwork(int playerID)
    {
        NetID = playerID;
        Send = (NetID == NetData.LocalPlayerID);
        Subscribe();
    }

    private void SwitchActivePlayer(int fromID, int toID)
    {
        if (NetID != fromID || NetID != toID) return;

        if(NetID == fromID)
        {
            IDisableIfRemotePlayer[] disable = GetComponentsInChildren<IDisableIfRemotePlayer>();
            for (int i = 0; i < disable.Length; i++)
            {
                disable[i].Disable(true);
            }
        }else if(NetID == toID)
        {
            IDisableIfRemotePlayer[] disable = GetComponentsInChildren<IDisableIfRemotePlayer>();
            for (int i = 0; i < disable.Length; i++)
            {
                disable[i].Disable(false);
            }
            NetData.LocalPlayerID = NetID;
        }

    }
    private void OnPlayerTransformReceived(IPacketData data)
    {
        PacketPlayerTransform packet = (PacketPlayerTransform)data;
        if (packet.playerID != NetID) return;

        transform.position = packet.position;
        Vector3 rot = transform.eulerAngles;
        rot.y = packet.rotation;
        transform.eulerAngles = rot;
    }

    float time = 0f;
    private void Update()
    {
        if (Send == false) return;

        time += Time.deltaTime;
        float rot = LookRotation;
        float dist = (transform.position - prevPosition).sqrMagnitude;

        if (time >= checkInterval || dist >= SendDistance || Mathf.Abs(rot - prevRotation) >= rotationDifftoSend)
        {
            PacketPlayerTransform packet = new PacketPlayerTransform
            {
                position = transform.position,
                rotation = rot,
                playerID = NetID
            };
            Events.Net.SendAsClient(NetID, packet);

            prevPosition = transform.position;
            prevRotation = rot;
        }
    }

    public void Subscribe()
    {
        Events.admin.OnSwitchActivePlayer += SwitchActivePlayer;

        Nadis.Net.Client.ClientPacketHandler.SubscribeTo((int)SharedPacket.PlayerTransform, OnPlayerTransformReceived);

        Events.Player.UnSubscribe += UnSubscribe;
    }

    public void UnSubscribe(int netID)
    {
        if (NetID != netID) return;

        Events.admin.OnSwitchActivePlayer -= SwitchActivePlayer;

        Nadis.Net.Client.ClientPacketHandler.UnSubscribeFrom((int)SharedPacket.PlayerTransform, OnPlayerTransformReceived);

        Events.Player.UnSubscribe -= UnSubscribe;
    }
}
