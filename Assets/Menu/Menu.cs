using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour {

    public Dropdown ddPlayer1, ddPlayer2;
    public GameObject[] config_1;
    public GameObject[] config_2;


    int player1 = 0;
    int player2 = 0;

    private void Awake()
    {
        foreach (GameObject g in config_1)
            g.SetActive(false);
        foreach (GameObject g in config_2)
            g.SetActive(false);
    }

    void Start()
    {
        PlayerPrefs.SetInt("p1_time_ab", 120);
        PlayerPrefs.SetInt("p1_depth_ab", 4);
        PlayerPrefs.SetInt("p2_time_ab", 120);
        PlayerPrefs.SetInt("p2_depth_ab", 4);
        PlayerPrefs.SetInt("p1_time_mcts", 5);
        PlayerPrefs.SetInt("p1_sim_mcts", 5);
        PlayerPrefs.SetInt("p2_time_mcts", 5);
        PlayerPrefs.SetInt("p2_sim_mcts", 5);

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
        foreach (GameObject g in config_1)
            g.SetActive(false);
        config_1[player1].SetActive(true);
    }

    private void player2_changed(Dropdown target)
    {
        //Debug.Log("selected: " + target.value);
        this.player2 = target.value;
        foreach (GameObject g in config_2)
            g.SetActive(false);
        config_2[player2].SetActive(true);
    }

    public void play()
    {
        PlayerPrefs.SetInt("player1", player1);
        PlayerPrefs.SetInt("player2", player2);
        PlayerPrefs.Save();
        SceneManager.LoadScene(1);
    }

    public void p1_time_ab(string text)
    {
        Debug.Log("p1_time_ab: "  + text);
        int time = int.Parse(text);
        if (time <= 0)
            time = 120;
        PlayerPrefs.SetInt("p1_time_ab", time);
    }
    public void p1_depth_ab(string text)
    {
        Debug.Log("p1_depth_ab: " + text);
        int v = int.Parse(text);
        if (v <= 0)
            v = 4;
        PlayerPrefs.SetInt("p1_depth_ab", v);
    }


    public void p2_time_ab(string text)
    {
        Debug.Log("p2_time_ab: " + text);
        int time = int.Parse(text);
        if (time <= 0)
            time = 120;
        PlayerPrefs.SetInt("p2_time_ab", time);
    }
    public void p2_depth_ab(string text)
    {
        Debug.Log("p2_depth_ab: " + text);
        int v = int.Parse(text);
        if (v <= 0)
            v = 4;
        PlayerPrefs.SetInt("p2_depth_ab", v);
    }

    public void p1_time_mcts(string text)
    {
        Debug.Log("p1_time_mcts: " + text);
        int time = int.Parse(text);
        if (time <= 0)
            time = 5;
        PlayerPrefs.SetInt("p1_time_mcts", time);
    }
    public void p1_sim_mcts(string text)
    {
        Debug.Log("p1_sim_mcts: " + text);
        int v = int.Parse(text);
        if (v <= 0)
            v = 5;
        PlayerPrefs.SetInt("p1_sim_mcts", v);
    }

    public void p2_time_mcts(string text)
    {
        Debug.Log("p2_time_mcts: " + text);
        int time = int.Parse(text);
        if (time <= 0)
            time = 5;
        PlayerPrefs.SetInt("p2_time_mcts", time);
    }
    public void p2_sim_mcts(string text)
    {
        Debug.Log("p2_sim_mcts: " + text);
        int v = int.Parse(text);
        if (v <= 0)
            v = 5;
        PlayerPrefs.SetInt("p2_sim_mcts", v);
    }
}
