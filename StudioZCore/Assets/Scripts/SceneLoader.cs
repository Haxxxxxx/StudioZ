using MiniGames;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadMiniGameScene(MiniGameData miniGame)
    {
        GameManager.instance.currentMiniGame = miniGame;
        SceneManager.LoadSceneAsync(miniGame.sceneName, LoadSceneMode.Single);
    }

    public void LoadMainMenuScene()
    {
        SceneManager.LoadSceneAsync("B2C_MainMenu", LoadSceneMode.Single);
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }
}
