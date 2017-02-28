using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileState : MonoBehaviour {
    public Color colorBlack = new Color(0, 0, 0, 1);
    public Color colorWhite = new Color(1, 1, 1, 1);
    public Color colorHighlight = new Color(1, 1, 0, 1);
    public Color colorLine;
    public int currentState = -1;
    public Tile tile;
    // Use this for initialization
    void Start () {
        this.tile = GetComponent<Tile>();
        colorLine = GetComponent<LineRenderer>().material.color;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void resetState()
    {
        this.currentState = -1;
    }

    public void setTileState(int state)
    {
        Renderer rend = GetComponent<Renderer>();
        if(state == 0)
            rend.material.color = colorWhite;
        else if (state == 1)
            rend.material.color = colorBlack;
        else if (state == 2)
            GetComponent<LineRenderer>().material.color = colorHighlight;
        if(state == 1 || state == 0)
            this.currentState = state;
    }

    public void highlight()
    {
        GetComponent<LineRenderer>().material.color = colorHighlight;
    }
    public void resetHighLight()
    {
        GetComponent<LineRenderer>().material.color = colorLine;
    }
}
