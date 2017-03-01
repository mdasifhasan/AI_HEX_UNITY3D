using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAI : Player
{
    public float testDelay = 0.3f;
    public override void StartPlay(GameController gc)
    {
        base.StartPlay(gc);
        StartCoroutine(StartGame(gc));
    }

    IEnumerator StartGame(GameController gc)
    {
        yield return 1;

        TileState ts = null;
        //ts = RandomMove(tileStates);

        AlphaBeta ab = new AlphaBeta(this.MyTurnID);
        ts = ab.NextMove(gc.grid.grid, gc.player_tiles[gc.currentTurn], gc.player_tiles[1 - gc.currentTurn], gc.availableTiles);

        this.OnPlayFinished(ts);
    }

    private static TileState RandomMove(List<TileState> tileStates)
    {
        TileState ts;
        while (true)
        {
            ts = tileStates[Random.Range(0, tileStates.Count)];
            if (ts.currentState == -1)
                break;
        }

        return ts;
    }
}
