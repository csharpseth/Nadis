using Proyecto26;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public static class DarwinAPIManager
{
    private static readonly string API_URL = "http://45.79.9.19:3000/";
    private static readonly string SERVERS_GET = "servers/";
    
    public static void GetServers(Action<ServerData[]> callback)
    {
        RestClient.GetArray<ServerData>(API_URL + SERVERS_GET).Then(response =>
        {
            callback?.Invoke(response);
        });
    }

}
