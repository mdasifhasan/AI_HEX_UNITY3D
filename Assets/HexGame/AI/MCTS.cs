using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCTS_Node
{
    public bool isTerminal = false;
    public int depth = 0;
    public int myPlayerID = 0;
    public TileState tileState = null;
    public List<MCTS_Node> childs = new List<MCTS_Node>();
    public List<TileState> possibleMoves, myTiles, opponentTiles;
    public int winner = -1;
    public double delta;
    public int visits = 0;
    public MCTS_Node parent = null;
    public bool isFullyExpanded()
    {
        //Debug.Log("isFullyExpanded: " + (availableTiles.Count == 0));
        return possibleMoves.Count == 0;
    }

    public MCTS_Node(MCTS_Node parent)
    {
        if (parent.parent == null)
        {
            this.myPlayerID = parent.myPlayerID;
            this.myTiles = new List<TileState>(parent.myTiles);
            this.opponentTiles = new List<TileState>(parent.opponentTiles);
            this.possibleMoves = new List<TileState>(parent.possibleMoves);
            this.depth = parent.depth + 1;
            this.parent = parent;
        }
        else
        {
            this.myPlayerID = 1 - parent.myPlayerID;
            this.myTiles = new List<TileState>(parent.opponentTiles);
            this.opponentTiles = new List<TileState>(parent.myTiles);
            this.possibleMoves = new List<TileState>(parent.possibleMoves);
            this.depth = parent.depth + 1;
            this.parent = parent;
        }
    }
    public MCTS_Node(int playerID)
    {
        this.myPlayerID = playerID;
    }

}

