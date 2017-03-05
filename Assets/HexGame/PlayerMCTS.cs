using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PlayerMCTS : Player
{

    public float testDelay = 0.0f;
    public int budget = 3;
    Thread thread;
    MCTS mcts;
    public int weightChainLength = 1;
    public int weightHorizontalNeighbours = 0;
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
        isMoveComputed = false;
        AstarData.weightChainLength = weightChainLength;
        AstarData.weightHorizontalNeighbours = weightHorizontalNeighbours;
        thread = new Thread(() => mcts.UCTSearch(this.MyTurnID, gc.grid.grid, gc.availableTiles, gc.player_tiles[gc.currentTurn], gc.player_tiles[1 - gc.currentTurn], callback));
        thread.Start();
    }
    private void OnDestroy()
    {
        if (mcts != null)
            mcts.destroy = true;
    }

    bool isMoveComputed = false;
    TileState ts = null;
    void callback(TileState ts)
    {
        //Debug.Log("Move selected: " + ts.tile.index);
        isMoveComputed = true;
        this.ts = ts;
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
