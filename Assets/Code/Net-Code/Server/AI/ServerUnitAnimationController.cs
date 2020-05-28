using UnityEngine;
using Nadis.Net;
using Nadis.Net.Server;

public class ServerUnitAnimationController : MonoBehaviour, INetworkInitialized
{
    public int NetID { get; private set; }

    public float sendDelay = 0.5f;

    private Vector3 lastPos;
    private Vector3 dir;
    private Vector2Int inputDir;

    private PacketUnitAnimationState animStatePacket;

    float sendTimer = 0f;

    private void Update()
    {
        return;
        sendTimer += Time.deltaTime;
        if (sendTimer < sendDelay) return;

        sendTimer = 0f;

        inputDir.y = (int)dir.z;
        inputDir.x = (int)dir.x;

        animStatePacket.moveDir = inputDir;
        
        ServerSend.UnReliableToAll(animStatePacket);
    }


    private float dirCheckDelay = 0.1f;
    float dirCheckTime = 0f;

    private void LateUpdate()
    {
        return;

        dirCheckTime += Time.deltaTime;
        if (dirCheckTime < dirCheckDelay) return;
        dirCheckTime = 0f;

        dir = transform.InverseTransformDirection(Util.FastNormalize(transform.position - lastPos));
        lastPos = transform.position;
    }

    public void InitFromNetwork(int netID)
    {
        NetID = netID;
        animStatePacket = new PacketUnitAnimationState
        {
            unitID = NetID
        };
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position + Vector3.up, dir);
    }
}
