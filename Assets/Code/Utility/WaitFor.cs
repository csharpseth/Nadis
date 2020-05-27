using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class WaitFor : MonoBehaviour
{
    private static Queue<WaitForData> waitQueue = new Queue<WaitForData>();

    private void Update()
    {
        if (waitQueue == null || waitQueue.Count == 0) return;

        for (int i = 0; i < waitQueue.Count; i++)
        {
            WaitForData data = waitQueue.Dequeue();
            data.time += Time.deltaTime;
            if(data.time >= data.waitForTime)
            {
                data.callback?.Invoke(data.clientID);
            }
        }
    }

    public static void Seconds(float secondsToWait, Action<int> callback, int clientID)
    {
        WaitForData data = new WaitForData
        {
            callback = callback,
            time = 0f,
            waitForTime = secondsToWait,
            clientID = clientID
        };

        waitQueue.Enqueue(data);
    }



}

public struct WaitForData
{
    public Action<int> callback;
    public float time;
    public float waitForTime;
    public int clientID;
}
