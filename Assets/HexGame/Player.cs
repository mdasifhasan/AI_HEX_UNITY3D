using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player : MonoBehaviour {
    public int MyTurnID = 0;
    public delegate void PlayFinished(TileState ts);
    public PlayFinished OnPlayFinished;
    public virtual void StartPlay(GameController gc)
    {

    }


}
