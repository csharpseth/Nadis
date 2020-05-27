using UnityEngine;
using Nadis.Net;
using Nadis.Net.Client;

public class ClientNetworkedUnit : MonoBehaviour, INetworkInitialized
{
    public int NetID { get; private set; }
    public int UnitID;
    public float speedBuffer = 1f;
    public float lagCompensationDirectionAdvancementAmount = 1f;

    Vector3 destination;
    Vector3 dir;
    float speed;

    public void InitFromNetwork(int netID)
    {
        NetID = netID;
        UnitID = netID;
        Subscribe();
    }

    private void SetPosition(IPacketData packet)
    {
        PacketUnitPosition data = (PacketUnitPosition)packet;
        if (NetID != data.unitID) return;

        speed = data.speed + speedBuffer;
        dir = (data.location - transform.position).normalized * lagCompensationDirectionAdvancementAmount;
        destination = data.location + dir;
    }

    private void Subscribe()
    {
        ClientPacketHandler.SubscribeTo((int)ServerPacket.UnitPosition, SetPosition);
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
