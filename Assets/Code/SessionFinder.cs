using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionFinder : MonoBehaviour
{
    public static SessionFinder ins;

    private Action<ServerData> waitSuccessCallback;
    private Action<object> waitFailCallback;
    private float maxWait;

    private void Awake()
    {
        if(ins == null)
        {
            ins = this;
            DontDestroyOnLoad(this);
        }
    }

    private void ServersRetrieved(ServerData[] servers)
    {
        StartCoroutine("FindSessionCouroutine", servers);
    }

    public void FindSession(Action<ServerData> successCallback, float waitFor = 2f, Action<object> failCallback = null)
    {
        DarwinAPIManager.GetServers(ServersRetrieved);
        waitSuccessCallback += successCallback;
        waitFailCallback += failCallback;
        maxWait = waitFor;
    }

    IEnumerator FindSessionCouroutine(ServerData[] servers)
    {
        float lowPing = 1000;
        ServerData server = null;
        for (int i = 0; i < servers.Length; i++)
        {
            Ping ping = new Ping(servers[i].remoteIP);
            yield return new WaitUntil(() => ping.isDone);
            if (ping.time < lowPing)
            {
                lowPing = ping.time;
                server = servers[i];
            }

            /*if(ping.isDone == true)
            {
                if (ping.time < lowPing)
                {
                    lowPing = ping.time;
                    server = servers[i];
                }
            }else
            {
                servers[i].remoteIP = "127.0.0.1";
                Ping p = new Ping(servers[i].remoteIP);
                yield return new WaitForSeconds(maxWait);
                if (ping.isDone)
                {
                    server = servers[i];
                    break;
                }
            }*/
        }

        if (server != null)
        {
            Debug.Log("Server Found!");
            waitSuccessCallback?.Invoke(server);
        }
        else
            waitFailCallback?.Invoke("Failed To Find A Server");

        waitSuccessCallback = null;
        waitFailCallback = null;
        maxWait = 2;
    }

}
