using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat  {
    public string player_name_1 = "";
    public string player_name_2 = "";

    public Stat()
    {

    }
    public Stat(string p1, string p2)
    {
        this.player_name_1 = p1;
        this.player_name_2 = p2;
    }

    public int totalPlay = 0;
    public int[] tossWon = new int[2];
    public int[] Won = new int[2];
    public int[] toss_won_then_won = new int[2];
    public int[] toss_lost_then_won = new int[2];


    public void RecordGame(int tossWon, int winner)
    {
        this.totalPlay++;
        this.tossWon[tossWon]++;
        this.Won[winner]++;
        if(tossWon == 0)
        {
            if (winner == 0)
                toss_won_then_won[0]++;
            else
                toss_lost_then_won[1]++;
        }
        else
        {
            if (winner == 1)
                toss_won_then_won[1]++;
            else
                toss_lost_then_won[0]++;
        }
    }

    public override string ToString()
    {
        //Debug.LogError(string.Format("totalPlay: {0}, tossWon[0]: {1}, tossWon[1]: {2}, Won[0]: {3}, Won[1]: {4}, toss_won_then_won[0]: {5}, toss_lost_then_won[0]: {6}, toss_won_then_won[1]: {7}, toss_lost_then_won[1]: {8}", totalPlay, tossWon[0], tossWon[1], Won[0], Won[1], toss_won_then_won[0], toss_lost_then_won[0], toss_won_then_won[1], toss_lost_then_won[1]));
        //return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", totalPlay, tossWon[0], tossWon[1], Won[0], Won[1], toss_won_then_won[0], toss_lost_then_won[0], toss_won_then_won[1], toss_lost_then_won[1]);
        return string.Format("totalPlay: {0}, tossWon[0]: {1}, tossWon[1]: {2}, Won[0]: {3}, Won[1]: {4}, toss_won_then_won[0]: {5}, toss_lost_then_won[0]: {6}, toss_won_then_won[1]: {7}, toss_lost_then_won[1]: {8}", totalPlay, tossWon[0], tossWon[1], Won[0], Won[1], toss_won_then_won[0], toss_lost_then_won[0], toss_won_then_won[1], toss_lost_then_won[1]);
    }
    public string Labels()
    {
        return string.Format("totalPlay, tossWon[0], tossWon[1], Won[0], Won[1], toss_won_then_won[0], toss_lost_then_won[0], toss_won_then_won[1], toss_lost_then_won[1]");
    }
}

public class Stats
{
    public static Dictionary<string, Stat> stats = new Dictionary<string, Stat>();

    public static void RecordGame(string player1, string player2, int tossWon, int won)
    {
        string game = player1 + player2;
        Stat stat = null;
        if (!stats.ContainsKey(game))
        {
            stat = new Stat(player1, player2);
            stats.Add(game, stat);
        }
        else
            stat = stats[game];
        stat.RecordGame(tossWon, won);
    }


    public static void Print()
    {
        PrintLabels();
        foreach(var k in stats.Keys)
        {
            Debug.LogError(stats[k].player_name_1 + " - " + stats[k].player_name_2 + " : " + stats[k]);
        }
    }

    internal static string getString()
    {
        string output = "";
        foreach (var k in stats.Keys)
        {
            output += stats[k].player_name_1 + " - " + stats[k].player_name_2 + "\n";
            output += stats[k] + "\n\n";
        }
        return output;
    }

    public static void PrintLabels()
    {
        Debug.LogError(string.Format("totalPlay, tossWon[0], tossWon[1], Won[0], Won[1], toss_won_then_won[0], toss_lost_then_won[0], toss_won_then_won[1], toss_lost_then_won[1]"));
    }
}
