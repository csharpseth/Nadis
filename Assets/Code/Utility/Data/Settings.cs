public static class Settings
{
    public struct Player
    {
        public const float DefaultMaxHorizontalAngle = 40f;
        public const float DefaultMinHorizontalAngle = 50f;
        
        private float maxHorizontalAngle;
        private float minHorizontalAngle;


        public float MaxHorizontalAngle
        {
            get
            {
                float val = maxHorizontalAngle;
                if(val < 5f)
                {
                    val = DefaultMaxHorizontalAngle;
                }
                return val;
            }
            set
            {
                maxHorizontalAngle = value;
            }
        }
        public float MinHorizontalAngle
        {
            get
            {
                float val = minHorizontalAngle;
                if (val < 5f)
                {
                    val = DefaultMinHorizontalAngle;
                }
                return val;
            }
            set
            {
                minHorizontalAngle = value;
            }
        }

    }

    public static Player player;
}
