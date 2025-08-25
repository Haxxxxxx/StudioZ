using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MiniGames;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [Header("References")]
    [SerializeField] private List<EpisodeData> episodes = new List<EpisodeData>();


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
