using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public int currentTurn = 0;
    public TileClick tileClick;

	// Use this for initialization
	void Start () {
        this.tileClick = GameObject.FindObjectOfType<TileClick>();
        this.tileClick.onTileClicked += TileClick_onTileClicked;
	}

    private void TileClick_onTileClicked(TileState tile)
    {
        if (tile.currentState != -1)
            return;
        tile.setTileState(this.currentTurn);
        this.currentTurn = (this.currentTurn + 1) % 2;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
