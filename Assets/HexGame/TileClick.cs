using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileClick : MonoBehaviour {
    public delegate void OnTileClicked(TileState tile);
    public event OnTileClicked onTileClicked;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                TileState t = hit.transform.GetComponent<TileState>();
                if (t != null)
                    if (onTileClicked != null)
                        onTileClicked(t);
            }
        }
    }
}
