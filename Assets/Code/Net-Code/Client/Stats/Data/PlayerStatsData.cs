namespace Nadis.Net.Client
{
    public struct PlayerStatsData
    {
        public int TargetID => target.NetID;

        public NetworkedPlayer target;
        public int health;
        public int power;
        public int maxHealth;
        public int maxPower;

        public bool isShutdown;

        private int startHealth;
        private int startPower;


        public PlayerStatsData(NetworkedPlayer target, int startHealth, int startPower, int maxHealth, int maxPower)
        {
            this.target = target;
            health = startHealth;
            power = startPower;
            this.maxHealth = maxHealth;
            this.maxPower = maxPower;

            this.startHealth = startHealth;
            this.startPower = startPower;
            isShutdown = false;
        }

        public void Reset() {
            health = startHealth;
            power = startPower;
        }

        public float HealthPercent => ((float)health / maxHealth);
        public float PowerPercent => ((float)power / maxPower);
    }
}