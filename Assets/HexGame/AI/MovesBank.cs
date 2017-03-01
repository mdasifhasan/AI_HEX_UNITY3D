using System.Collections.Generic;
using UnityEngine;

public class MovesBank
{

    public static TileState BridgeTowardsGoal(int playerID, List<TileState> playerMaxTiles, List<TileState> playerMinTiles, Dictionary<string, Tile> grid, List<TileState> ignore, bool closest)
    {
        
        List<Hex> lineStart, lineGoal;
        List<TileState> playerTiles;
        if (playerID == 0)
        {
            playerTiles = playerMinTiles;
            lineStart = FractionalHex.HexLinedraw(new Hex(0, 0, 0), new Hex(0, 7, -7));
            lineGoal = FractionalHex.HexLinedraw(new Hex(7, 0, -7), new Hex(7, 7, -14));
        }
        else
        {
            playerTiles = playerMaxTiles;
            lineStart = FractionalHex.HexLinedraw(new Hex(0, 0, 0), new Hex(7, 0, -7));
            lineGoal = FractionalHex.HexLinedraw(new Hex(0, 7, -7), new Hex(7, 7, -14));
        }

        //Debug.Log("PlayerID: " + playerID + " isMax: " + (playerTiles == playerMaxTiles )) ;

        int minD = -1;
        TileState ts = null;
        Tile tile = null;
        foreach (TileState t in playerTiles)
        {
            List<Tile> nn = HexGridUtil.Neighbours(grid, t.tile);
            foreach (Tile n in nn) {
                if (n.GetComponent<TileState>().currentState != -1)
                    continue;
                if (ignore != null && ignore.Count > 0 &&  ignore.Contains(n.GetComponent<TileState>()))
                    continue;
                foreach (Hex h in lineStart) {
                    int d = Hex.Distance(new Hex(n.index.x, n.index.y, n.index.z), new Hex(0, 0, 0));
                    if (closest) { 
                        if (minD == -1 || minD > d)
                        {
                            minD = d;
                            tile = n;
                        }
                    }
                    else
                    {
                        if (minD == -1 || minD < d)
                        {
                            minD = d;
                            tile = n;
                        }
                    }
                }
                foreach (Hex h in lineGoal)
                {
                    int d = Hex.Distance(new Hex(n.index.x, n.index.y, n.index.z), new Hex(0, 0, 0));
                    if (closest)
                    {
                        if (minD == -1 || minD > d)
                        {
                            minD = d;
                            tile = n;
                        }
                    }
                    else
                    {
                        if (minD == -1 || minD < d)
                        {
                            minD = d;
                            tile = n;
                        }
                    }
                }
            }
        }
        if (tile != null)
            ts = tile.GetComponent<TileState>();
        return ts;
    }

    public static TileState addRandomMove(List<TileState> availableTiles)
    {
        while (availableTiles.Count > 0)
        {
            var ts = availableTiles[UnityEngine.Random.Range(0, availableTiles.Count)];
            if (ts.currentState == -1)
                return ts;
        }
        return null;
    }

    public static TileState maxSafePattern(Dictionary<string, Tile> grid, int playerID, bool min)
    {
        int maxEmpty = -1;
        TileState max_ts = null;
        foreach (var t in grid.Values)
        {
            int empty = 0;
            TileState ts = null;
            TileState t1 = t.GetComponent<TileState>();
            if (t1.currentState == 1 - playerID)
                continue;
            if (t1.currentState == -1 && ts == null)
                ts = t1;
            var t2 = t1.getNeighbour(0, grid);
            if (t2 == null || t2.currentState == 1 - playerID)
                continue;
            if (t2.currentState == -1 && ts == null)
                ts = t2;
            if (t2.currentState == -1)
                empty++;
            var t3 = t2.getNeighbour(1, grid);
            if (t3 == null || t3.currentState == 1 - playerID)
                continue;
            if (t3.currentState == -1 && ts == null)
                ts = t3;
            if (t3.currentState == -1)
                empty++;
            var t4 = t2.getNeighbour(0, grid);
            if (t4 == null || t4.currentState == 1 - playerID)
                continue;
            if (t4.currentState == -1 && ts == null)
                ts = t4;
            if (t4.currentState == -1)
                empty++;
            var t5 = t3.getNeighbour(0, grid);
            if (t5 == null || t5.currentState == 1 - playerID)
                continue;
            if (t5.currentState == -1 && ts == null)
                ts = t5;
            if (t5.currentState == -1)
                empty++;
            var t6 = t5.getNeighbour(0, grid);
            if (t6 == null || t6.currentState == 1 - playerID)
                continue;
            if (t6.currentState == -1 && ts == null)
                ts = t6;
            if (t6.currentState == -1)
                empty++;

            if (min)
            {
                if (maxEmpty == -1 || empty > maxEmpty)
                {
                    max_ts = ts;
                    maxEmpty = empty;
                }
            }
            else
            {
                if (maxEmpty == -1 || empty < maxEmpty)
                {
                    max_ts = ts;
                    maxEmpty = empty;
                }
            }
        }
        //if (max_ts != null)
        //    Debug.LogError("BestMove min_empty: " + maxEmpty);
        return max_ts;
    }
}
