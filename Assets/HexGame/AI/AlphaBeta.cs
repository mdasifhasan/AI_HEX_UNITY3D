﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RetIterate
{
    public int score;
    public Node node;

    public RetIterate(int score, Node node)
    {
        this.score = score;
        this.node = node;
    }
}

public class AlphaBeta
{
    bool MaxPlayer = true;
    int playerID = -1;
    public AlphaBeta(int playerID)
    {
        this.playerID = playerID;
    }

    public TileState NextMove(Dictionary<string, Tile> grid, List<TileState> playerMaxTiles, List<TileState> playerMinTiles, List<TileState> availableTiles)
    {
        i = 0;
        RetIterate selected = Iterate(new Node(grid, playerMaxTiles, playerMinTiles, availableTiles, null), depthBudget, -9999, 9999, true);
        return selected.node.tile;
    }

    static int i = 0;
    static int budget = 500;
    static int branchingBudget = 20;
    static int depthBudget = 2;
    public RetIterate Iterate(Node node, int depth, int alpha, int beta, bool Player)
    {
        i++;
        Debug.Log("iteration: " + i + ", depth: " + depth + ", alpha: " + alpha + ", beta: " + beta + ", Player: " + Player);
        if (i > budget)
        {
            Debug.Log("Breaking for out of budget");
            return new RetIterate(node.GetTotalScore(this.playerID, Player), node);
        }

        if (depth == 0 || node.IsTerminal())
        {
            Debug.Log("depth == 0 || node.IsTerminal(): " + (depth == 0) + ", " + node.IsTerminal());
            return new RetIterate(node.GetTotalScore(this.playerID, Player), node);
        }

        int m = 0;
        List<TileState> expanded = new List<TileState>();
        if (Player == MaxPlayer)
        {
            //Debug.Log("Max Player");
            Node selected = null;
            //foreach (Node child in node.Children(Player))
            Node child;

            while ((child = node.getNextChildNode(Player)) != null)
            {
                m++;
                if (m > branchingBudget)
                    break;
                Debug.Log("m: "+ m);
                //Debug.Log("child " + child.tile.tile.index);
                if (i > budget)
                {
                    Debug.Log("Breaking for out of budget");
                    break;
                }
                child.tile.currentState = this.playerID;
                child.playerMaxTiles.Add(child.tile);
                child.availableTiles.Remove(child.tile);
                node.availableTiles.Remove(child.tile);
                expanded.Add(child.tile);
                var result = Iterate(child, depth - 1, alpha, beta, !Player);
                if (alpha < result.score)
                    selected = child;
                alpha = Math.Max(alpha, result.score);
                //child.availableTiles.Add(child.tile);
                child.tile.resetState();
                child.playerMaxTiles.Remove(child.tile);
                if (beta < alpha)
                {
                    break;
                }
            }
            foreach (TileState ts in expanded)
                node.availableTiles.Add(ts);
            return new RetIterate(alpha, selected);
        }
        else
        {
            //Debug.Log("Min Player");
            Node selected = null;
            //foreach (Node child in node.Children(Player))
            Node child;
            while ((child = node.getNextChildNode(Player)) != null)
            {
                m++;
                if (m > branchingBudget)
                    break;
                Debug.Log("m: " + m);
                if (i > budget)
                {
                    Debug.Log("Breaking for out of budget");
                    break;
                }
                //Debug.Log("child " + child.tile.tile.index);
                child.tile.currentState = 1 - this.playerID;
                child.playerMinTiles.Add(child.tile);
                child.availableTiles.Remove(child.tile);
                node.availableTiles.Remove(child.tile);
                expanded.Add(child.tile);
                var result = Iterate(child, depth - 1, alpha, beta, !Player);
                if (beta > result.score)
                    selected = child;
                beta = Math.Min(beta, result.score);
                //child.availableTiles.Add(child.tile);
                child.tile.resetState();
                child.playerMinTiles.Remove(child.tile);
                if (beta < alpha)
                {
                    selected = child;
                    break;
                }
            }
            foreach (TileState ts in expanded)
                node.availableTiles.Add(ts);
            return new RetIterate(beta, selected);
        }
    }
}

