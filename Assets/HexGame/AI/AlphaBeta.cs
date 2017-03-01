using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RetIterate
{
    public int score = -1;
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
        if (this.playerID == 0)
            MaxPlayer = true;
        else
            MaxPlayer = false;
    }

    public TileState NextMove(Dictionary<string, Tile> grid, List<TileState> playerMaxTiles, List<TileState> playerMinTiles, List<TileState> availableTiles)
    {
        i = 0;
        Node n = new Node(grid, playerMaxTiles, playerMinTiles, availableTiles, null);
        RetIterate selected = Iterate(n, depthBudget, -9999, 9999, true);
        Node t = null;
        int s = -99999;
        if (MaxPlayer)
        {
            foreach (var c in n.children)
            {
                if (s == -99999)
                {
                    s = c.score;
                    t = c;
                    //Debug.Log(s + " Move Selected: " + t.tile.tile.index);
                }
                else
                {
                    if (s < c.score)
                    {

                        s = c.score;
                        t = c;
                        //Debug.Log(s + " Move Selected: " + t.tile.tile.index);
                    }
                }
            }
        }
        else
        {
            foreach (var c in n.children)
            {
                if (s == -99999)
                {
                    s = c.score;
                    t = c;
                    //Debug.Log(s + " Move Selected: " + t.tile.tile.index);
                }
                else
                {
                    if (s > c.score)
                    {
                        s = c.score;
                        t = c;
                        //Debug.Log(s + " Move Selected: " + t.tile.tile.index);
                    }
                }
            }
        }
        if (t != null)
            Debug.Log(i + " iterations" + " Final Move Selected: " + t.tile.tile.index + ", Note: " + t.note);
        else
            Debug.Log(i + " iterations" + " No Move Found!!!");
        return t.tile;
    }

    static int i = 0;
    static int budget = 2500;
    //static int branchingBudget = 4;
    static int depthBudget = 15;
    public RetIterate Iterate(Node node, int depth, int alpha, int beta, bool Player)
    {
        i++;
        //if(beta < alpha)
        //Debug.Log("iteration: " + i + ", depth: " + depth + ", alpha: " + alpha + ", beta: " + beta + ", Player: " + Player);
        if (i > budget)
        {
            //Debug.Log("Breaking for out of budget");
            return new RetIterate(node.GetTotalScore(this.playerID, Player), node);
        }

        if (depth == 0 || node.IsTerminal())
        {
            //Debug.Log("depth == 0 || node.IsTerminal(): " + (depth == 0) + ", " + node.IsTerminal());
            return new RetIterate(node.GetTotalScore(this.playerID, Player), node);
        }

        int m = 0;
        List<TileState> expanded = new List<TileState>();
        if (Player == MaxPlayer)
        {
            //Debug.Log("Max Player");
            Node selected = null;
            foreach (Node child in node.Children(this.playerID))
            {
                //Debug.Log("child " + child.tile.tile.index);
                if (i > budget)
                {
                    //Debug.Log("Breaking for out of budget");
                    break;
                }
                child.tile.currentState = this.playerID;
                child.playerMaxTiles.Add(child.tile);
                child.availableTiles.Remove(child.tile);
                var result = Iterate(child, depth - 1, alpha, beta, !Player);
                //if (result.score == -1 || alpha < result.score)
                //{                    
                //    selected = child;
                //    Debug.Log("SELECTED" + depth + ": curr: " + result.score + ": beta: " + beta + ": Move Selected: " + selected.tile.tile.index);
                //}
                alpha = Math.Max(alpha, result.score);
                child.score = result.score;
                child.availableTiles.Add(child.tile);
                child.playerMaxTiles.Remove(child.tile);
                child.tile.resetState();
                if (beta < alpha)
                {
                    //Debug.Log("MAX Pruning:" + "iteration: " + i + ", depth: " + depth + ", alpha: " + alpha + ", beta: " + beta + ", Player: " + Player + ": score: " + result.score);
                    break;
                }
            }
            //Debug.Log("iteration: " + i + ", depth: " + depth + ", alpha: " + alpha + ", beta: " + beta + ", Player: " + Player);
            return new RetIterate(alpha, selected);
        }
        else
        {
            //Debug.Log("Min Player");
            Node selected = null;
            foreach (Node child in node.Children(1 - this.playerID))
            //Node child;
            //while ((child = node.getNextChildNode(1 - this.playerID)) != null)
            {
                if (i > budget)
                {
                    //Debug.Log("Breaking for out of budget");
                    break;
                }
                //Debug.Log("child " + child.tile.tile.index);
                child.tile.currentState = 1 - this.playerID;
                child.playerMinTiles.Add(child.tile);
                child.availableTiles.Remove(child.tile);
                var result = Iterate(child, depth - 1, alpha, beta, !Player);
                //Debug.Log(depth + ": curr: " + result.score + ": beta: " + beta + ": Move Selected: " + selected.tile.tile.index);
                //if (result.score == -1 || beta < result.score) { 
                //    selected = child;
                //    Debug.Log("SELECTED " + depth + ": curr: " + result.score + ": beta: " + beta + ": Move Selected: " + selected.tile.tile.index);
                //}
                beta = Math.Min(beta, result.score);
                child.score = result.score;
                child.availableTiles.Add(child.tile);
                child.tile.resetState();
                child.playerMinTiles.Remove(child.tile);
                if (beta < alpha)
                {
                    //Debug.Log("MIN Pruning:" + "iteration: " + i + ", depth: " + depth + ", alpha: " + alpha + ", beta: " + beta + ", Player: " + Player + ": score: " + result.score);
                    selected = child;
                    break;
                }
            }
            return new RetIterate(beta, selected);
        }
    }
}

