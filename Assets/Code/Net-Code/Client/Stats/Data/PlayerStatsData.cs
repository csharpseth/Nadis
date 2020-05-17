namespace Nadis.Net.Client
{
    public struct PlayerStatsData
    {
        public int TargetID => target.NetID;

        public NetworkedPlayer target;
        public StatData health;
        public StatData power;

        public PlayerStatsData(NetworkedPlayer target, StatData initialHealth, StatData initialPower)
        {
            this.target = target;
            health = initialHealth;
            power = initialPower;
        }

        public void Reset() {
            health.Reset();
            power.Reset();
        }
    }
}