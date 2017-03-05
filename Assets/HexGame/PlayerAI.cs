using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PlayerAI : Player
{
    
    public float testDelay = 0.3f;
    public int TimeBudget = 120;
    public int DepthBudget = 3;
    public bool randomMoveInLevels = false;
    public int weightChainLength = 1;
    public int weightHorizontalNeighbours = 0;
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
        ab.budget = TimeBudget;
        ab.depthBudget = DepthBudget;
        ab.randomMoveInLevels = randomMoveInLevels;
        isMoveComputed = false;
        ts = null;

        AstarData.weightChainLength = this.weightChainLength;
        AstarData.weightHorizontalNeighbours = this.weightHorizontalNeighbours;
        Thread thread = new Thread(() => ab.NextMove(gc.grid.grid, gc.player_tiles[gc.currentTurn], gc.player_tiles[1 - gc.currentTurn], gc.availableTiles, callback));
        thread.Start();
        

        //this.OnPlayFinished(ts);
    }

    bool isMoveComputed = false;
    TileState ts = null;
    void callback(TileState ts, int score)
    {
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