public class Node
{
    public TileState tile;
    public Dictionary<string, Tile> grid;
    public List<TileState> playerMaxTiles, playerMinTiles, availableTiles;
    int winner = -1;
    public Node(Dictionary<string, Tile> grid, List<TileState> playerMaxTiles, List<TileState> playerMinTiles, List<TileState> availableTiles, TileState tile)
    {
        this.grid = grid;
        //this.playerMaxTiles = new List<TileState>(playerMaxTiles);
        //this.playerMinTiles = new List<TileState>(playerMinTiles);
        this.playerMaxTiles = playerMaxTiles;
        this.playerMinTiles = playerMinTiles;
        this.availableTiles = availableTiles;
        this.tile = tile;
    }

    public Node getNextChildNode(bool player)
    {
        if (availableTiles.Count == 0)
            return null;
        TileState ts = null;
        if (UnityEngine.Random.Range(0, 100) < 50)
            while (true)
            {
                ts = this.availableTiles[UnityEngine.Random.Range(0, this.availableTiles.Count)];
                if (ts.currentState == -1)
                    break;
            }
        else
        {
            int i = 0;
            while (++i < this.playerMaxTiles.Count)
            {
                var mx = this.playerMaxTiles[UnityEngine.Random.Range(0, this.playerMaxTiles.Count)];
                var n = HexGridUtil.Neighbours(this.grid, mx.tile);
                foreach (var t in n)
                {
                    var p = t.GetComponent<TileState>();
                    if (p.currentState == -1)
                    {
                        ts = p;
                        break;
                    }
                }
            }
            if (ts == null)
                ts = this.availableTiles[UnityEngine.Random.Range(0, this.availableTiles.Count)];
        }
        return new Node(grid, playerMaxTiles, playerMinTiles, availableTiles, ts);
    }

    public List<Node> Children(bool Player)
    {
        List<Node> children = new List<Node>();

        // Create your subtree here and return the results
        return children;
    }

    public bool IsTerminal()
    {
        bool terminalNode = false;

        // Game over?
        this.winner = HexGridUtil.CheckGameOver(this.grid);
        terminalNode = this.winner != -1;
        return terminalNode;
    }

    public int GetTotalScore(int playerID, bool Player)
    {
        int totalScore = 0;

        if (this.winner != -1)
            if (this.winner == playerID)
                return 100;
            else
                return -100;


        totalScore = evaluate(playerMaxTiles, playerID) - evaluate(playerMinTiles, 1 - playerID);



        //int c = 0;
        //while (frontier.Count > 0)
        //{
        //    c++;
        //    if (c > 1000)
        //    {
        //        break;
        //    }
        //    TileState ts = frontier[0];
        //    frontier.RemoveAt(0);
        //    expanded.Add(ts);
        //    foreach (TileState t in frontier)
        //    {
        //        if (HexGridUtil.IsNeighbour(ts.tile, t.tile))
        //            totalScore++;
        //    }
        //}

        //frontier = new List<TileState>(playerMinTiles);
        //expanded = new List<TileState>();
        //c = 0;
        //int enemyScore = 0;
        //while (frontier.Count > 0)
        //{
        //    c++;
        //    if (c > 1000)
        //    {
        //        break;
        //    }
        //    TileState ts = frontier[0];
        //    frontier.RemoveAt(0);
        //    expanded.Add(ts);
        //    foreach (TileState t in frontier)
        //    {
        //        if (HexGridUtil.IsNeighbour(ts.tile, t.tile))
        //            enemyScore++;
        //    }
        //}
        //Debug.Log("TotalScore: " + totalScore + " EnemyScore: " + enemyScore);
        //return totalScore - enemyScore;

        Debug.Log("TotalScore: " + totalScore);
        return totalScore;
    }

    int evaluate(List<TileState> playerTiles, int playerID)
    {
        int totalScore = 0;
        List<TileState> frontier = new List<TileState>(playerTiles);
        List<TileState> expanded = new List<TileState>();
        int c = 0;
        while (frontier.Count > 0)
        {
            c++;
            if (c > 1000)
            {
                Debug.LogError("Checking bridge exceeding time limit");
                break;
            }
            TileState ts = frontier[0];
            frontier.RemoveAt(0);
            expanded.Add(ts);
            List<Tile> n = HexGridUtil.Neighbours(grid, ts.tile);
            foreach (Tile t in n)
            {
                TileState nts = t.GetComponent<TileState>();
                if (nts.currentState == playerID)
                {
                    if (!expanded.Contains(nts))
                    {
                        //nts.highlight();
                        if (playerTiles.Contains(nts))
                        {
                            totalScore++;
                        }
                        frontier.Add(nts);
                    }
                }
            }
        }
        return totalScore;

    }

}