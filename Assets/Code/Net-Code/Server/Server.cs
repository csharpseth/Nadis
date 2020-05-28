namespace Nadis.Net.Server
{
    public class Server
    {
        public static Server instance;
        public int MaxPlayers { get; private set; }
        public int Port { get; private set; }

        private ServerTCP TCP;
        public ServerUDP UDP;

        public Server(int maxPlayers = NetData.Default.MaxPlayers, int port = NetData.Default.Port)
        {
            if (instance == null) instance = this;
            MaxPlayers = maxPlayers;
            Port = port;

            TCP = new ServerTCP(Port);
            UDP = new ServerUDP(Port);
        }

        public void Start()
        {
            ClientManager.Init(MaxPlayers);
            ItemManager.Init();
            ServerPacketHandler.Initialize();
            ServerData.chargingStationLocations = ServerScenePrescence.GetAllChargingStationLocations();
            ServerData.playerSpawnLocations = ServerScenePrescence.GetAllPlayerSpawnPoints();
            ServerUnitController.Initialize(10);

            TCP.Start();
            UDP.Start();

            Log.Not("Server Started Successfully");
        }
        public void Stop()
        {
            ClientManager.Clear(true);
            TCP.Stop();
        }
    }
}
