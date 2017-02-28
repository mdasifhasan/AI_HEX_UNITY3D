using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCTS : MonoBehaviour
{

    public Grid grid;
    public GameController gc;
    public int playerID;
    public Dictionary<TileState, List<TileState>> tried = new Dictionary<TileState, List<TileState>>();
    public void UCTSearch()
    {
        List<TileState> validTiles = new List<TileState>(gc.tileStates);
        Dictionary<string, Tile> tiles = new Dictionary<string, Tile>(grid.Tiles);
        TileState next = validTiles[Random.Range(0, validTiles.Count)];
        float t = 0;
        while (t < 2)
        {
            next = TreePolicy(next);
            t += Time.deltaTime;
        }
    }

    public TileState TreePolicy(TileState ts)
    {
        while (isNonTerminal(ts))
            if (!isFullyExpanded(ts))
                return Expand(ts);
            else
                ts = BestChild(ts);
        return ts;
    }

    public TileState Expand(TileState ts)
    {
        var n = grid.Neighbours(ts.tile);
        foreach(Tile t in n)
        {
            TileState c = t.GetComponent<TileState>();
            if (!tried.ContainsKey(c))
            {
                // add a new child to v
            }
        }
        return null;
    }

    public TileState BestChild(TileState ts)
    {
        return null;
    }

    bool isNonTerminal(TileState state)
    {
        return true;
    }
    bool isFullyExpanded(TileState state)
    {
        return true;
    }

}
