using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darwin_Server
{
    public static class NetObjectManager
    {

        private static List<NetObjectData> netObjects = new List<NetObjectData>();

        private static int nextID = 0;
        private static int NextID()
        {
            nextID = nextID + 1;
            return nextID;
        }

        public static int RegisterObject(int registryID)
        {
            NetObjectData netObject = new NetObjectData(registryID, NextID());

            netObjects.Add(netObject);

            return netObject.ID;
        }

        public static void RemoveRegisteredObject(int spawnID)
        {
            for (int i = 0; i < netObjects.Count; i++)
            {
                if(netObjects[i].ID == spawnID)
                {
                    netObjects.Remove(netObjects[i]);
                }
            }
        }

        public static void SetNetObjectPosition(int spawnID, Vector3 newPosition)
        {
            for (int i = 0; i < netObjects.Count; i++)
            {
                if (netObjects[i].ID == spawnID)
                {
                    netObjects[i].SetPosition(newPosition);
                }
            }
        }

        public static void SetNetObjectRotation(int spawnID, Vector3 newRotation)
        {
            for (int i = 0; i < netObjects.Count; i++)
            {
                if (netObjects[i].ID == spawnID)
                {
                    netObjects[i].SetRotation(newRotation);
                }
            }
        }

    }

    public class NetObjectData
    {
        private int registryID;
        private int spawnID;

        private Vector3 position;
        private Vector3 rotation;

        public int ID { get { return spawnID; } }

        public NetObjectData(int registryID, int spawnID)
        {
            this.registryID = registryID;
            this.spawnID = spawnID;
        }

        public void SetPosition(Vector3 newPos)
        {
            position = newPos;
        }

        public void SetRotation(Vector3 newRot)
        {
            rotation = newRot;
        }

    }
}
