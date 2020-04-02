using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darwin_Server
{
    public struct ItemMetaData
    {
        public int ItemID { get; private set; }
        public int InstanceID { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 Rotation { get; private set; }

        public ItemMetaData(int itemID, int instanceID, Vector3 pos, Vector3 rot)
        {
            ItemID = itemID;
            InstanceID = instanceID;
            Position = pos;
            Rotation = rot;
        }

        public void UpdatePosition(Vector3 pos)
        {
            Position = pos;
        }

        public void UpdateRotation(Vector3 rot)
        {
            Rotation = rot;
        }
    }
}
