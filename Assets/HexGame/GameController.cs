using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public int currentTurn = 0;
    public Text textCurrentPlayer;
    public Player[] players;
    public List<TileState> tileStates;

    // Use this for initialization
    void Start () {
        tileStates = new List<TileState>( FindObjectsOfType<TileState>() );
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

    private void OnPlayFinished(TileState ts)
    {
        isCurrentTurnUpdated = true;
        ts.setTileState(this.currentTurn);
        tileStates.Remove(ts);
        this.currentTurn = (this.currentTurn + 1) % 2;
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
    void Update () {
        if (isCurrentTurnUpdated)
        {
            UpdateCurrentPlayerText();
            players[currentTurn].StartPlay(this.tileStates);
            isCurrentTurnUpdated = false;
        }
    }
}
