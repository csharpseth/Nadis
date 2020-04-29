using Nadis.Net.Client;
using Nadis.Net.Server;

using UnityEngine;

public class NetTester : MonoBehaviour
{
    private Server server;
    private Client client;

    private void Awake()
    {
        server = null;
        client = null;
    }
    
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.H) && server == null)
        {
            //Start Server
            server = new Server();
            server.Start();
        }

        if(Input.GetKeyDown(KeyCode.C) && client == null)
        {
            //Start Connection
            client = new Client();
            client.ConnectToServer();
        }
    }
}
