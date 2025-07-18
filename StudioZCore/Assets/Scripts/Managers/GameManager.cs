using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [Header("References")]
    [SerializeField] private List<EpisodeData> episodes = new List<EpisodeData>();


    [Header("Current Game State")]
    private EpisodeData currentEpisode;
    private MiniGameData currentMiniGame;

    [Header("Camera Settings")]
    private float referenceWidth = 720f; 
    private float referenceOrthoSize = 5f; 


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
        SceneManager.LoadSceneAsync(currentMiniGame.sceneAsset.name, LoadSceneMode.Single);
    }

    private void AdaptCameraScale()
    {
        Camera cam = Camera.main;
        float currentAspect = (float)Screen.width / Screen.height;
        float referenceAspect = referenceWidth / (referenceWidth / (2 * referenceOrthoSize));
        cam.orthographicSize = referenceOrthoSize * (referenceAspect / currentAspect);
    }
}
