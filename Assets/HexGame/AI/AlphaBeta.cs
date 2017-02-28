using System;
using System.Collections;
using System.Collections.Generic;

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

public class AlphaBeta  {
    bool MaxPlayer = true;
    int playerID = -1;
    public AlphaBeta(int playerID)
    {
        this.playerID = playerID;
    }

    public Tile NextMove(Dictionary<string, Tile> grid, List<TileState> playerMaxTiles, List<TileState> playerMinTiles)
    {
        RetIterate selected = Iterate(new Node(grid, playerMaxTiles, playerMinTiles, null), 10, -9999, 9999, true);
        return selected.node.tile;
    }

    public RetIterate Iterate(Node node, int depth, int alpha, int beta, bool Player)
    {
        if (depth == 0 || node.IsTerminal())
        {
            return new RetIterate( node.GetTotalScore(this.playerID), node);
        }

        if (Player == MaxPlayer)
        {
            Node selected = null;
            foreach (Node child in node.Children(Player))
            {
                alpha = Math.Max(alpha, Iterate(child, depth - 1, alpha, beta, !Player).score);
                if (beta < alpha)
                {
                    selected = child;
                    break;
                }
            }
            return new RetIterate(alpha, selected);
        }
        else
        {
            Node selected = null;
            foreach (Node child in node.Children(Player))
            {
                beta = Math.Min(beta, Iterate(child, depth - 1, alpha, beta, !Player).score);

                if (beta < alpha)
                {
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
    public Tile tile;
    Dictionary<string, Tile> grid;
    List<TileState> playerMaxTiles, playerMinTiles;
    int winner = -1;
    public Node(Dictionary<string, Tile> grid, List<TileState> playerMaxTiles, List<TileState> playerMinTiles, Tile tile)
    {
        this.grid = grid;
        this.playerMaxTiles = playerMaxTiles;
        this.playerMinTiles = playerMinTiles;
        this.tile = tile;
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

    public int GetTotalScore(int playerID)
    {
        int totalScore = 0;

        if (this.winner != -1)
            if (this.winner == playerID)
                return 100;
            else
                return -100;

        List<TileState> frontier = new List<TileState>(playerMaxTiles);
        List<TileState> expanded = new List<TileState>();
        int c = 0;
        while (frontier.Count > 0)
        {
            c++;
            if (c > 1000)
            {
                break;
            }
            TileState ts = frontier[0];
            frontier.RemoveAt(0);
            expanded.Add(ts);
            foreach (TileState t in frontier)
            {
                if (HexGridUtil.IsNeighbour(ts.tile, t.tile))
                    totalScore++;
            }
        }

        return totalScore;
    }

}
