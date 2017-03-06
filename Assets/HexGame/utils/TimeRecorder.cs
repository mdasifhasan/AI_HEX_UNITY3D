using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeRecorder
{
    public static TimeRecorder Instance = new TimeRecorder();
    Dictionary<string, double> times = new Dictionary<string, double>();
    Dictionary<string, double> running = new Dictionary<string, double>();
    bool disabled = true;
    public double getTime()
    {
        var dateTimeNow = DateTime.Now;
        return ((dateTimeNow.Hour * 3600000) + (dateTimeNow.Minute * 60000) + (dateTimeNow.Second * 1000) + dateTimeNow.Millisecond) / (double)1000;
        //return DateTime.Now..Ticks / TimeSpan.TicksPerSecond / 1000;
    }

    public void resetTimer(string key)
    {

        if (disabled)
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
        if (disabled)
            return;
        if (!times.ContainsKey(key))
        {
            times.Add(key, 0);
            running.Add(key, 0);
        }
        running[key] = getTime();
    }
    public void stopTimer(string key)
    {

        if (disabled)
            return;
        times[key] += getTime() - running[key];
    }

    public void printStats()
    {
        if (disabled)
            return;
        foreach (string k in times.Keys)
        {
            Debug.Log(k + " - " + times[k]);
        }
    }
    public void printStat(string key)
    {
        if (disabled)
            return;
        Debug.Log(key + " - " + times[key]);
    }
}