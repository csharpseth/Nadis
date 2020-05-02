using Nadis.Net.Client;
using Nadis.Net.Server;

using UnityEngine;

public class NetTester : MonoBehaviour
{
    private Server server;
    private Client client;
    public bool LogText = false;
    public bool LogWarn = false;
    public bool LogError = true;

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

        Log.LogText = LogText;
        Log.LogWarnings = LogWarn;
        Log.LogErrors = LogError;
    }

    private void OnApplicationQuit()
    {
        if (client != null)
            Events.Net.DisconnectClient();
        if (server != null)
            server.Stop();
    }
}
