using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nadis
{
    public struct TimerData
    {
        public float TimeStarted { get; private set; }
        public float ElapsedTime
        {
            get
            {
                float diff = Time.realtimeSinceStartup - TimeStarted;
                return diff;
            }
        }

        public void Start()
        {
            TimeStarted = Time.realtimeSinceStartup;
        }
    }

    public static class Timer
    {
        private static Dictionary<string, TimerData> timers = new Dictionary<string, TimerData>();

        /// <summary>
        /// Keyword is a string identifier used as a key in the Dictionary of timers. Keyword is always forced to lowercase.
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="overrideIfKeywordExists"></param>
        public static void BeginTime(string keyword, bool overrideIfKeywordExists = false)
        {
            string key = keyword.ToLower();
            if(timers.ContainsKey(key) == true)
            {
                if(overrideIfKeywordExists)
                    timers.Remove(key);
                else
                    return;
            }

            TimerData timer = new TimerData();
            timer.Start();
            timers.Add(key, timer);
        }

        public static float StopTime(string keyword)
        {
            string key = keyword.ToLower();
            if (timers.ContainsKey(key) == false) return -1f;

            float time = timers[key].ElapsedTime;
            timers.Remove(key);

            return time;
        }

        public static float EvaluateTime(string keyword)
        {
            string key = keyword.ToLower();
            if (timers.ContainsKey(key) == false) return -1f;

            return timers[key].ElapsedTime;
        }

        public static void ResetTime(string keyword)
        {
            string key = keyword.ToLower();
            if (timers.ContainsKey(key) == false) return;

            TimerData data = timers[key];
            data.Start();
            timers[key] = data;
        }

    }
}
