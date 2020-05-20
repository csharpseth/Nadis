namespace Nadis.Net.Server
{
    public static class ServerData
    {
        public static int PlayerStartHealth = 100;
        public static int PlayerMaxHealth = 100;
        public static int PlayerMinHealthBeforeDeath = 2;

        public static int PlayerStartPower = 500;
        public static int PlayerMaxPower = 500;

        public static float PlayerUnitsToMoveBeforeChargingPower = 3f;
        public static int PlayerPowerToLose = 2;
        public static float PlayerChargeDelay = 0.5f;
        public static int PlayerChargeAmountPerDelay = 10;
        public static int PlayerOverChargeDamageAmount = 5;

        public static UnityEngine.Vector3[] chargingStationLocations;
        public static float ChargeDistance = 5f;

        public static float DamageMultiplierFrom(PlayerAppendage appendage)
        {
            switch(appendage)
            {
                case(PlayerAppendage.Arms):
                    return 1f;
                case(PlayerAppendage.Chest):
                    return 1.5f;
                case(PlayerAppendage.Foot):
                    return 0.5f;
                case(PlayerAppendage.Head):
                    return 2f;
                case(PlayerAppendage.Leg):
                    return 0.75f;
                case(PlayerAppendage.Pelvis):
                    return 1f;
                case(PlayerAppendage.Hands):
                    return 0.5f;
                default:
                    break;
            }

            return 0f;
        }
    }
}