public class MCTS
{
    Dictionary<string, Tile> grid;
    public bool destroy = false;
    double startTime, time = 0;
    public int budget = 2;
    Dictionary<int, int> counts = new Dictionary<int, int>();
    public void UCTSearch(int mode, int playerID, Dictionary<string, Tile> grid, List<TileState> availableTiles, List<TileState> myTiles, List<TileState> opponentTiles, Action<TileState> callback)
    {
        TimeRecorder.Instance.resetTimer("DefaultPolicy");
        TimeRecorder.Instance.resetTimer("TreePolicy");
        TimeRecorder.Instance.resetTimer("Backup");
        TimeRecorder.Instance.resetTimer("BestChild");
        TimeRecorder.Instance.resetTimer("Expand");
        TimeRecorder.Instance.resetTimer("isNonTerminal");
        TimeRecorder.Instance.resetTimer("isNonTerminal - 1");
        TimeRecorder.Instance.resetTimer("MCTS_AlphaBeta");
        //Debug.Log("UCT Search");
        try
        {
            int iterations = 0;
            this.grid = grid;
            int maxDepth = 0;

            startTime = TimeRecorder.Instance.getTime();
            MCTS_Node root = new MCTS_Node(playerID)
            {
                possibleMoves = new List<TileState>(availableTiles),
                myTiles = new List<TileState>(myTiles),
                opponentTiles = new List<TileState>(opponentTiles)
            };
            MCTS_Node v = root;
            while (time < budget)
            {
                if (destroy)
                    break;
                iterations++;
                //Debug.Log("UCT Search, time: " + time);
                TimeRecorder.Instance.startTimer("TreePolicy");
                v = TreePolicy(root);
                if (!counts.ContainsKey(v.depth))
                    counts.Add(v.depth, 1);
                else
                    counts[v.depth]++;
                if (maxDepth < v.depth)
                    maxDepth = v.depth;
                //Debug.Log("UCT Search, next node with delta: " + v.delta);
                TimeRecorder.Instance.stopTimer("TreePolicy");
                TimeRecorder.Instance.startTimer("DefaultPolicy");
                double delta = DefaultPolicy(v);
                TimeRecorder.Instance.stopTimer("DefaultPolicy");
                TimeRecorder.Instance.startTimer("Backup");

                //Debug.Log("Backup called from depth: " + v.depth);
                //Debug.Log("Backup called with delta: " + v.delta);
                Backup(v, delta);
                TimeRecorder.Instance.stopTimer("Backup");
                time = TimeRecorder.Instance.getTime() - startTime;
            }

            MCTS_Node bestChild = SelectFinalMove(mode, root, 0);
            TileState ts = bestChild.tileState;

            //TimeRecorder.Instance.printStat("DefaultPolicy");
            //TimeRecorder.Instance.printStat("TreePolicy");
            //TimeRecorder.Instance.printStat("Backup");
            //TimeRecorder.Instance.printStat("BestChild");
            //TimeRecorder.Instance.printStat("Expand");
            //TimeRecorder.Instance.printStat("isNonTerminal");
            //TimeRecorder.Instance.printStat("isNonTerminal - 1");
            TimeRecorder.Instance.printStat("MCTS_AlphaBeta");

            //int c = 0;
            //foreach (var m in counts.Keys)
            //{
            //    c++;
            //    if (c > 1000)
            //    {
            //        Debug.Log("break print");
            //        break;
            //    }
            //    if (destroy)
            //        break;
            //    Debug.Log(m + " - " + counts[m]);
            //}

            Debug.Log("Total iterations: " + iterations + " Best child: " + ts.tile.index + " score: " + bestChild.delta + " visits: " + bestChild.visits + " avgScore: " + (bestChild.delta/ (double) bestChild.visits) + " maxDepth: " + maxDepth);
            callback(ts);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void Backup(MCTS_Node v, double delta)
    {
        MCTS_Node current = v;
        do
        {
            current.visits++;
            current.delta += delta;
            delta = -delta;
            current = current.parent;
        }
        while (current != null);
    }

    private double DefaultPolicy(MCTS_Node v)
    {
        // have to handle win weight according to v.depth possibly
        if (v.winner != -1)
            if (v.winner == v.myPlayerID)
            {
                //Debug.Log(v.myPlayerID + " Me Winner depth: " + v.depth);
                return 1000;
            }
            else
            {
                //Debug.Log(v.myPlayerID + " Opponent Winner depth: " + v.depth);
                return -1000;
            }
        return HexGridUtil.evaluate(v.myTiles, v.myPlayerID) - HexGridUtil.evaluate(v.opponentTiles, 1 - v.myPlayerID); ;

        //TimeRecorder.Instance.startTimer("MCTS_AlphaBeta");
        ////Debug.Log("calling alpha beta");
        //AlphaBeta ab = new AlphaBeta(v.myPlayerID);
        //ab.budget = 120;
        //ab.depthBudget = 2;
        //Node n = ab.NextMove(this.grid, v.myTiles, v.opponentTiles, v.availableTiles, null);
        //TimeRecorder.Instance.stopTimer("MCTS_AlphaBeta");
        //return n.score;
        //return HexGridUtil.evaluate(v.myTiles, v.myPlayerID);
    }

    public MCTS_Node TreePolicy(MCTS_Node node)
    {
        MCTS_Node current = node;
        while (isNonTerminal(current))
            if (!current.isFullyExpanded())
                return Expand(current);
            else
            {
                //Debug.Log("Node fully expanded");
                MCTS_Node b = BestChild(current, 1.44f);
                if (b == null)
                    break;
                else
                    current = b;
            }
        return current;
    }
    System.Random Rand = new System.Random();
    public MCTS_Node Expand(MCTS_Node node)
    {
        TimeRecorder.Instance.startTimer("Expand");
        MCTS_Node child = new MCTS_Node(node);
        TileState ts = node.possibleMoves[Rand.Next(0, node.possibleMoves.Count)];
        child.tileState = ts;
        child.myTiles.Add(ts);
        node.possibleMoves.Remove(ts);
        child.possibleMoves.Remove(ts);
        node.childs.Add(child);
        //Debug.Log("expanding child to depth: " + child.depth);
        TimeRecorder.Instance.stopTimer("Expand");
        return child;
    }
    public MCTS_Node SelectFinalMove(int mode, MCTS_Node current, float C)
    {
        MCTS_Node bestChild = null;
        double best = double.NegativeInfinity;

        foreach (MCTS_Node child in current.childs)
        {
            double UCB1 = 0;
            if (mode == 0)
                UCB1 = ((double)child.delta / (double)child.visits) + C * Math.Sqrt((2.0 * Math.Log((double)current.visits)) / (double)child.visits);
            else if (mode == 1)
                UCB1 = ((double)child.delta / (double)child.visits) * 2 + child.visits;
            else 
                UCB1 = child.visits;

            if (UCB1 > best)
            {
                bestChild = child;
                best = UCB1;
            }
        }
        return bestChild;
    }
    public MCTS_Node BestChild(MCTS_Node current, float C)
    {
        TimeRecorder.Instance.startTimer("BestChild");
        //Debug.Log("In Best child, total childs: " +  current.childs.Count);
        MCTS_Node bestChild = null;
        double best = double.NegativeInfinity;

        foreach (MCTS_Node child in current.childs)
        {
            double UCB1 = ((double)child.delta / (double)child.visits) + C * Math.Sqrt((2.0 * Math.Log((double)current.visits)) / (double)child.visits);

            if (UCB1 > best)
            {
                bestChild = child;
                best = UCB1;
            }
        }
        TimeRecorder.Instance.stopTimer("BestChild");
        //Debug.Log("In Best child: " + bestChild.tileState.tile.index);
        return bestChild;
    }

    public bool isNonTerminal(MCTS_Node node)
    {
        TimeRecorder.Instance.startTimer("isNonTerminal");
        if (node.isTerminal)
            return false;
        if (node.myTiles.Count < 8)
            return true;
        List<TileState> changed = new List<TileState>();

        foreach (TileState ts in node.myTiles)
        {
            if (ts.currentState == -1)
            {
                ts.currentState = node.myPlayerID;
                changed.Add(ts);
            }
        }
        foreach (TileState ts in node.opponentTiles)
        {
            if (ts.currentState == -1)
            {
                ts.currentState = 1 - node.myPlayerID;
                changed.Add(ts);
            }
        }
        TimeRecorder.Instance.startTimer("isNonTerminal - 1");
        node.winner = HexGridUtil.CheckGameOver(grid);
        if (node.winner != -1)
            node.isTerminal = true;
        TimeRecorder.Instance.stopTimer("isNonTerminal - 1");
        foreach (TileState ts in changed)
        {
            ts.currentState = -1;
        }
        TimeRecorder.Instance.stopTimer("isNonTerminal");
        return node.winner == -1;
    }
}
