using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class NetData
{
    public struct Default
    {
        public const int Port = 55225;
        public const int MaxPlayers = 15;
        public const int BufferSize = 2048;
        public const string IP = "127.0.0.1";
        public const int InventorySize = 7;
    }

    public static int LocalPlayerID = -1;
    public static float ClientTimeoutTimeInSeconds = 10f;
    
}