public class Node
{
    public int score = 0;
    public TileState tile;
    public Dictionary<string, Tile> grid;
    public List<TileState> playerMaxTiles, playerMinTiles, availableTiles;
    int winner = -1;
    public List<Node> children;
    public string note = "NONE";
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


    public Node getNextChildNode(int playerID)
    {
        if (availableTiles.Count == 0)
            return null;
        TileState ts = null;
        ts = MovesBank.maxSafePattern(this.grid, playerID, true);
        if (ts == null)
        {
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
        }
        else
        {
            Debug.LogError("BestMove Selected: " + ts.tile.index);
        }
        return new Node(grid, playerMaxTiles, playerMinTiles, availableTiles, ts);
    }

    public List<Node> Children(int playerID)
    {
        children = new List<Node>();

        TileState ts = null;
        //ts = MovesBank.BridgeTowardsGoal(playerID, this.playerMaxTiles, this.playerMinTiles, this.grid, null, true);
        //createNode(ts, children, "BridgeTowardsGoal: TRUE");
        //ts = MovesBank.BridgeTowardsGoal(playerID, this.playerMaxTiles, this.playerMinTiles, this.grid, null, false);
        //createNode(ts, children, "BridgeTowardsGoal: FALSE");

        //ts = MovesBank.maxSafePattern(this.grid, playerID, true);
        //createNode(ts, children, "maxSafePattern: TRUE");

        //ts = MovesBank.maxSafePattern(this.grid, playerID, false);
        //createNode(ts, children, "maxSafePattern: FALSE");

        //if (ts == null)
        //{
        //    ts = MovesBank.addRandomMove(this.availableTiles);
        //    createNode(ts, children, "RANDOM");
        //}
        List<TileState> currentTiles = new List<TileState>(this.availableTiles);
        for (int i = 0; i < 220; i++)
        {
            ts = MovesBank.addRandomMove(currentTiles);
            currentTiles.Remove(ts);
            createNode(ts, children, "RANDOM");
        }
        //Debug.Log("Total children added: " + this.children.Count);

        // Create your subtree here and return the results
        return children;
    }
    public Node createNode(TileState ts, List<Node> children, string note = "NONE")
    {
        if (ts == null)
            return null;
        Node n = new Node(grid, playerMaxTiles, playerMinTiles, availableTiles, ts);
        n.note = note;
        children.Add(n);
        return n;
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
                return 1000;
            else
                return -1000;


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

        //Debug.Log("TotalScore: " + totalScore);
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
