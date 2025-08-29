using MiniGames;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SceneLoader))]
public class PauseHandler : MonoBehaviour
{
    private SceneLoader sceneLoader;
    private MiniGameBase miniGame;

    [Header("UI References")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject mask;
    public Button resumeBtn;
    [SerializeField] private Button restartBtn;
    [SerializeField] private Button mainMenuBtn;

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

    public void BS_TogglePause(bool isPaused)
    {
        mask.SetActive(isPaused);
        pauseMenu.SetActive(isPaused);
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

    public void SetMultiSetup()
    {
        restartBtn.interactable = false;
        mainMenuBtn.interactable = false;
    }
}
