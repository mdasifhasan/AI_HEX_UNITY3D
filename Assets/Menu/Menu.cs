using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour {

    public Dropdown ddPlayer1, ddPlayer2;

    int player1 = 0;
    int player2 = 0;
    void Start()
    {
        ddPlayer1.onValueChanged.AddListener(delegate {
            player1_changed(ddPlayer1);
        });

        ddPlayer2.onValueChanged.AddListener(delegate {
            player2_changed(ddPlayer2);
        });
    }

    void Destroy()
    {
        ddPlayer1.onValueChanged.RemoveAllListeners();
        ddPlayer2.onValueChanged.RemoveAllListeners();
    }

    private void player1_changed(Dropdown target)
    {
        //Debug.Log("selected: " + target.value);
        this.player1 = target.value;
    }

    private void player2_changed(Dropdown target)
    {
        //Debug.Log("selected: " + target.value);
        this.player2 = target.value;
    }

    public void play()
    {
        PlayerPrefs.SetInt("player1", player1);
        PlayerPrefs.SetInt("player2", player2);
        PlayerPrefs.Save();
        SceneManager.LoadScene(1);
    }

}
