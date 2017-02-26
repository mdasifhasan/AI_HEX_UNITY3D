using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHuman : Player
{
    public TileClick tileClick;
    // Use this for initialization
    void Start ()
    {
        if (this.tileClick == null)
            tileClick = FindObjectOfType<TileClick>();
        this.tileClick.onTileClicked += TileClick_onTileClicked;
        this.tileClick.enabled = false;
    }

    public override void StartPlay(List<TileState> tileStates)
    {
        base.StartPlay(tileStates);
        this.tileClick.enabled = true;
    }

    private void TileClick_onTileClicked(TileState tile)
    {
        if (tile.currentState != -1)
            return;
        this.OnPlayFinished(tile);
        this.tileClick.enabled = false;
    }
}
