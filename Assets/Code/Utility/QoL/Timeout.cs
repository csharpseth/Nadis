using UnityEngine;
using System.Collections.Generic;
using System;
using Nadis;

public class Timeout : MonoBehaviour
{
    #region Singleton
    private static Timeout instance;
    private void Awake()
    {
        if (instance != null)
            Destroy(this);

        instance = this;
        DontDestroyOnLoad(this);
        timeouts = new List<TimeoutData>();
    }
    #endregion

    private static List<TimeoutData> timeouts;

    private void Update()
    {
        if (timeouts == null || timeouts.Count == 0) return;

        for (int i = 0; i < timeouts.Count; i++)
        {
            float time = Timer.EvaluateTime(timeouts[i].key);
            if(time == -1f)
            {
                timeouts.RemoveAt(i);
                break;
            }

            if(time >= timeouts[i].timeBeforeExecutingCallback)
            {
                timeouts[i].callback?.Invoke();
                if(timeouts[i].repeat == false)
                {
                    timeouts.RemoveAt(i);
                    break;
                }else
                {
                    Timer.ResetTime(timeouts[i].key);
                }
            }
        }
    }


    public static void Create(string keyword, float waitTime, Action callback, bool repeat)
    {
        TimeoutData data = new TimeoutData
        {
            key = keyword,
            timeBeforeExecutingCallback = waitTime,
            callback = callback,
            repeat = repeat
        };
        Timer.BeginTime(keyword, true);
        timeouts.Add(data);
    }
}

public struct TimeoutData
{
    public string key;
    public float timeBeforeExecutingCallback;
    public Action callback;
    public bool repeat;
}
