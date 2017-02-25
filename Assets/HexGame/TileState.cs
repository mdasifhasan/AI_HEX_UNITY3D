using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileState : MonoBehaviour {
    public Color colorBlack = new Color(0, 0, 0, 1);
    public Color colorWhite = new Color(1, 1, 1, 1);
    public int currentState = -1;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void setTileState(int state)
    {
        Renderer rend = GetComponent<Renderer>();
        if(state == 0)
            rend.material.color = colorWhite;
        else
            rend.material.color = colorBlack;
        this.currentState = state;
    }
}
