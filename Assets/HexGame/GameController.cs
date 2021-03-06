﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{

    public int currentTurn = 0;
    public Text textCurrentPlayer;
    public Text[] textPlayer1, textPlayer2;
    public Player[] players;
    public List<TileState> availableTiles;
    public List<TileState>[] player_tiles;
    public Grid grid;
    public GameObject uiGameOver;

    public Player[] list_player_1, list_player_2;

    // Use this for initialization
    void Start()
    {
        list_player_1[1].GetComponent<PlayerAI>().TimeBudget = PlayerPrefs.GetInt("p1_time_ab", 120);
        list_player_1[1].GetComponent<PlayerAI>().DepthBudget = PlayerPrefs.GetInt("p1_depth_ab", 4);
        list_player_2[1].GetComponent<PlayerAI>().TimeBudget = PlayerPrefs.GetInt("p2_time_ab", 120);
        list_player_2[1].GetComponent<PlayerAI>().DepthBudget = PlayerPrefs.GetInt("p2_depth_ab", 4);
        list_player_1[2].GetComponent<PlayerMCTS>().budget = PlayerPrefs.GetInt("p1_time_mcts", 5);
        list_player_1[2].GetComponent<PlayerMCTS>().totalSimulation = PlayerPrefs.GetInt("p1_sim_mcts", 5);
        list_player_2[2].GetComponent<PlayerMCTS>().budget = PlayerPrefs.GetInt("p2_time_mcts", 5);
        list_player_2[2].GetComponent<PlayerMCTS>().totalSimulation = PlayerPrefs.GetInt("p2_sim_mcts", 5);

        int player1 = PlayerPrefs.GetInt("player1");
        int player2 = PlayerPrefs.GetInt("player2");
        this.players[0] = this.list_player_1[player1];
        this.players[1] = this.list_player_2[player2];

        foreach(Text t in textPlayer1)
        {
            t.text = this.players[0].name;
        }

        foreach (Text t in textPlayer2)
        {
            t.text = this.players[1].name;
        }

        uiGameOver.SetActive(false);
        if (grid == null)
            grid = FindObjectOfType<Grid>();
        availableTiles = new List<TileState>(FindObjectsOfType<TileState>());
        player_tiles = new List<TileState>[2];
        player_tiles[0] = new List<TileState>();
        player_tiles[1] = new List<TileState>();
        UpdateCurrentPlayerText();
        for (int i = 0; i < players.Length; i++)
        {
            Player p = players[i];
            p.OnPlayFinished += OnPlayFinished;
            p.MyTurnID = i;
        }

        StartCoroutine(StartGame());
    }

    public InputField textStats;
    public GameObject stats;
    public void ShowStats()
    {
        textStats.text = Stats.getString();
        stats.SetActive(true);
    }
    public void CloseStats()
    {
        stats.SetActive(false);
    }

    public void Restart()
    {
        SceneManager.LoadScene(1);
    }
    public void Menu()
    {
        SceneManager.LoadScene(0);
    }

    public void TestEvaluationFunction()
    {
        //Debug.Log(HexGridUtil.evaluate(this.player_tiles[0], 0) + " vs " + HexGridUtil.evaluate(this.player_tiles[1], 1));
    }

    public Tile[] testMoves;

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
        //this.currentTurn = 1;
        //Debug.Log("Toss won by: " + this.currentTurn);
        tossWon = currentTurn;
        UpdateCurrentPlayerText();
        players[currentTurn].StartPlay(this);
    }

    public int tossWon = 0;
    public int testDirection = 0;
    private void OnPlayFinished(TileState ts)
    {
        ts.setTileState(this.currentTurn);
        this.player_tiles[this.currentTurn].Add(ts);
        availableTiles.Remove(ts);
        //int go = CheckGameOver();
        int go = HexGridUtil.CheckGameOver(0, player_tiles[0], player_tiles[1]);
        if (go != -1)
        {
            //Debug.Log("GameOver: " + go);
            uiGameOver.SetActive(true);
            if (this.currentTurn == 0)
                textCurrentPlayer.text = "Winner: " + players[0].name;
            else
                textCurrentPlayer.text = "Winner: " + players[1].name;
            Stats.RecordGame(players[0].gameObject.name, players[1].gameObject.name, tossWon, this.currentTurn);
            Stats.Print();
            //Restart();
            return;
        }
        //ts.updateTileSet();
        //Debug.LogError("ts chain length: " + ts.tileSet.GetRoot().chainLength);
        TestEvaluationFunction();
        this.currentTurn = (this.currentTurn + 1) % 2;
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

    private void UpdateCurrentPlayerText()
    {
        if (this.currentTurn == 0)
            textCurrentPlayer.text = "Turn: " + players[0].name;
        else
            textCurrentPlayer.text = "Turn: " + players[1].name;
    }


    bool isCurrentTurnUpdated = false;
    // Update is called once per frame
    void Update()
    {
        if (isCurrentTurnUpdated)
        {
            UpdateCurrentPlayerText();
            players[currentTurn].StartPlay(this);
            isCurrentTurnUpdated = false;
        }
    }
}
