using UnityEngine;
using LitMotion.Animation;
using System.Collections;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(SceneLoader))]
public class GameResultHandler : MonoBehaviour
{
    private GameManager gameManager;
    private SceneLoader sceneLoader;

    [Header("UI References")]
    [SerializeField] private GameObject gameResultCanvas;
    [SerializeField] private LitMotionAnimation star1Animation;
    [SerializeField] private LitMotionAnimation star2Animation;
    [SerializeField] private LitMotionAnimation star3Animation;
    [SerializeField] private TextMeshProUGUI chronoText;
    [SerializeField] private Button nextBtn;

    private void Start()
    {
        gameManager = GameManager.instance;
        sceneLoader = GetComponent<SceneLoader>();
        CheckNextMiniGame();
    }

    private void CheckNextMiniGame()
    {
        if (gameManager && gameManager.currentEpisode.miniGames.IndexOf(gameManager.currentMiniGame) == gameManager.currentEpisode.miniGames.Count - 1)
        {
            nextBtn.gameObject.SetActive(false);
        }
        else
        {
            nextBtn.gameObject.SetActive(true);
        }
    }

    public void ShowGameResult(int stars, string chrono)
    {
        gameResultCanvas.SetActive(true);
        StartCoroutine(AnimateStars(stars));
        chronoText.text = chrono;
    }

    private IEnumerator AnimateStars(int stars)
    {
        star1Animation.Play();
        while (star1Animation.IsPlaying)
        {
            yield return null;
        }

        if (stars < 2) yield break;

        star2Animation.Play();
        while (star2Animation.IsPlaying)
        {
            yield return null;
        }

        if (stars < 3) yield break;

        star3Animation.Play();
    }

    #region Button Function

    public void BS_MainMenu()
    {
        sceneLoader.LoadMainMenuScene();
    }

    public void BS_Restart()
    {
        sceneLoader.ReloadCurrentScene();
    }

    public void BS_Next()
    {
        gameManager.currentMiniGame = gameManager.currentEpisode.miniGames[gameManager.currentEpisode.miniGames.IndexOf(gameManager.currentMiniGame) + 1];
        sceneLoader.LoadMiniGameScene(gameManager.currentMiniGame);
    }

    #endregion
}
