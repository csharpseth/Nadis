using UnityEngine;

namespace Nadis.Net.Server
{
    public struct ItemData
    {
        public int ID { get; set; }
        public int NetworkID { get; set; }

        public Vector3 Position { get; set; }
    }
}
