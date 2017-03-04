using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeRecorder
{
    public static TimeRecorder Instance = new TimeRecorder();
    Dictionary<string, float> times = new Dictionary<string, float>();
    Dictionary<string, float> running = new Dictionary<string, float>();

    public void resetTimer(string key)
    {

        if (true)
            return;
        if (times.ContainsKey(key))
        {
            times[key] = 0;
            running[key] = 0;
        }
        else
        {
            times.Add(key, 0);
            running.Add(key, 0);
        }
    }

    public void startTimer(string key)
    {
        if (true)
            return;
        if (!times.ContainsKey(key))
        {
            times.Add(key, 0);
            running.Add(key, 0);
        }
        running[key] = Time.realtimeSinceStartup;
    }
    public void stopTimer(string key)
    {

        if (true)
            return;
        times[key] += Time.realtimeSinceStartup - running[key];
    }

    public void printStats()
    {
        foreach (string k in times.Keys)
        {
            Debug.Log(k + " - " + times[k]);
        }
    }
}