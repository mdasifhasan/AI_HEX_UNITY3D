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
        //if (this.playerID == 0)
        //    MaxPlayer = true;
        //else
        //    MaxPlayer = false;
    }


    public static int testMoveNo = 0;
    public TileState NextMove(Dictionary<string, Tile> grid, List<TileState> playerMaxTiles, List<TileState> playerMinTiles, List<TileState> availableTiles)
    {
        testMoveNo++;
        startTime = Time.realtimeSinceStartup;
        time = 0;
        i = 0;
        int initialScore = HexGridUtil.evaluate(grid, playerMaxTiles, playerID);
        //Debug.Log(playerID + " initialScore: " + initialScore );
        Node n = new Node(grid, playerMaxTiles, playerMinTiles, availableTiles, null);
        RetIterate selected = Iterate(n, depthBudget, -9999, 9999, true, initialScore);
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
            Debug.Log(i + " iterations" + " Final Move Selected: " + t.tile.tile.index + " with score: " + s + ", Note: " + t.note);
        else
            Debug.Log(i + " iterations" + " No Move Found!!!");
        Debug.Log("MaxDepth: " + maxDepth);
        return t.tile;
    }

    static float time = 0;
    static float i = 0;
    static float startTime = Time.realtimeSinceStartup;
    static int budget = 6;
    //static int branchingBudget = 4;
    static int depthBudget = 2;
    public static int totalRandomMoves = 100;

    int maxDepth = -1;

    /*
     * May be when breaking for out of budget, the score need be backed up
     */
    public RetIterate Iterate(Node node, int depth, int alpha, int beta, bool Player, int initialScore)
    {
        if (maxDepth == -1 || maxDepth > depth)
            maxDepth = depth;
        i++;
        time = Time.realtimeSinceStartup - startTime;
        //if(beta < alpha)
        //Debug.Log(node.note+ " - iteration: " + i + ", depth: " + depth + ", alpha: " + alpha + ", beta: " + beta + ", Player: " + Player);
        if (time > budget)
        {
            //int score = node.GetTotalScore(this.playerID, Player, initialScore);
            int score = Player ? 9999 : -9999;
            node.score = score;
            Debug.Log("Breaking for out of budget, depth: " + depth + " score: " + node.score);
            return new RetIterate(score, node);
        }

        if (depth == 0 || node.IsTerminal())
        {
            int score = node.GetTotalScore(this.playerID, Player, initialScore);
            node.score = score;
            //Debug.Log("depth == 0 || node.IsTerminal(): " + (depth == 0) + ", " + node.IsTerminal() + ", depth: " + depth + " score: " + node.score);
            return new RetIterate(score, node);
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
                //if (time > budget)
                //{
                //    Debug.Log("MAX Breaking for out of budget: " + depth + " selected: " + selected);
                //    //break;
                //    if (selected == null)
                //        return new RetIterate(9999, child);
                //    else
                //        break;
                //}
                child.tile.currentState = this.playerID;
                child.playerMaxTiles.Add(child.tile);
                child.availableTiles.Remove(child.tile);
                var result = Iterate(child, depth - 1, alpha, beta, !Player, initialScore);
                //if (result.score == -1 || alpha < result.score)
                //{                    
                //    selected = child;
                //    Debug.Log("SELECTED" + depth + ": curr: " + result.score + ": beta: " + beta + ": Move Selected: " + selected.tile.tile.index);
                //}
                int a = alpha;
                alpha = Math.Max(alpha, result.score);
                //Debug.Log("Alpha updated from: " + a + " to: " + alpha + " at depth: " + depth);
                child.score = result.score;
                child.availableTiles.Add(child.tile);
                child.playerMaxTiles.Remove(child.tile);
                child.tile.resetState();
                if (beta <= alpha)
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
                //if (time > budget)
                //{
                //    Debug.Log("MIN Breaking for out of budget: " + depth + " selected: " + selected);
                //    //break;
                //    if (selected == null)
                //        return new RetIterate(-9999, child);
                //    else
                //        break;
                //}
                //Debug.Log("child " + child.tile.tile.index);
                child.tile.currentState = 1 - this.playerID;
                child.playerMinTiles.Add(child.tile);
                child.availableTiles.Remove(child.tile);
                var result = Iterate(child, depth - 1, alpha, beta, !Player, initialScore);
                //Debug.Log(depth + ": curr: " + result.score + ": beta: " + beta + ": Move Selected: " + selected.tile.tile.index);
                //if (result.score == -1 || beta < result.score) { 
                //    selected = child;
                //    Debug.Log("SELECTED " + depth + ": curr: " + result.score + ": beta: " + beta + ": Move Selected: " + selected.tile.tile.index);
                //}
                int b = beta;
                beta = Math.Min(beta, result.score);
                //Debug.Log("Beta updated from: " + b + " to: " + beta + " at depth: " + depth);
                child.score = result.score;
                child.availableTiles.Add(child.tile);
                child.tile.resetState();
                child.playerMinTiles.Remove(child.tile);
                if (beta <= alpha)
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

        /*
         * Defense mechanism
         * Find Wayout of a blocked column
         */
        children = new List<Node>();

        Tile[] testMoves = GameObject.FindObjectOfType<GameController>().testMoves;
        TileState ts = null;
        if (testMoves.Length > AlphaBeta.testMoveNo - 1)
        {
            Tile t = testMoves[AlphaBeta.testMoveNo - 1];
            var s = t.GetComponent<TileState>();
            if(s.currentState == -1)
                createNode(s, children, "TestMove " + AlphaBeta.testMoveNo);
        }
        //ts = MovesBank.BridgeTowardsGoal_Player_2(playerID, this.playerMaxTiles, this.playerMinTiles, this.grid, null, true);
        //createNode(ts, children, "BridgeSimple: TRUE");
        //ts = MovesBank.BridgeTowardsGoal_Player_2(playerID, this.playerMaxTiles, this.playerMinTiles, this.grid, null, false);
        //createNode(ts, children, "BridgeSimple: False");

        //ts = MovesBank.BridgeTowardsGoal(playerID, this.playerMaxTiles, this.playerMinTiles, this.grid, null, true);
        //createNode(ts, children, "BridgeTowardsGoal: TRUE");
        //ts = MovesBank.BridgeTowardsGoal(playerID, this.playerMaxTiles, this.playerMinTiles, this.grid, null, false);
        //createNode(ts, children, "BridgeTowardsGoal: FALSE");

        //ts = MovesBank.maxSafePattern(this.grid, playerID, true);
        //createNode(ts, children, "maxSafePattern: TRUE");

        //ts = MovesBank.maxSafePattern(this.grid, playerID, false);
        //createNode(ts, children, "maxSafePattern: FALSE");

        if (ts == null && AlphaBeta.testMoveNo > 1)
        {
            ts = MovesBank.addRandomMove(this.availableTiles);
            createNode(ts, children, "RANDOM");
        }
        List<TileState> currentTiles = new List<TileState>(this.availableTiles);
        for (int i = 0; i < AlphaBeta.totalRandomMoves; i++)
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

    public int GetTotalScore(int playerID, bool Player, int initialScore)
    {
        int totalScore = 0;

        //if (this.winner != -1)
        //    if (this.winner == playerID)
        //        return 1000;
        //    else
        //        return -1000;


        //totalScore = evaluate(playerMaxTiles, playerID) - evaluate(playerMinTiles, 1 - playerID);
        //totalScore = evaluate(playerMaxTiles, playerID);
        //totalScore = HexGridUtil.evaluate(grid, playerMaxTiles, playerID) - HexGridUtil.evaluate(grid, playerMinTiles, 1 - playerID);
        totalScore = HexGridUtil.evaluate(grid, playerMaxTiles, playerID);
        //Debug.Log("TotalScore: " + totalScore);
        return totalScore;
    }

    public override string ToString()
    {
        if(this.tile != null)
            return "Node - tile: " + tile.tile.index + " score: " + score;
        else
            return "Node - tile: " + tile + " score: " + score;
    }
}
