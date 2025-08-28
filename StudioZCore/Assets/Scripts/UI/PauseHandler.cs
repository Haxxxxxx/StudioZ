using MiniGames;
using UnityEngine;

[RequireComponent(typeof(SceneLoader))]
public class PauseHandler : MonoBehaviour
{
    private SceneLoader sceneLoader;
    private MiniGameBase miniGame;

    [Header("UI References")]
    [SerializeField] private GameObject pauseMenu;

    private void Awake()
    {
        sceneLoader = GetComponent<SceneLoader>();
        pauseMenu.SetActive(false);
    }

    private void Start()
    {
        miniGame = FindAnyObjectByType<MiniGameBase>();
    }

    public void BS_TogglePause()
    {
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
