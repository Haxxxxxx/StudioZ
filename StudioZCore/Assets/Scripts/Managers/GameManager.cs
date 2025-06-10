using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("References")]
    [SerializeField] private List<EpisodeData> episodes = new List<EpisodeData>();
    private EpisodeData currentEpisode;
    private MiniGameData currentMiniGame;


    void Start()
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

    public void LoadMiniGame()
    {
        SceneManager.LoadSceneAsync(currentMiniGame.sceneName, LoadSceneMode.Single);
    }
}
