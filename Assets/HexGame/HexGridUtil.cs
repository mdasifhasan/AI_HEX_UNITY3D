using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridUtil  {


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
}
