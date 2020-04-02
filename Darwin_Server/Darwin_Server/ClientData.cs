using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darwin_Server
{
    public class ClientData
    {
        private Vector3 position;
        private Vector3 rotation;
        private int connectionID;

        private bool grounded;
        private float inputX;
        private float inputY;
        private int moveState;
        private int[] inventory;

        public int ConnectionID { get { return connectionID; } }
        public Vector3 Position { get { return position; } set { position = value; } }
        public Vector3 Rotation { get { return rotation; } set { rotation = value; } }

        public ClientData(int connectionID, Vector3 pos = default(Vector3), Vector3 rot = default(Vector3))
        {
            this.connectionID = connectionID;
            position = pos;
            rotation = rot;
        }

        public void SetPosition(float x, float y, float z)
        {
            position = new Vector3(x, y, z);
        }

        public void SetRotation(float x, float y, float z)
        {
            rotation = new Vector3(x, y, z);
        }

        public void SetMoveData(bool grounded, float inputX, float inputY, int moveState)
        {
            this.grounded = grounded;
            this.inputX = inputX;
            this.inputY = inputY;
            this.moveState = moveState;
        }

        public void SetInventory(int[] ids)
        {
            inventory = new int[ids.Length];

            for (int i = 0; i < ids.Length; i++)
            {
                inventory[i] = ids[i];
            }
        }
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 Zero()
        {
            return new Vector3(0f, 0f, 0f);
        }

    }

}
