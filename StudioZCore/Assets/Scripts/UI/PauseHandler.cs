using MiniGames;
using UnityEngine;

[RequireComponent(typeof(SceneLoader))]
public class PauseHandler : MonoBehaviour
{
    private SceneLoader sceneLoader;
    private MiniGameBase miniGame;

    [Header("UI References")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject mask;

    private void Awake()
    {
        sceneLoader = GetComponent<SceneLoader>();
        pauseMenu.SetActive(false);
        mask.SetActive(false);
    }

    private void Start()
    {
        miniGame = FindAnyObjectByType<MiniGameBase>();
    }

    public void BS_TogglePause()
    {
        mask.SetActive(!pauseMenu.activeSelf);
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        bool isPaused = pauseMenu.activeSelf;
        if(isPaused)
        {
            miniGame.PauseMiniGame();
        }
        else
        {
            miniGame.UnPauseMiniGame();
        }
    }

    public void BS_RestartMiniGame()
    {
        sceneLoader.ReloadCurrentScene();
    }

    public void BS_ExitToMainMenu()
    {
        sceneLoader.LoadMainMenuScene();
    }
}
