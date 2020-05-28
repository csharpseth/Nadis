using Unity.Mathematics;
using UnityEngine;

namespace Nadis.Net.Server
{
    public class NetworkedUnit : MonoBehaviour
    {
        public float distanceToSendThreshold = 0.5f;
        public float rotationSendThreshold = 5f;

        private float3 lastPos;
        private float lastRot;

        private float speed;
        private float3 pos;
        private float sqrDist;

        private float rot;
        private float rotDiff;

        private ServerUnitData data;

        public new Transform transform;

        private PacketUnitPosition positionPacket;
        private PacketUnitRotation rotationPacket;

        private void Awake()
        {
            data = GetComponent<ServerUnitData>();
            positionPacket = new PacketUnitPosition();
            rotationPacket = new PacketUnitRotation();
            transform = GetComponent<Transform>();
        }

        private void Update()
        {
            pos = data.Location;
            speed = data.Speed;
            sqrDist = math.distancesq(pos, lastPos);

            rot = transform.eulerAngles.y;
            rotDiff = math.abs(rot - lastRot);

            if (sqrDist >= (distanceToSendThreshold * distanceToSendThreshold))
            {
                //Send Unit Position
                positionPacket.unitID = data.NetID;
                positionPacket.location = data.Location;
                positionPacket.speed = data.Speed;

                ServerSend.UnReliableToAll(positionPacket);
                lastPos = pos;
            }

            if(rotDiff >= rotationSendThreshold)
            {
                rotationPacket.unitID = data.NetID;
                rotationPacket.rot = rot;

                ServerSend.UnReliableToAll(rotationPacket);
                lastRot = rot;
            }

        }
    }
}
