using MiniGames;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private SceneAsset mainMenuScene;

    public void LoadMiniGameScene(MiniGameData miniGame)
    {
        GameManager.instance.currentMiniGame = miniGame;
        SceneManager.LoadSceneAsync(miniGame.miniGameScene.name, LoadSceneMode.Single);
    }

    public void LoadMainMenuScene()
    {
        SceneManager.LoadSceneAsync(mainMenuScene.name, LoadSceneMode.Single);
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }
}
