using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darwin_Server
{
    public static class MapData
    {
        public static int seed;
        public static float mapHeightMultiplier;

        private static Random prng;
        private static DateTime dateTime;
        private static bool generated = false;

        public static void Generate()
        {
            if (generated)
                return;

            dateTime = DateTime.Now;
            prng = new Random((int)dateTime.Ticks);

            seed = prng.Next(int.MinValue, int.MaxValue);
            mapHeightMultiplier = 15f;


        }

        public static void Reset()
        {
            generated = false;
        }


    }
}
