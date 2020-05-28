using UnityEngine;
using Nadis.Net;
using Nadis.Net.Client;

public class ClientNetworkedUnit : MonoBehaviour, INetworkInitialized
{
    public int NetID { get; private set; }
    public int UnitID;
    public float speedBuffer = 1f;
    public float lagCompensationDirectionAdvancementAmount = 1f;

    private Animator anim;

    Vector3 destination;
    Vector3 dir;
    float speed;

    public void InitFromNetwork(int netID)
    {
        NetID = netID;
        UnitID = netID;
        Subscribe();
        anim = GetComponentInChildren<Animator>();
    }

    private void SetPosition(IPacketData packet)
    {
        PacketUnitPosition data = (PacketUnitPosition)packet;
        if (NetID != data.unitID) return;

        speed = data.speed + speedBuffer;
        dir = (data.location - transform.position).normalized * lagCompensationDirectionAdvancementAmount;
        destination = data.location + dir;
    }

    Vector3 rotBuffer = Vector3.zero;
    private void SetRotation(IPacketData packet)
    {
        PacketUnitRotation data = (PacketUnitRotation)packet;
        if (data.unitID != NetID) return;
        rotBuffer = transform.eulerAngles;
        rotBuffer.y = data.rot;
        transform.eulerAngles = rotBuffer;
    }

    private void SetAnimationState(IPacketData packet)
    {
        PacketUnitAnimationState data = (PacketUnitAnimationState)packet;
        if (data.unitID != NetID || anim == null) return;

        anim.SetFloat("forward_blend", data.moveDir.y);
        anim.SetFloat("side_blend", data.moveDir.x);
    }

    private void Subscribe()
    {
        ClientPacketHandler.SubscribeTo((int)ServerPacket.UnitPosition, SetPosition);
        ClientPacketHandler.SubscribeTo((int)ServerPacket.UnitRotation, SetRotation);
        ClientPacketHandler.SubscribeTo((int)ServerPacket.UnitAnimationState, SetAnimationState);
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, destination, speed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(destination, 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + (Vector3.up * 1.5f), dir);
    }
}
