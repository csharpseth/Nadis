using UnityEngine;

namespace Nadis.Net.Server
{
    public class NetworkedUnit : MonoBehaviour
    {
        public float distanceToSendThreshold = 0.5f;
        private Vector3 lastPos;
        private ServerUnitData data;

        private void Awake()
        {
            data = GetComponent<ServerUnitData>();
        }

        private void Update()
        {
            Vector3 pos = data.Location;
            float speed = data.Speed;
            float sqrDist = (lastPos - pos).sqrMagnitude;
            if (sqrDist >= (distanceToSendThreshold * distanceToSendThreshold))
            {
                //Send Unit Position
                PacketUnitPosition packet = new PacketUnitPosition
                {
                    unitID = data.unitID,
                    location = pos,
                    speed = speed
                };
                ServerSend.UnReliableToAll(packet);
                lastPos = pos;
            }
        }
    }
}
