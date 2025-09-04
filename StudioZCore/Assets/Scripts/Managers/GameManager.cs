using System.Collections.Generic;
using UnityEngine;
using MiniGames;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    public bool isMulti = false;

    [Header("References")]
    public List<EpisodeData> episodes = new List<EpisodeData>();


    [Header("Current Game State")]
    [NonSerialized] public EpisodeData currentEpisode;
    [NonSerialized] public MiniGameData currentMiniGame;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        currentEpisode = episodes[0];
        currentMiniGame = episodes[0].miniGames[0];
    }
}
