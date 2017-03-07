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
        AlphaBeta.TotalNodesEvaluated = 0;
        AlphaBeta.MaxTreeSize = 0;
        AlphaBeta.TreeSize = 0;
        Thread thread = new Thread(() => ab.NextMove(gc.grid.grid, gc.player_tiles[gc.currentTurn], gc.player_tiles[1 - gc.currentTurn], gc.availableTiles, callback));
        thread.Start();
        

        //this.OnPlayFinished(ts);
    }

    bool isMoveComputed = false;
    TileState ts = null;
    int maxTotalNodesEvaluated = 0;
    int maxTreeSize = 0;
    void callback(TileState ts, int score)
    {
        
        if (maxTotalNodesEvaluated < AlphaBeta.TotalNodesEvaluated)
            maxTotalNodesEvaluated = AlphaBeta.TotalNodesEvaluated;
        if (maxTreeSize < AlphaBeta.MaxTreeSize)
            maxTreeSize = AlphaBeta.MaxTreeSize;
        Debug.LogWarning(this.MyTurnID +" Total Node Evaluated: " + AlphaBeta.TotalNodesEvaluated + " Max total nodes evaluated: " + maxTotalNodesEvaluated);
        Debug.LogWarning(this.MyTurnID + " Current Move Max Tree Size: " + AlphaBeta.MaxTreeSize + " Max Tree Size: " + maxTreeSize);
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
