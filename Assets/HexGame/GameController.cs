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

        CheckGameOver();
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
        int go = CheckGameOver();
        if (go != -1)
        {
            Debug.Log("GameOver: " + go);
            return;
        }
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

        //while (true)
        //{
        //    Hex n = Hex.Neighbor(new Hex(index.x, index.y, index.z), testDirection);
        //    //Debug.Log(i + " Neightbour: " + n.q + ", " + n.r + ", " + n.s);
        //    Tile t = grid.TileAt(n.q, n.r, n.s);
        //    //Debug.Log("Tile found: " + t, t);
        //    if (t != null)
        //        t.GetComponent<TileState>().setTileState(2);
        //    else
        //        break;
        //    index = t.index;
        //}


        // this is necessary to switch to next turn after each turn is finished
        isCurrentTurnUpdated = true;

    }

    private int CheckGameOver()
    {
        //Debug.Log("CheckGameOver()");
        var lineStart = FractionalHex.HexLinedraw(new Hex(0, 0, 0), new Hex(7, 0, -7));
        var lineGoal = FractionalHex.HexLinedraw(new Hex(0, 7, -7), new Hex(7, 7, -14));
        if (CheckPlayerWin(0, lineStart, lineGoal))
            return 0;
        else
        {
            lineStart = FractionalHex.HexLinedraw(new Hex(0, 0, 0), new Hex(0, 7, -7));
            lineGoal = FractionalHex.HexLinedraw(new Hex(7, 0, -7), new Hex(7, 7, -14));
            if (CheckPlayerWin(1, lineStart, lineGoal))
                return 1;
        }
        return -1;

    }

    private bool CheckPlayerWin(int playerID, List<Hex> lineStart, List<Hex> lineGoal)
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
            Tile t = grid.TileAt(n.q, n.r, n.s);
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
            Tile t = grid.TileAt(n.q, n.r, n.s);
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
        while (frontier.Count > 0 )
        {
            c++;
            if( c > 1000)
            {
                Debug.LogError("Checking game over exceeding time limit");
                break;
            }
            TileState ts = frontier[0];
            frontier.RemoveAt(0);
            expanded.Add(ts);
            List<Tile> n = grid.Neighbours(ts.tile);
            foreach (Tile t in n)
            {
                TileState nts = t.GetComponent<TileState>();
                if (nts.currentState == playerID)
                {
                    if (!expanded.Contains(nts)) { 
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

    public void recurseCheckGameOver(TileState ts, int playerID)
    {
        int winner = -1;
        List<Tile> n = grid.Neighbours(ts.tile);
        List<TileState> frontier = new List<TileState>();
        foreach(Tile t in n)
        {
            TileState nts = t.GetComponent<TileState>();
            if (nts.currentState == playerID)
                frontier.Add(nts);
        }
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
