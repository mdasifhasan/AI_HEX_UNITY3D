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
    public List<TileState> possibleMoves, myTiles, opponentTiles, availableMoves;
    public int winner = -1;
    public double delta;
    public int visits = 0;
    public MCTS_Node parent = null;
    public bool isFullyExpanded()
    {
        //Debug.Log("isFullyExpanded: " + (availableTiles.Count == 0));
        return availableMoves.Count == 0;
    }

    public MCTS_Node(MCTS_Node parent)
    {
        if (parent.parent == null)
        {
            this.myPlayerID = parent.myPlayerID;
            this.myTiles = new List<TileState>(parent.myTiles);
            this.opponentTiles = new List<TileState>(parent.opponentTiles);
            this.possibleMoves = new List<TileState>(parent.possibleMoves);
            this.availableMoves = new List<TileState>(parent.availableMoves);
            this.depth = parent.depth + 1;
            this.parent = parent;
        }
        else
        {
            this.myPlayerID = 1 - parent.myPlayerID;
            this.myTiles = new List<TileState>(parent.opponentTiles);
            this.opponentTiles = new List<TileState>(parent.myTiles);
            this.possibleMoves = new List<TileState>(parent.possibleMoves);
            this.availableMoves = new List<TileState>(parent.availableMoves);
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
                availableMoves = new List<TileState>(availableTiles),
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
                if (maxDepth < v.depth)
                    maxDepth = v.depth;
                //Debug.Log("UCT Search, next node with delta: " + v.delta);
                TimeRecorder.Instance.stopTimer("TreePolicy");
                TimeRecorder.Instance.startTimer("DefaultPolicy");
                double delta = DefaultPolicy(v);
                //Debug.Log("UCT Search, sim delta: " + delta);
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

            TimeRecorder.Instance.printStat("DefaultPolicy");
            //TimeRecorder.Instance.printStat("TreePolicy");
            //TimeRecorder.Instance.printStat("Backup");
            //TimeRecorder.Instance.printStat("BestChild");
            //TimeRecorder.Instance.printStat("Expand");
            //TimeRecorder.Instance.printStat("isNonTerminal");
            //TimeRecorder.Instance.printStat("isNonTerminal - 1");
            //TimeRecorder.Instance.printStat("MCTS_AlphaBeta");

            Debug.Log("Total iterations: " + iterations + " Best child: " + ts.tile.index + " score: " + bestChild.delta + " visits: " + bestChild.visits + " avgScore: " + (bestChild.delta / (double)bestChild.visits) + " maxDepth: " + maxDepth);
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
        if (v.winner != -1)
        {
            if (v.winner == v.myPlayerID)
            {
                //Debug.Log(v.myPlayerID + " Me Winner depth: " + v.depth + " index: " + v.tileState.tile.index);
                return 100;
            }
            else
            {
                //Debug.Log(v.myPlayerID + " Opponent Winner depth: " + v.depth + " index: " + v.tileState.tile.index);
                return -100;
            }
        }
        int[] win = new int[2];
        int totalPlay = 0;

        List<TileState> possibleMoves = new List<TileState>(v.possibleMoves);
        List<TileState> changed = new List<TileState>();
        List<TileState> myTriedMoves = new List<TileState>();
        List<TileState> oppTriedMoves = new List<TileState>();
        foreach (TileState ts in v.myTiles)
        {
            if (ts.currentState == -1)
            {
                ts.currentState = v.myPlayerID;
                changed.Add(ts);
            }
        }
        foreach (TileState ts in v.opponentTiles)
        {
            if (ts.currentState == -1)
            {
                ts.currentState = 1 - v.myPlayerID;
                changed.Add(ts);
            }
        }
        int sizeMoves = possibleMoves.Count;
        //var board = new Dictionary<string, Tile>(grid);
        for (int i = 0; i < 5; i++)
        {
            //possibleMoves = new List<TileState>(v.possibleMoves);
            //Debug.Log(i + " inner loop, BeginSizeMoves: " + sizeMoves + " nowSizeMoves: " + possibleMoves.Count);

            foreach (TileState ts in v.myTiles)
            {
                ts.initTileSet();
            }
            foreach (TileState ts in v.opponentTiles)
            {
                ts.initTileSet();
            }
            foreach (TileState ts in v.possibleMoves)
            {
                ts.currentState = -1;
                ts.initTileSet();
            }
            foreach (var t in v.myTiles)
            {
                t.updateTileSet();
            }
            foreach (var t in v.opponentTiles)
            {
                t.updateTileSet();
            }
            int currentTurn = v.myPlayerID;
            int c = 0;
            while (possibleMoves.Count > 0)
            {
                if (++c > 100)
                {
                    Debug.LogError("Simulation exceeded budget");
                    return 0;
                }
                var move = MovesBank.addRandomMove(possibleMoves);

                if (move == null)
                {
                    Debug.Log("Weird! No moves available whereas game is not finised !?");
                    break;
                }

                possibleMoves.Remove(move);
                if (currentTurn == v.myPlayerID)
                {
                    v.myTiles.Add(move);
                    myTriedMoves.Add(move);
                }
                else
                {
                    v.opponentTiles.Add(move);
                    oppTriedMoves.Add(move);
                }

                //Debug.Log(i + " PossibleMoves size: " + possibleMoves.Count + " myTriedMoves size: " + myTriedMoves.Count + " oppTriedMoves size: " + oppTriedMoves.Count);
                move.currentState = currentTurn;
                move.updateTileSet();
                //int cl = move.tileSet.GetRoot().chainLength;

                int cl = 0;
                int winner = -1;
                int maxChainLength = 0;
                if (currentTurn == v.myPlayerID)
                {
                    foreach (var t in v.myTiles)
                    {
                        if (t.tileSet.GetRoot().chainLength > maxChainLength)
                        {
                            maxChainLength = t.tileSet.GetRoot().chainLength;
                        }
                    }
                    cl = maxChainLength;
                    if (cl >= 7)
                        winner = v.myPlayerID;
                }
                else
                {
                    maxChainLength = 0;
                    foreach (var t in v.opponentTiles)
                    {
                        if (t.tileSet.GetRoot().chainLength > maxChainLength)
                        {
                            maxChainLength = t.tileSet.GetRoot().chainLength;
                        }
                    }
                    cl = maxChainLength;
                    if (cl >= 7)
                        winner = 1 - v.myPlayerID;
                }

                //if (cl >= 7)
                //{
                //    if (currentTurn == v.myPlayerID)
                //        winner = v.myPlayerID;
                //    else
                //        winner = 1 - v.myPlayerID;
                //}
                //if (possibleMoves.Count == 0)
                //{
                //    int chainLength = 0;
                //    if (currentTurn == v.myPlayerID)
                //    {
                //        chainLength = HexGridUtil.evaluate(v.myTiles, v.myPlayerID);
                //        if (chainLength >= 7)
                //            winner = v.myPlayerID;
                //    }
                //    else
                //    {
                //        chainLength = HexGridUtil.evaluate(v.opponentTiles, 1 - v.myPlayerID);
                //        if (chainLength >= 7)
                //            winner = 1 - v.myPlayerID;
                //    }

                //    Debug.LogError("PossibleMoves got to zero, isTerminal? winner: " + winner + " chainlength: " + chainLength + " cl: " + cl + " move.root" + (move.tileSet == move.tileSet.GetRoot()));
                //    Debug.Log("Game: " + i + ", opponentTiles: " + v.opponentTiles.Count + ", myTiles: " + v.myTiles.Count + ", BeginSizeMoves: " + sizeMoves + " PossibleMoves size: " + possibleMoves.Count + " myTriedMoves size: " + myTriedMoves.Count + " oppTriedMoves size: " + oppTriedMoves.Count);
                //}

                currentTurn = 1 - currentTurn;
                //Debug.Log("calc winner: " + winner);
                if (winner != -1)
                {
                    totalPlay++;
                    win[winner]++;
                    //Debug.Log("Simulation terminal point: " + winner);
                    break;
                }
            }

            foreach (TileState ts in myTriedMoves)
            {
                v.myTiles.Remove(ts);
                ts.currentState = -1;
                possibleMoves.Add(ts);
            }
            myTriedMoves.Clear();
            foreach (TileState ts in oppTriedMoves)
            {
                v.opponentTiles.Remove(ts);
                ts.currentState = -1;
                possibleMoves.Add(ts);
            }
            oppTriedMoves.Clear();
        }

        foreach (TileState ts in changed)
        {
            ts.currentState = -1;
        }
        if (totalPlay == 0)
            return 0;
        return win[v.myPlayerID] / (double)totalPlay;
        //return win[v.myPlayerID] / (double)totalPlay * 10 - win[1 - v.myPlayerID] / (double)totalPlay * 10;
    }
    private double DefaultPolicyEval(MCTS_Node v)
    {
        bool isTerminal = !isNonTerminal(v);
        // have to handle win weight according to v.depth possibly
        if (v.winner != -1)
        {
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
        }

        int score = 0;
        List<TileState> changed = new List<TileState>();

        foreach (TileState ts in v.myTiles)
        {
            if (ts.currentState == -1)
            {
                ts.currentState = v.myPlayerID;
                changed.Add(ts);
            }
        }
        foreach (TileState ts in v.opponentTiles)
        {
            if (ts.currentState == -1)
            {
                ts.currentState = 1 - v.myPlayerID;
                changed.Add(ts);
            }
        }
        score = HexGridUtil.evaluate(v.myTiles, v.myPlayerID) - HexGridUtil.evaluate(v.opponentTiles, 1 - v.myPlayerID);
        foreach (TileState ts in changed)
        {
            ts.currentState = -1;
        }

        return score;

        //TimeRecorder.Instance.startTimer("MCTS_AlphaBeta");
        ////Debug.Log("calling alpha beta");
        //AlphaBeta ab = new AlphaBeta(v.myPlayerID);
        //ab.budget = 120;
        //ab.depthBudget = 2;
        //Node n = ab.NextMove(this.grid, v.myTiles, v.opponentTiles, v.possibleMoves, null);
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
        TileState ts = node.availableMoves[Rand.Next(0, node.availableMoves.Count)];
        child.tileState = ts;
        child.myTiles.Add(ts);
        node.availableMoves.Remove(ts);
        child.availableMoves.Remove(ts);
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
            double UCB1 = -1;
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
        //TimeRecorder.Instance.startTimer("isNonTerminal - 1");
        //node.winner = HexGridUtil.CheckGameOver(grid);
        node.winner = HexGridUtil.CheckGameOver(node.myPlayerID, node.myTiles, node.opponentTiles);
        if (node.winner != -1)
            node.isTerminal = true;
        //TimeRecorder.Instance.stopTimer("isNonTerminal - 1");
        foreach (TileState ts in changed)
        {
            ts.currentState = -1;
        }
        //TimeRecorder.Instance.stopTimer("isNonTerminal");
        return node.winner == -1;
    }
}
