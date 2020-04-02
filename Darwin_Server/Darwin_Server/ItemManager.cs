using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darwin_Server
{
    public static class ItemManager
    {
        private static Dictionary<int, ItemMetaData> registeredItems;
        private static int totalItemsRegistered = 0;
        
        public static void Initialize()
        {
            registeredItems = new Dictionary<int, ItemMetaData>();
            totalItemsRegistered = 0;
        }

        public static int RegisterNewItem(int itemID, Vector3 pos, Vector3 rot)
        {
            totalItemsRegistered++;
            ItemMetaData item = new ItemMetaData(itemID, totalItemsRegistered, pos, rot);
            registeredItems.Add(item.InstanceID, item);

            return item.InstanceID;
        }

        public static bool RemoveItem(int instanceID)
        {
            if (registeredItems.ContainsKey(instanceID) == false)
                return false;

            registeredItems.Remove(instanceID);
            return true;
        }

        public static void UpdateItemPosition(int instanceID, Vector3 pos)
        {
            if (registeredItems.ContainsKey(instanceID) == false)
                return;

            registeredItems[instanceID].UpdatePosition(pos);

        }

        public static void UpdateItemRotation(int instanceID, Vector3 rot)
        {
            if (registeredItems.ContainsKey(instanceID) == false)
                return;

            registeredItems[instanceID].UpdateRotation(rot);
        }
    }
}