using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PlayerMCTS : Player
{

    public float testDelay = 0.0f;
    public int budget = 3;
    public int totalSimulation = 2;
    public float C = 1.44f;
    Thread thread;
    MCTS mcts;

    public int mode = 0;
    public override void StartPlay(GameController gc)
    {
        base.StartPlay(gc);
        StartCoroutine(StartGame(gc));
    }

    IEnumerator StartGame(GameController gc)
    {
        if (testDelay == 0)
            yield return 1;
        else
            yield return new WaitForSeconds(testDelay);

        mcts = new MCTS();
        mcts.budget = budget;
        mcts.totalSimulation = totalSimulation;
        isMoveComputed = false;
        mcts.C = C;
        MCTS.TreeSize = 0;
        MCTS.TreeSize = 0;
        thread = new Thread(() => mcts.UCTSearch(mode, this.MyTurnID, gc.grid.grid, gc.availableTiles, gc.player_tiles[gc.currentTurn], gc.player_tiles[1 - gc.currentTurn], callback));
        thread.Start();
    }
    private void OnDestroy()
    {
        if (mcts != null)
            mcts.destroy = true;
    }
    int maxTreeSize = 0;
    bool isMoveComputed = false;
    TileState ts = null;
    void callback(TileState ts)
    {
        //Debug.Log("Move selected: " + ts.tile.index);
        isMoveComputed = true;
        this.ts = ts;
        if (maxTreeSize < MCTS.TreeSize)
            maxTreeSize = MCTS.TreeSize;
        Debug.LogWarning(this.MyTurnID + " Current Move Max Tree Size: " + MCTS.TreeSize + " Max Tree Size: " + maxTreeSize);
    }

    private void Update()
    {
        if (isMoveComputed)
        {
            isMoveComputed = false;
            this.OnPlayFinished(ts);
        }
    }
}
