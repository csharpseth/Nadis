using Nadis.Net.Client;
using Nadis.Net.Server;

using UnityEngine;

public class NetTester : MonoBehaviour
{
    [Range(1, 50)]
    public int numClientsToSimulate = 1;
    private Server server;
    private int numClients = 0;

    private void Awake()
    {
        server = new Server();
        server.Start();

        
    }

    float time = 0f;
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            int fromID = NetData.LocalPlayerID;
            int toID = fromID + 1;
            int numClient = ClientManager.Clients.Count;
            if (toID >= numClient) toID = (numClient - 1);

            Events.admin.OnSwitchActivePlayer?.Invoke(fromID, toID);
        }


        if (numClients >= numClientsToSimulate) return;
        
        time += Time.deltaTime;
        if (numClients < 1 || Input.GetKeyDown(KeyCode.P))
        {
            Client c = new Client();
            c.ConnectToServer();
            numClients++;

            time = 0f;
        }
    }
}
