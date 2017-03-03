using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridUtil
{
    public static int CheckGameOver(Dictionary<string, Tile> grid)
    {
        //Debug.Log("CheckGameOver()");
        var lineStart = FractionalHex.HexLinedraw(new Hex(0, 0, 0), new Hex(7, 0, -7));
        var lineGoal = FractionalHex.HexLinedraw(new Hex(0, 7, -7), new Hex(7, 7, -14));
        if (CheckPlayerWin(grid, 0, lineStart, lineGoal))
            return 0;
        else
        {
            lineStart = FractionalHex.HexLinedraw(new Hex(0, 0, 0), new Hex(0, 7, -7));
            lineGoal = FractionalHex.HexLinedraw(new Hex(7, 0, -7), new Hex(7, 7, -14));
            if (CheckPlayerWin(grid, 1, lineStart, lineGoal))
                return 1;
        }
        return -1;

    }

    public static Tile TileAt(Dictionary<string, Tile> grid, int x, int y, int z)
    {
        CubeIndex index = new CubeIndex(x, y, z);
        if (grid.ContainsKey(index.ToString()))
            return grid[index.ToString()];
        return null;
    }

    private static bool CheckPlayerWin(Dictionary<string, Tile> grid, int playerID, List<Hex> lineStart, List<Hex> lineGoal)
    {
        //Debug.Log("CheckPlayerWin: " + playerID);
        //foreach (Tile t in grid.Tiles.Values)
        //{
        //    TileState ts = t.GetComponent<TileState>();
        //    ts.resetHighLight();
        //}
        List<TileState> frontier = new List<TileState>();
        List<TileState> goals = new List<TileState>();
        foreach (Hex n in lineStart)
        {
            Tile t = TileAt(grid, n.q, n.r, n.s);
            if (t != null)
            {
                TileState ts = t.GetComponent<TileState>();
                //ts.setTileState(2);
                if (ts.currentState == playerID)
                {
                    frontier.Add(ts);
                }
            }
        }

        foreach (Hex n in lineGoal)
        {
            Tile t = TileAt(grid, n.q, n.r, n.s);
            if (t != null)
            {
                TileState ts = t.GetComponent<TileState>();
                //ts.setTileState(2);
                if (ts.currentState == playerID)
                {
                    goals.Add(ts);
                }
            }
        }

        List<TileState> expanded = new List<TileState>();
        int c = 0;
        while (frontier.Count > 0)
        {
            c++;
            if (c > 1000)
            {
                Debug.LogError("Checking game over exceeding time limit");
                break;
            }
            TileState ts = frontier[0];
            frontier.RemoveAt(0);
            expanded.Add(ts);
            List<Tile> n = Neighbours(grid, ts.tile);
            foreach (Tile t in n)
            {
                TileState nts = t.GetComponent<TileState>();
                if (nts.currentState == playerID)
                {
                    if (!expanded.Contains(nts))
                    {
                        //nts.highlight();
                        if (goals.Contains(nts))
                        {
                            return true;
                        }
                        frontier.Add(nts);
                    }
                }
            }
        }
        return false;
    }

    public static List<Tile> Neighbours(Dictionary<string, Tile> grid, Tile tile)
    {
        List<Tile> ret = new List<Tile>();
        for (int i = 0; i < 6; i++)
        {
            Hex h = Hex.Neighbor(new Hex(tile.index.x, tile.index.y, tile.index.z), i);
            Tile t = TileAt(grid, h.q, h.r, h.s);
            if (t != null)
                ret.Add(t);
        }
        return ret;
    }

    public static bool IsNeighbour(Tile tile, Tile neighbour)
    {
        for (int i = 0; i < 6; i++)
        {
            Hex h = Hex.Neighbor(new Hex(tile.index.x, tile.index.y, tile.index.z), i);
            if (!(h.q == neighbour.index.x && h.r == neighbour.index.y && h.s == neighbour.index.z))
            {
                return true;
            }
        }
        return false;
    }
    static int getHexIndexForPlayer(int playerID, TileState ts)
    {
        if (playerID == 1)
            return ts.tile.index.x;
        else
            return ts.tile.index.y;
    }

    public static int evaluate(Dictionary<string, Tile> grid, List<TileState> playerTiles, int playerID)
    {
        //Debug.Log("Evaluating for player: " + playerID);
        int totalScore = 0;
        List<TileState> frontier = new List<TileState>(playerTiles);
        List<TileState> expanded = new List<TileState>();

        Dictionary<int, List<TileState>> tilesByLayer = new Dictionary<int, List<TileState>>();

        int lowestLayerID = -1;
        int lastLayerID = -1;
        int maxChainLength = 0;
        foreach (TileState t in frontier)
        {
            int currentLevel = getHexIndexForPlayer(playerID, t);
            if (lowestLayerID == -1 || lowestLayerID > currentLevel)
                lowestLayerID = currentLevel;
            if (lastLayerID == -1 || lastLayerID < currentLevel)
                lastLayerID = currentLevel;
            if (!tilesByLayer.ContainsKey(currentLevel))
                tilesByLayer.Add(currentLevel, new List<TileState>());
            tilesByLayer[currentLevel].Add(t);
            t.data = null;
        }

        int c = 0;
        for (int i = lowestLayerID; i <= lastLayerID; i++)
        {
            //Debug.Log("Working for i: " + i);
            if (!tilesByLayer.ContainsKey(i))
                continue;
            frontier = new List<TileState>(tilesByLayer[i]);
            foreach (TileState t in frontier)
            {
                t.data = new AstarData(7 - i, 1, 1);
            }
            int curLayer = i;
            while (frontier.Count > 0)
            {
                c++;
                if (c > 1000)
                {
                    //Debug.LogError("Checking bridge exceeding time limit");
                    break;
                }
                TileState ts = frontier[0];
                frontier.Remove(ts);
                expanded.Add(ts);
                //Debug.Log(ts.data.chainLength + ": Current Node: " + ts.tile.index + " frontier: " + frontier.Count + " chainLength: " + ts.data.chainLength);
                List<Tile> n = HexGridUtil.Neighbours(grid, ts.tile);

                int parentLayer = getHexIndexForPlayer(playerID, ts);
                bool end = true;
                // for each of the child
                foreach (Tile t in n)
                {
                    TileState nts = t.GetComponent<TileState>();

                    int childLayer = getHexIndexForPlayer(playerID, nts);

                    if (nts.currentState != playerID)
                        continue;

                    //Debug.Log("parentLayer: " + parentLayer + " childLayer: " + childLayer);
                    if (childLayer >= parentLayer)
                    {
                        if (!frontier.Contains(nts) && !expanded.Contains(nts))
                        {
                            nts.data = new AstarData(7 - childLayer, childLayer - i, childLayer - i);
                            addToFrontier(frontier, nts);
                            //Debug.Log("Same Layer Neighbour Node: " + t.index + " chainLength: " + nts.data.chainLength);
                        }
                        end = false;
                    }

                    //if (tilesByLayer[curLayer].Contains(nts))
                    //{
                    //    if (!frontier.Contains(nts) && !expanded.Contains(nts))
                    //    {
                    //        nts.data = new AstarData(7 - curLayer, ts.data.g, curLayer);
                    //        addToFrontier(frontier, nts);
                    //        Debug.Log(nts.data.layer + ": Same Layer Neighbour Node: " + t.index + " chainLength: " + nts.data.layer);
                    //    }
                    //    end = false;
                    //}
                    //else if (tilesByLayer.ContainsKey(curLayer + 1) && tilesByLayer[curLayer + 1].Contains(nts))
                    //{
                    //    if (!frontier.Contains(nts) && !expanded.Contains(nts))
                    //    {
                    //        nts.data = new AstarData(7 - curLayer, ts.data.g, curLayer + 1);
                    //        addToFrontier(frontier, nts);

                    //        Debug.Log(nts.data.layer + ": Next Layer Neighbour Node: " + t.index + " chainLength: " + nts.data.layer);
                    //    }
                    //    end = false;
                    //}
                }
                //Debug.Log("After adding childs, frontier length: " + frontier.Count);
                //if (end)
                //{
                //if (end)
                //    Debug.Log("EnD: " + end + " curMax: " + maxChainLength + " curLayer: " + curLayer);
                int chainLength = ts.data.chainLength;
                if (maxChainLength == -1 || maxChainLength < chainLength)
                {
                    maxChainLength = chainLength;
                    //Debug.Log("maxChainLength updated to: " + maxChainLength);
                }
                //}
            }
        }
        totalScore = maxChainLength;
        return totalScore;
    }

    public static void addToFrontier(List<TileState> frontier, TileState ts)
    {
        bool isAdded = false;
        for (int i = 0; i < frontier.Count; i++)
        {
            if (frontier[i].data.f > ts.data.f)
            {
                frontier.Insert(i, ts);
                isAdded = true;
                break;
            }
        }
        if (!isAdded)
            frontier.Add(ts);
    }

    public static int evaluate_no_of_connected(Dictionary<string, Tile> grid, List<TileState> playerTiles, int playerID)
    {
        int totalScore = 0;
        List<TileState> frontier = new List<TileState>(playerTiles);
        List<TileState> visited = new List<TileState>();


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
            visited.Add(ts);

            List<Tile> n = HexGridUtil.Neighbours(grid, ts.tile);
            foreach (Tile t in n)
            {
                TileState nts = t.GetComponent<TileState>();
                if (nts.currentState == playerID)
                {
                    if (!visited.Contains(nts))
                    {
                        visited.Add(nts);
                        totalScore++;
                    }
                }
            }
        }
        return totalScore;

    }
}
