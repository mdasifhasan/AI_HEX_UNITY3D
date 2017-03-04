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
                TileState ts = t.tileState;
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
                TileState ts = t.tileState;
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
            List<Tile> n = ts.tile.neighbours;
            //List<Tile> n = Neighbours(grid, ts.tile);
            foreach (Tile t in n)
            {
                TileState nts = t.tileState;
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
            TimeRecorder.Instance.startTimer("Neighbours-1");
            Hex h = Hex.Neighbor(new Hex(tile.index.x, tile.index.y, tile.index.z), i);
            TimeRecorder.Instance.stopTimer("Neighbours-1");
            TimeRecorder.Instance.startTimer("Neighbours-2");
            //Tile t = TileAt(grid, h.q, h.r, h.s);
            Tile t = null;
            string s = string.Format("Hex[{0},{1},{2}]", h.q, h.r, h.s);
            GameObject g = GameObject.Find(s);
            if (g != null)
                t = g.GetComponent<Tile>();
            TimeRecorder.Instance.stopTimer("Neighbours-2");
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
        TimeRecorder.Instance.startTimer("evaluate");
        //Debug.Log("Evaluating for player: " + playerID);
        int totalScore = 0;
        List<TileState> frontier = new List<TileState>(playerTiles);
        List<TileState> expanded = new List<TileState>();

        Dictionary<int, List<TileState>> tilesByLayer = new Dictionary<int, List<TileState>>();

        int lowestLayerID = -1;
        int lastLayerID = -1;
        int maxChainLength = 0;
        List<TileState> maxTile = null;
        TimeRecorder.Instance.startTimer("evaluate-prepareLayers");
        foreach (TileState t in frontier)
        { 
            t.data.isExpanded = false;
            int currentLevel = getHexIndexForPlayer(playerID, t);
            if (lowestLayerID == -1 || lowestLayerID > currentLevel)
                lowestLayerID = currentLevel;
            if (lastLayerID == -1 || lastLayerID < currentLevel)
                lastLayerID = currentLevel;
            if (!tilesByLayer.ContainsKey(currentLevel))
                tilesByLayer.Add(currentLevel, new List<TileState>());
            tilesByLayer[currentLevel].Add(t);
            if (t.data == null)
                t.data = new AstarData();
            //t.data = null;
        }
        TimeRecorder.Instance.stopTimer("evaluate-prepareLayers");

        int c = 0;
        for (int i = lowestLayerID; i <= lastLayerID; i++)
        {
            //Debug.Log("Working for i: " + i);
            if (!tilesByLayer.ContainsKey(i))
                continue;
            frontier = new List<TileState>(tilesByLayer[i]);
            foreach (TileState t in frontier)
            {
                t.data.set(7 - i, 1, 0, 0);
            }

            TimeRecorder.Instance.startTimer("evaluate-inner-while-loop");
            
            int curLayer = i;
            while (frontier.Count > 0)
            {
                c++;
                if (c > 1000)
                {
                    Debug.LogError("Evaluation exceeded time limit");
                    break;
                }
                TimeRecorder.Instance.startTimer("evaluate-inner-while-loop-2");
                TileState ts = frontier[0];
                frontier.Remove(ts);
                //expanded.Add(ts);
                ts.data.isExpanded = true;

                TimeRecorder.Instance.stopTimer("evaluate-inner-while-loop-2");
                //Debug.Log(ts.data.chainLength + ": Current Node: " + ts.tile.index + " frontier: " + frontier.Count + " chainLength: " + ts.data.chainLength);
                //List<Tile> n = new List<Tile>( ts.tile.neighbours);
                List<Tile> n = ts.tile.neighbours;
                //HexGridUtil.Neighbours(grid, ts.tile);
                TimeRecorder.Instance.startTimer("evaluate-inner-while-loop-1");
                int parentLayer = getHexIndexForPlayer(playerID, ts);
                // for each of the child
                foreach (Tile t in n)
                {
                  
                    //TileState nts = t.GetComponent<TileState>();
                    TileState nts = t.tileState;

                    int childLayer = getHexIndexForPlayer(playerID, nts);

                    if (nts.currentState != playerID)
                        continue;

                    //Debug.Log("parentLayer: " + parentLayer + " childLayer: " + childLayer);
                    if (childLayer > parentLayer)
                    {
                        if (!frontier.Contains(nts) && !nts.data.isExpanded )
                        {
                            nts.data.set(7 - childLayer, childLayer - i, (childLayer - i), ts.data.horizontalNeighbours);
                            addToFrontier(frontier, nts);
                            //Debug.Log("Diff Layer Neighbour Node: " + t.index + " chainLength: " + nts.data.chainLength + " horizontalNeighbours: " + nts.data.horizontalNeighbours);
                        }
                    }
                    else if (childLayer == parentLayer)
                    {
                        if (!frontier.Contains(nts) && !nts.data.isExpanded)
                        {
                            nts.data.set(7 - childLayer, childLayer - i, (childLayer - i), ts.data.horizontalNeighbours + 1);
                            addToFrontier(frontier, nts);
                            //Debug.Log("Same Layer Neighbour Node: " + t.index + " chainLength: " + nts.data.chainLength + " horizontalNeighbours: " + nts.data.horizontalNeighbours);
                        }
                    }
                }
                TimeRecorder.Instance.stopTimer("evaluate-inner-while-loop-1");
                int chainLength = ts.data.score;
                if (maxChainLength == -1 || maxChainLength < chainLength)
                {
                    maxChainLength = chainLength;
                    //Debug.Log("maxChainLength updated to: " + maxChainLength + " chainLength: " + ts.data.chainLength + " horizontalNeighbours: " + ts.data.horizontalNeighbours);
                }
            }
            TimeRecorder.Instance.stopTimer("evaluate-inner-while-loop");
        }

        totalScore = maxChainLength;

        TimeRecorder.Instance.stopTimer("evaluate");
        return totalScore;
    }

    public static void addToFrontier(List<TileState> frontier, TileState ts)
    {
        TimeRecorder.Instance.startTimer("evaluate-inner-while-loop-3");
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

        TimeRecorder.Instance.stopTimer("evaluate-inner-while-loop-3");
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
