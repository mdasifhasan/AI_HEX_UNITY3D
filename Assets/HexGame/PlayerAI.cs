using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAI : Player {
    public float testDelay = 0.3f;
    public override void StartPlay(List<TileState> tileStates)
    {
        base.StartPlay(tileStates);
        StartCoroutine(StartGame(tileStates));
    }

    IEnumerator StartGame(List<TileState> tileStates)
    {
        yield return new WaitForSeconds(testDelay);

        TileState ts = null;
        while (true)
        {
            ts = tileStates[Random.Range(0, tileStates.Count)];
            if (ts.currentState == -1)
                break;
        }
        this.OnPlayFinished(ts);
    }
}
