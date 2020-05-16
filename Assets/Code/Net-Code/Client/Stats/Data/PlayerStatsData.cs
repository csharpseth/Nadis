namespace Nadis.Net.Client
{
    public struct PlayerStatsData
    {
        public int TargetID => target.NetID;

        public NetworkedPlayer target;
        public HealthData health;

        public PlayerStatsData(NetworkedPlayer target, HealthData initialHealth)
        {
            this.target = target;
            health = initialHealth;
        }

        public void Reset() {
            health.Reset();
        }
    }
}