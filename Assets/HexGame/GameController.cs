using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    public int currentTurn = 0;
    public Text textCurrentPlayer;
    public Player[] players;
    public List<TileState> tileStates;
    public Grid grid;

    // Use this for initialization
    void Start()
    {
        if (grid == null)
            grid = FindObjectOfType<Grid>();
        tileStates = new List<TileState>(FindObjectsOfType<TileState>());
        UpdateCurrentPlayerText();
        for (int i = 0; i < players.Length; i++)
        {
            Player p = players[i];
            p.OnPlayFinished += OnPlayFinished;
            p.MyTurnID = i;
        }
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        textCurrentPlayer.text = "Tossing";
        yield return new WaitForSeconds(.5f);
        textCurrentPlayer.text = "Tossing.";
        yield return new WaitForSeconds(.5f);
        textCurrentPlayer.text = "Tossing..";
        yield return new WaitForSeconds(.5f);
        textCurrentPlayer.text = "Tossing...";
        yield return new WaitForSeconds(.5f);
        this.currentTurn = Random.Range(0, 2);
        UpdateCurrentPlayerText();
        players[currentTurn].StartPlay(this.tileStates);
    }

    public int testDirection = 0;
    private void OnPlayFinished(TileState ts)
    {
        ts.setTileState(this.currentTurn);
        tileStates.Remove(ts);
        this.currentTurn = (this.currentTurn + 1) % 2;
        CubeIndex index = ts.GetComponent<Tile>().index;
        //for (int i = 0; i < 6; i++)
        //{
        //    Hex n = Hex.Neighbor(new Hex(index.x, index.y, index.z), i);
        //    Debug.Log(i + " Neightbour: " + n.q + ", " + n.r + ", " + n.s);
        //    Tile t = grid.TileAt(n.q, n.r, n.s);
        //    Debug.Log("Tile found: " + t, t);
        //    if (t != null)
        //        t.GetComponent<TileState>().setTileState(2);
        //}

        while (true)
        {
            Hex n = Hex.Neighbor(new Hex(index.x, index.y, index.z), testDirection);
            //Debug.Log(i + " Neightbour: " + n.q + ", " + n.r + ", " + n.s);
            Tile t = grid.TileAt(n.q, n.r, n.s);
            //Debug.Log("Tile found: " + t, t);
            if (t != null)
                t.GetComponent<TileState>().setTileState(2);
            else
                break;
            index = t.index;
        }


        // this is necessary to switch to next turn after each turn is finished
        isCurrentTurnUpdated = true;

    }

    private void UpdateCurrentPlayerText()
    {
        if (this.currentTurn == 0)
            textCurrentPlayer.text = "Turn: Player 1";
        else
            textCurrentPlayer.text = "Turn: Player 2";
    }


    bool isCurrentTurnUpdated = false;
    // Update is called once per frame
    void Update()
    {
        if (isCurrentTurnUpdated)
        {
            UpdateCurrentPlayerText();
            players[currentTurn].StartPlay(this.tileStates);
            isCurrentTurnUpdated = false;
        }
    }
}
