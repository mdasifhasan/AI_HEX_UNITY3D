using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AstarData
{
    public int chainLength = 0;
    public int h = 0;
    public int g = 0;
    public int horizontalNeighbours = 0;
    public static int weightChainLength = 1;
    public static int weightHorizontalNeighbours = 0;


    public bool isExpanded = false;
    public int f
    {
        get
        {
            return this.g + this.h;
        }
    }
    public int score
    {
        get
        {
            return this.chainLength * weightChainLength + this.horizontalNeighbours * weightHorizontalNeighbours;
            //return this.chainLength * 100 ;
        }
    }
    public AstarData(int h, int g, int layer, int horizontalNeighbours)
    {
        this.h = h;
        this.g = g;
        this.horizontalNeighbours = horizontalNeighbours;
        this.chainLength = layer;
    }
    public void set(int h, int g, int layer, int horizontalNeighbours)
    {
        this.h = h;
        this.g = g;
        this.horizontalNeighbours = horizontalNeighbours;
        this.chainLength = layer;
    }
    public AstarData()
    {

    }
    public List<TileState> path = new List<TileState>();
}
public class TileSet
{
    public int high = int.MinValue;
    public int low = int.MaxValue;
    public int size
    {
        get
        {
            return set.Count;
        }
    }
    public List<TileState> set = new List<TileState>();
    public int chainLength
    {
        get
        {
            return high - low;
        }
    }

    internal void unify(TileSet n)
    {
        set.AddRange(n.set);
        if (n.high > high)
            high = n.high;
        else if (n.high < high)
            n.high = high;

        if (n.low < low)
            low = n.low;
        else if (n.low > low)
            n.low = low;
    }
}


public class TileState : MonoBehaviour
{
    public Color colorBlack = new Color(0, 0, 0, 1);
    public Color colorWhite = new Color(1, 1, 1, 1);
    public Color colorHighlight = new Color(1, 1, 0, 1);
    public Color colorLine;
    public int currentState = -1;
    public Tile tile;
    public AstarData data = new AstarData();

    public TileSet tileSet = null;

    // Use this for initialization
    void Start()
    {
        this.tile = GetComponent<Tile>();
        colorLine = GetComponent<LineRenderer>().material.color;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void resetState()
    {
        this.currentState = -1;
    }

    public void setTileState(int state)
    {
        Renderer rend = GetComponent<Renderer>();
        if (state == 0)
            rend.material.color = colorWhite;
        else if (state == 1)
            rend.material.color = colorBlack;
        else if (state == 2)
            GetComponent<LineRenderer>().material.color = colorHighlight;
        if (state == 1 || state == 0)
            this.currentState = state;
    }

    public void updateTileSet(Dictionary<TileState, TileSet> tileSets)
    {
        this.tileSet = tileSets[this];
        if (this.tileSet == null)
        {
            int layer = HexGridUtil.getHexIndexForPlayer(this.currentState, this);
            Debug.Log("tile layer: " + layer);
            foreach (Tile t in this.tile.neighbours)
            {
                TileState n = t.tileState;
                if (n.currentState == this.currentState)
                {
                    n.tileSet = tileSets[n];
                    if (this.tileSet == null)
                    {
                        n.tileSet.set.Add(this);
                        this.tileSet = n.tileSet;
                        if (tileSet.high < layer)
                            tileSet.high = layer;
                        if (tileSet.low > layer)
                            tileSet.low = layer;
                    }
                    else
                    {
                        // unify two tileSets
                        if(tileSet.size > n.tileSet.size) {
                            tileSet.unify(n.tileSet);
                            n.tileSet = tileSet;
                        }
                        else {
                            n.tileSet.unify(tileSet);
                            this.tileSet = n.tileSet;
                        }
                    }
                    tileSets[this] = this.tileSet;
                    tileSets[n] = n.tileSet;
                }
            }
            if (this.tileSet == null) // no neighbours is of same color
            {
                this.tileSet = new TileSet();
                this.tileSet.set.Add(this);
                this.tileSet.high = this.tileSet.low = layer;
                tileSets[this] = this.tileSet;
            }
        }
        else
        {
            Debug.LogError("IT Shouldn't be!");
        }
        Debug.Log(this.currentState + ": chainLength: " + tileSet.chainLength);
    }

    public void highlight()
    {
        GetComponent<LineRenderer>().material.color = colorHighlight;
    }

    public TileState highlightNeighbour(int direction, Dictionary<string, Tile> grid)
    {
        Hex h = Hex.Neighbor(new Hex(tile.index.x, tile.index.y, tile.index.z), direction);
        Tile t = HexGridUtil.TileAt(grid, h.q, h.r, h.s);
        TileState ts = t.GetComponent<TileState>();
        ts.highlight();
        return ts;
    }
    public TileState getNeighbour(int direction, Dictionary<string, Tile> grid)
    {
        Hex h = Hex.Neighbor(new Hex(tile.index.x, tile.index.y, tile.index.z), direction);
        Tile t = HexGridUtil.TileAt(grid, h.q, h.r, h.s);
        if (t == null)
            return null;
        TileState ts = t.GetComponent<TileState>();
        return ts;
    }

    public void resetHighLight()
    {
        GetComponent<LineRenderer>().material.color = colorLine;
    }
}
