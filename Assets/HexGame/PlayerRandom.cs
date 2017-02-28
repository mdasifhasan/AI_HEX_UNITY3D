using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRandom : Player
{
    public float testDelay = 0.3f;
    public override void StartPlay(GameController gc)
    {
        base.StartPlay(gc);
        StartCoroutine(StartGame(gc));
    }

    IEnumerator StartGame(GameController gc)
    {
        yield return new WaitForSeconds(testDelay);

        TileState ts = null;
        ts = RandomMove(gc.availableTiles);

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
